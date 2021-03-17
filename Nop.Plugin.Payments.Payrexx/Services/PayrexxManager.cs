using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
        /// <returns>The asynchronous task whose result contains function result; error message if exists</returns>
        private async Task<(TResult Result, string ErrorMessage)> HandleFunctionAsync<TResult>(Func<Task<TResult>> function)
        {
            try
            {
                //ensure that plugin is configured
                if (!IsConfigured())
                    throw new NopException("Plugin not configured");

                //invoke function
                return (await function(), default);
            }
            catch (Exception exception)
            {
                //log errors
                var customer = await _workContext.GetCurrentCustomerAsync();
                var errorMessage = $"{PayrexxDefaults.SystemName} error: {Environment.NewLine}{exception.Message}";
                await _logger.ErrorAsync(errorMessage, exception, customer);

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
        /// <returns>The asynchronous task whose result contains response</returns>
        private async Task<TResponseData> HandleRequestAsync<TRequest, TResponseData>(TRequest request)
            where TRequest : Request where TResponseData : ResponseData
        {
            //execute request
            var response = await _httpClient.RequestAsync<TRequest, TResponseData>(request)
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
        /// <returns>The asynchronous task whose result contains function result; error message if exists</returns>
        public async Task<(bool Result, string ErrorMessage)> CheckSignatureAsync()
        {
            return await HandleFunctionAsync(async () =>
            {
                var response = await HandleRequestAsync<SignatureRequest, ResponseData>(new SignatureRequest());
                return response is not null;
            });
        }

        /// <summary>
        /// Get gateway details
        /// </summary>
        /// <param name="gatewayId">Gateway identifier</param>
        /// <returns>The asynchronous task whose result contains gateway; error message if exists</returns>
        public async Task<(Gateway Gateway, string ErrorMessage)> GetGatewayAsync(string gatewayId)
        {
            return await HandleFunctionAsync(async () =>
            {
                return await HandleRequestAsync<GetGatewayRequest, Gateway>(new GetGatewayRequest { Id = gatewayId });
            });
        }

        /// <summary>
        /// Create gateway
        /// </summary>
        /// <param name="request">Request details to create gateway</param>
        /// <returns>The asynchronous task whose result contains gateway; error message if exists</returns>
        public async Task<(Gateway Gateway, string ErrorMessage)> CreateGatewayAsync(CreateGatewayRequest request)
        {
            return await HandleFunctionAsync(async () =>
            {
                return await HandleRequestAsync<CreateGatewayRequest, Gateway>(request);
            });
        }

        /// <summary>
        /// Capture transaction
        /// </summary>
        /// <param name="transactionId">Transaction identifer</param>
        /// <param name="amount">Amount to capture</param>
        /// <returns>The asynchronous task whose result contains transaction; error message if exists</returns>
        public async Task<(Transaction Transaction, string ErrorMessage)> CaptureTransactionAsync(string transactionId, int amount)
        {
            return await HandleFunctionAsync(async () =>
            {
                var request = new CaptureTransactionRequest { Id = transactionId, TotalAmount = amount };
                return await HandleRequestAsync<CaptureTransactionRequest, Transaction>(request);
            });
        }

        /// <summary>
        /// Handle webhook transaction
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleWebhookTransactionAsync(HttpContext context)
        {
            await HandleFunctionAsync(async () =>
            {
                //get transaction details
                using var streamReader = new StreamReader(context.Request.Body, Encoding.Default);
                var message = await streamReader.ReadToEndAsync();
                var transaction = JsonConvert.DeserializeObject<Webhook>(message)?.Transaction
                    ?? throw new NopException("Webhook error - Transaction not found");

                //whether there is an invoice
                if (transaction.Invoice is null)
                    throw new NopException("Webhook error - Invoice not found");

                //try to get an order for this invoice
                var order = await _orderService.GetOrderByCustomOrderNumberAsync(transaction.Invoice.ReferenceId)
                    ?? throw new NopException("Webhook error - Order not found");

                //validate received invoice
                var invoiceId = await _genericAttributeService.GetAttributeAsync<string>(order, PayrexxDefaults.InvoiceIdAttribute)
                    ?? string.Empty;
                if (!invoiceId.Equals(transaction.Invoice.InvoiceId))
                    throw new NopException("Webhook error - Invoice ids don't match");

                //add order note
                await _orderService.InsertOrderNoteAsync(new OrderNote
                {
                    OrderId = order.Id,
                    Note = $"Webhook details: {Environment.NewLine}{message}",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });

                //get invoice from created gateway
                var (gateway, errorMessage) = await GetGatewayAsync(transaction.Invoice.InvoiceId);
                if (gateway == null)
                    throw new NopException("Webhook error - Payment gateway not found");

                //check invoice status
                switch (gateway.Status)
                {
                    case InvoiceStatus.Pending:
                        order.OrderStatus = OrderStatus.Pending;
                        await _orderService.UpdateOrderAsync(order);
                        await _orderProcessingService.CheckOrderStatusAsync(order);
                        break;

                    case InvoiceStatus.Confirmed:
                        if (gateway.TotalAmount == Math.Round(order.OrderTotal, 2) * 100 && _orderProcessingService.CanMarkOrderAsPaid(order))
                        {
                            order.CaptureTransactionId = gateway.Id;
                            await _orderService.UpdateOrderAsync(order);
                            await _orderProcessingService.MarkOrderAsPaidAsync(order);
                        }
                        break;

                    case InvoiceStatus.Authorized:
                    case InvoiceStatus.Reserved:
                        if (gateway.TotalAmount == Math.Round(order.OrderTotal, 2) * 100 && _orderProcessingService.CanMarkOrderAsAuthorized(order))
                        {
                            order.AuthorizationTransactionId = gateway.Id;
                            await _orderService.UpdateOrderAsync(order);
                            await _orderProcessingService.MarkAsAuthorizedAsync(order);
                        }
                        break;

                    case InvoiceStatus.Refunded:
                        if (await _orderProcessingService.CanRefundAsync(order))
                            await _orderProcessingService.RefundAsync(order);
                        break;

                    case InvoiceStatus.PartiallyRefunded:
                        var amountToRefund = gateway.TotalAmount ?? decimal.Zero;
                        if (await _orderProcessingService.CanPartiallyRefundAsync(order, amountToRefund))
                            await _orderProcessingService.PartiallyRefundAsync(order, amountToRefund);
                        break;

                    case InvoiceStatus.Cancelled:
                    case InvoiceStatus.Declined:
                    case InvoiceStatus.Chargeback:
                        if (_orderProcessingService.CanCancelOrder(order))
                            await _orderProcessingService.CancelOrderAsync(order, true);
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