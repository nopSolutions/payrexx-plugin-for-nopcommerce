using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.Payrexx.Domain;
using Nop.Services.Common;
using Nop.Services.Logging;
using Nop.Services.Orders;

namespace Nop.Plugin.Payments.Payrexx.Services
{
    /// <summary>
    /// Represents the service manager
    /// </summary>
    public class PayrexxManager
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILogger _logger;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;
        private readonly PayrexxHttpClient _httpClient;
        private readonly PayrexxSettings _payrexxSettings;

        #endregion

        #region Ctor

        public PayrexxManager(IGenericAttributeService genericAttributeService,
            ILogger logger,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IWorkContext workContext,
            PayrexxHttpClient httpClient,
            PayrexxSettings payrexxSettings)
        {
            _genericAttributeService = genericAttributeService;
            _logger = logger;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _workContext = workContext;
            _httpClient = httpClient;
            _payrexxSettings = payrexxSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Handle function and get result
        /// </summary>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="function">Function</param>
        /// <returns>Result; error message if exists</returns>
        private (TResult Result, string ErrorMessage) HandleFunction<TResult>(Func<TResult> function)
        {
            try
            {
                //ensure that plugin is configured
                if (!IsConfigured())
                    throw new NopException("Plugin not configured");

                //invoke function
                return (function(), default);
            }
            catch (Exception exception)
            {
                //log errors
                var errorMessage = $"{PayrexxDefaults.SystemName} error: {Environment.NewLine}{exception.Message}";
                _logger.Error(errorMessage, exception, _workContext.CurrentCustomer);

                return (default, errorMessage);
            }
        }

        /// <summary>
        /// Check whether the plugin is configured
        /// </summary>
        /// <returns>Result</returns>
        private bool IsConfigured()
        {
            //instance name and secret key are required to request services
            return !string.IsNullOrEmpty(_payrexxSettings.InstanceName) && !string.IsNullOrEmpty(_payrexxSettings.SecretKey);
        }

        /// <summary>
        /// Handle request and get response
        /// </summary>
        /// <typeparam name="TRequest">Request type</typeparam>
        /// <typeparam name="TResponseData">Response data type</typeparam>
        /// <param name="request">Request</param>
        /// <returns>Response</returns>
        private TResponseData HandleRequest<TRequest, TResponseData>(TRequest request) where TRequest : Request where TResponseData : ResponseData
        {
            //execute request
            var response = _httpClient.RequestAsync<TRequest, TResponseData>(request)?.Result
                ?? throw new NopException("No response from service");

            //check whether request was successfull
            if (response.Status != ResponseStatus.Success)
                throw new NopException($"Request status - {response.Status}. {Environment.NewLine}{response.ErrorMessage}");

            return response.Data;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Check whether signature based on plugin credentials is valid
        /// </summary>
        /// <returns>Result; error message if exists</returns>
        public (bool Result, string ErrorMessage) CheckSignature()
        {
            return HandleFunction(() => HandleRequest<SignatureRequest, ResponseData>(new SignatureRequest()) != null);
        }

        /// <summary>
        /// Get gateway details
        /// </summary>
        /// <param name="gatewayId">Gateway identifier</param>
        /// <returns>Gateway; error message if exists</returns>
        public (Gateway Gateway, string ErrorMessage) GetGateway(string gatewayId)
        {
            return HandleFunction(() => HandleRequest<GetGatewayRequest, Gateway>(new GetGatewayRequest { Id = gatewayId }));
        }

        /// <summary>
        /// Create gateway
        /// </summary>
        /// <param name="request">Request details to create gateway</param>
        /// <returns>Gateway; error message if exists</returns>
        public (Gateway Gateway, string ErrorMessage) CreateGateway(CreateGatewayRequest request)
        {
            return HandleFunction(() => HandleRequest<CreateGatewayRequest, Gateway>(request));
        }

        /// <summary>
        /// Capture transaction
        /// </summary>
        /// <param name="transactionId">Transaction identifer</param>
        /// <param name="amount">Amount to capture</param>
        /// <returns>Transaction; error message if exists</returns>
        public (Transaction Transaction, string ErrorMessage) CaptureTransaction(string transactionId, int amount)
        {
            return HandleFunction(() =>
            {
                var request = new CaptureTransactionRequest { Id = transactionId, TotalAmount = amount };
                return HandleRequest<CaptureTransactionRequest, Transaction>(request);
            });
        }

        /// <summary>
        /// Handle webhook transaction
        /// </summary>
        /// <param name="context">HTTP context</param>
        public void HandleWebhookTransaction(HttpContext context)
        {
            HandleFunction(() =>
            {
                //get transaction details
                var syncIOFeature = context.Features.Get<IHttpBodyControlFeature>();
                if (syncIOFeature != null)
                    syncIOFeature.AllowSynchronousIO = true;
                using var streamReader = new StreamReader(context.Request.Body, Encoding.Default);
                var message = streamReader.ReadToEnd();
                var transaction = JsonConvert.DeserializeObject<Webhook>(message)?.Transaction;
                if (transaction == null)
                    throw new NopException("Webhook error - Transaction not found");

                //whether there is an invoice
                if (transaction.Invoice == null)
                    throw new NopException("Webhook error - Invoice not found");

                //try to get an order for this invoice
                var order = _orderService.GetOrderByCustomOrderNumber(transaction.Invoice.ReferenceId);
                if (order == null)
                    throw new NopException("Webhook error - Order not found");

                //validate received invoice
                var invoiceId = _genericAttributeService.GetAttribute<string>(order, PayrexxDefaults.InvoiceIdAttribute) ?? string.Empty;
                if (!invoiceId.Equals(transaction.Invoice.InvoiceId))
                    throw new NopException("Webhook error - Invoice ids don't match");

                //add order note
                _orderService.InsertOrderNote(new OrderNote
                {
                    OrderId = order.Id,
                    Note = $"Webhook details: {Environment.NewLine}{message}",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });

                //get invoice from created gateway
                var (gateway, errorMessage) = GetGateway(transaction.Invoice.InvoiceId);
                if (gateway == null)
                    throw new NopException("Webhook error - Payment gateway not found");

                //check invoice status
                switch (gateway.Status)
                {
                    case InvoiceStatus.Pending:
                        order.OrderStatus = OrderStatus.Pending;
                        _orderService.UpdateOrder(order);
                        _orderProcessingService.CheckOrderStatus(order);
                        break;

                    case InvoiceStatus.Confirmed:
                        if (gateway.TotalAmount == Math.Round(order.OrderTotal, 2) * 100 && _orderProcessingService.CanMarkOrderAsPaid(order))
                        {
                            order.CaptureTransactionId = gateway.Id;
                            _orderService.UpdateOrder(order);
                            _orderProcessingService.MarkOrderAsPaid(order);
                        }
                        break;

                    case InvoiceStatus.Authorized:
                    case InvoiceStatus.Reserved:
                        if (gateway.TotalAmount == Math.Round(order.OrderTotal, 2) * 100 && _orderProcessingService.CanMarkOrderAsAuthorized(order))
                        {
                            order.AuthorizationTransactionId = gateway.Id;
                            _orderService.UpdateOrder(order);
                            _orderProcessingService.MarkAsAuthorized(order);
                        }
                        break;

                    case InvoiceStatus.Refunded:
                        if (_orderProcessingService.CanRefund(order))
                            _orderProcessingService.Refund(order);
                        break;

                    case InvoiceStatus.PartiallyRefunded:
                        var amountToRefund = gateway.TotalAmount ?? decimal.Zero;
                        if (_orderProcessingService.CanPartiallyRefund(order, amountToRefund))
                            _orderProcessingService.PartiallyRefund(order, amountToRefund);
                        break;

                    case InvoiceStatus.Cancelled:
                    case InvoiceStatus.Declined:
                    case InvoiceStatus.Chargeback:
                        if (_orderProcessingService.CanCancelOrder(order))
                            _orderProcessingService.CancelOrder(order, true);
                        break;

                    case InvoiceStatus.Error:
                    default:
                        break;
                }

                return true;
            });
        }

        #endregion
    }
}