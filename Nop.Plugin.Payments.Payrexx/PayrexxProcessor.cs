using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.Payrexx.Domain;
using Nop.Plugin.Payments.Payrexx.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;

namespace Nop.Plugin.Payments.Payrexx
{
    /// <summary>
    /// Represents Payrexx payment gateway processor
    /// </summary>
    public class PayrexxProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ICurrencyService _currencyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IWebHelper _webHelper;
        private readonly PayrexxManager _payrexxManager;
        private readonly PayrexxSettings _payrexxSettings;

        #endregion

        #region Ctor

        public PayrexxProcessor(CurrencySettings currencySettings,
            IActionContextAccessor actionContextAccessor,
            ICurrencyService currencyService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            ISettingService settingService,
            IStoreContext storeContext,
            IUrlHelperFactory urlHelperFactory,
            IWebHelper webHelper,
            PayrexxManager payrexxManager,
            PayrexxSettings payrexxSettings)
        {
            _currencySettings = currencySettings;
            _actionContextAccessor = actionContextAccessor;
            _currencyService = currencyService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _urlHelperFactory = urlHelperFactory;
            _webHelper = webHelper;
            _payrexxManager = payrexxManager;
            _payrexxSettings = payrexxSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult();
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //prepare URLs
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var successUrl = urlHelper
                .RouteUrl("CheckoutCompleted", new { orderId = postProcessPaymentRequest.Order.Id }, _webHelper.CurrentRequestProtocol);
            var failUrl = urlHelper
                .RouteUrl("OrderDetails", new { orderId = postProcessPaymentRequest.Order.Id }, _webHelper.CurrentRequestProtocol);

            //try to get previosly created invoice for this order
            var invoiceId = _genericAttributeService
                .GetAttribute<string>(postProcessPaymentRequest.Order, PayrexxDefaults.InvoiceIdAttribute) ?? string.Empty;
            if (!string.IsNullOrEmpty(invoiceId))
            {
                //whether payment link is already created and invoice is pending
                var (invoice, error) = _payrexxManager.GetGateway(invoiceId);
                if (invoice != null)
                {
                    if (invoice.PaymentLink != null && invoice.Status == InvoiceStatus.Pending)
                    {
                        _actionContextAccessor.ActionContext.HttpContext.Response.Redirect(invoice.PaymentLink);
                        return;
                    }

                    _actionContextAccessor.ActionContext.HttpContext.Response.Redirect(failUrl);
                    return;
                }
            }

            //the amount of money, in the smallest denomination of the currency indicated by currency, for example, when currency is USD, amount is in cents
            //most currencies consist of 100 units of smaller denomination, so we multiply the total by 100
            var orderTotal = (int)(Math.Round(postProcessPaymentRequest.Order.OrderTotal, 2) * 100);
            var currencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId)?.CurrencyCode;

            //prepare request to create gateway
            var request = new CreateGatewayRequest
            {
                TotalAmount = orderTotal,
                VatRate = null, //unknown
                CurrencyCode = currencyCode,
                Sku = null, //there may be several different items in the order
                Purpose = $"{_storeContext.CurrentStore.Name}. Order #{postProcessPaymentRequest.Order.CustomOrderNumber}",
                SuccessRedirectUrl = successUrl,
                FailedRedirectUrl = failUrl,
                PaymentServiceProviders = null, //pass null to enable all available providers
                PaymentMethods = null, //pass null to enable all payment methods
                Authorized = false,
                Reserved = false,
                ReferenceId = postProcessPaymentRequest.Order.CustomOrderNumber,
                SkipResultPage = true,
                AdditionalFields = new List<(string Name, string Value)>
                {
                    { ("forename", postProcessPaymentRequest.Order.BillingAddress?.FirstName) },
                    { ("surname", postProcessPaymentRequest.Order.BillingAddress?.LastName) },
                    { ("phone", postProcessPaymentRequest.Order.BillingAddress?.PhoneNumber) },
                    { ("email", postProcessPaymentRequest.Order.BillingAddress?.Email) },
                    { ("street", postProcessPaymentRequest.Order.BillingAddress?.Address1) },
                    { ("place", postProcessPaymentRequest.Order.BillingAddress?.City) },
                    { ("country", postProcessPaymentRequest.Order.BillingAddress?.Country?.TwoLetterIsoCode) },
                    { ("postcode", postProcessPaymentRequest.Order.BillingAddress?.ZipPostalCode) }
                }
            };

            //create gateway
            var (gateway, errorMessage) = _payrexxManager.CreateGateway(request);
            if (gateway?.PaymentLink == null || !string.IsNullOrEmpty(errorMessage))
            {
                _actionContextAccessor.ActionContext.HttpContext.Response.Redirect(failUrl);
                return;
            }

            //save invoice id for the further validation
            _genericAttributeService
                .SaveAttribute(postProcessPaymentRequest.Order, PayrexxDefaults.InvoiceIdAttribute, gateway.Id);

            //redirect to payment link
            _actionContextAccessor.ActionContext.HttpContext.Response.Redirect(gateway.PaymentLink);
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            return new CapturePaymentResult { Errors = new[] { "Capture method not supported" } };
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            return new RefundPaymentResult { Errors = new[] { "Refund method not supported" } };
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            return new VoidPaymentResult { Errors = new[] { "Void method not supported" } };
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return decimal.Zero;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            return true;
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            return new List<string>();
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            return new ProcessPaymentRequest();
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/Payrexx/Configure";
        }

        /// <summary>
        /// Gets a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <param name="viewComponentName">View component name</param>
        public string GetPublicViewComponentName()
        {
            return null;
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new PayrexxSettings());

            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Payrexx.Fields.InstanceName", "Instance name");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Payrexx.Fields.InstanceName.Hint", "Enter your Payrexx instance name. If you access your Payrexx payment page with example.payrexx.com, the name would be example.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Payrexx.Fields.SecretKey", "API secret key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Payrexx.Fields.SecretKey.Hint", "Enter your Payrexx API secret key.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.Payrexx.PaymentMethodDescription", "You will be redirected to Payrexx site to complete the payment");

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<PayrexxSettings>();

            //locales
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Payrexx.Fields.InstanceName");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Payrexx.Fields.InstanceName.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Payrexx.Fields.SecretKey");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Payrexx.Fields.SecretKey.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.Payrexx.PaymentMethodDescription");

            base.Uninstall();
        }

        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture => false;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund => false;

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund => false;

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid => false;

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo => true;

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription => _localizationService.GetResource("Plugins.Payments.Payrexx.PaymentMethodDescription");

        #endregion
    }
}