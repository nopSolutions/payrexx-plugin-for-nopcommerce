using System;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.Payrexx.Domain;
using Nop.Plugin.Payments.Payrexx.Services;
using Nop.Services.Common;
using Nop.Services.Orders;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.Payrexx.Controllers
{
    public class PayrexxWebhookController : BaseController
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly PayrexxManager _payrexxManager;

        #endregion

        #region Ctor

        public PayrexxWebhookController(IGenericAttributeService genericAttributeService,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            PayrexxManager payrexxManager)
        {
            _genericAttributeService = genericAttributeService;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _payrexxManager = payrexxManager;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Pending
        /// </summary>
        /// <param name="order">Order</param>
        private void MarkOrderAsPending(Order order)
        {
            order.OrderStatus = OrderStatus.Pending;
            _orderService.UpdateOrder(order);
            _orderProcessingService.CheckOrderStatus(order);
        }

        /// <summary>
        /// Paid
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="gateway"></param>
        private void MarkOrderAsPaid(Order order, Gateway gateway)
        {
            //compare amounts
            var orderTotal = Math.Round(order.OrderTotal, 2) * 100;
            if (orderTotal != gateway.TotalAmount)
                return;

            //all is ok, so paid order
            if (_orderProcessingService.CanMarkOrderAsPaid(order))
            {
                order.CaptureTransactionId = gateway.Id;
                _orderService.UpdateOrder(order);
                _orderProcessingService.MarkOrderAsPaid(order);
            }
        }

        /// <summary>
        /// Authorize
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="gateway"></param>
        private void MarkOrderAsAuthorized(Order order, Gateway gateway)
        {
            //compare amounts
            var orderTotal = Math.Round(order.OrderTotal, 2) * 100;
            if (orderTotal != gateway.TotalAmount)
                return;

            //all is ok, so authorize order
            if (_orderProcessingService.CanMarkOrderAsAuthorized(order))
            {
                order.AuthorizationTransactionId = gateway.Id;
                _orderService.UpdateOrder(order);
                _orderProcessingService.MarkAsAuthorized(order);
            }
        }

        /// <summary>
        /// Cancel
        /// </summary>
        /// <param name="order">Order</param>
        private void MarkOrderAsCancelled(Order order)
        {
            if (_orderProcessingService.CanCancelOrder(order))
                _orderProcessingService.CancelOrder(order, true);
        }

        /// <summary>
        /// Refund
        /// </summary>
        /// <param name="order">Order</param>
        private void MarkOrderAsRefunded(Order order)
        {
            if (_orderProcessingService.CanRefund(order))
                _orderProcessingService.Refund(order);
        }

        /// <summary>
        /// Partially refund
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="amountToRefund">Amount to refund</param>
        private void MarkOrderAsPartiallyRefunded(Order order, decimal amountToRefund)
        {
            if (_orderProcessingService.CanPartiallyRefund(order, amountToRefund))
                _orderProcessingService.PartiallyRefund(order, amountToRefund);
        }

        #endregion

        #region Methods

        [HttpPost]
        public IActionResult WebhookHandler()
        {
            try
            {
                //get transaction details
                var (transaction, rawRequestString) = _payrexxManager.GetTransactionFromWebhookRequest(HttpContext.Request);
                if (transaction == null)
                    return BadRequest();

                //whether there is an invoice
                if (transaction.Invoice == null)
                    return Ok();

                //try to get an order for this invoice
                var order = _orderService.GetOrderByCustomOrderNumber(transaction.Invoice.ReferenceId);
                if (order == null)
                    return Ok();

                //validate received invoice
                var invoiceId = _genericAttributeService.GetAttribute<string>(order, PayrexxDefaults.InvoiceIdAttribute) ?? string.Empty;
                if (!invoiceId.Equals(transaction.Invoice.InvoiceId))
                    return Ok();

                //add order note
                order.OrderNotes.Add(new OrderNote()
                {
                    Note = $"Webhook details: {Environment.NewLine}{rawRequestString}",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);

                //get invoice from created gateway
                var (invoice, errorMessage) = _payrexxManager.GetGateway(transaction.Invoice.InvoiceId);
                if (invoice == null)
                    return Ok();

                //check invoice status
                switch (invoice.Status)
                {
                    case InvoiceStatus.Pending:
                        MarkOrderAsPending(order);
                        break;

                    case InvoiceStatus.Confirmed:
                        MarkOrderAsPaid(order, invoice);
                        break;

                    case InvoiceStatus.Authorized:
                    case InvoiceStatus.Reserved:
                        MarkOrderAsAuthorized(order, invoice);
                        break;

                    case InvoiceStatus.Refunded:
                        MarkOrderAsRefunded(order);
                        break;

                    case InvoiceStatus.PartiallyRefunded:
                        MarkOrderAsPartiallyRefunded(order, invoice.TotalAmount ?? decimal.Zero);
                        break;

                    case InvoiceStatus.Cancelled:
                    case InvoiceStatus.Declined:
                    case InvoiceStatus.Chargeback:
                        MarkOrderAsCancelled(order);
                        break;

                    case InvoiceStatus.Error:
                    default:
                        break;
                }
            }
            catch { }

            return Ok();
        }

        #endregion
    }
}