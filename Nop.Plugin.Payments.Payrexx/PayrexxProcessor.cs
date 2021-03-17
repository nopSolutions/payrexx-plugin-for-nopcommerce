using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IWebHelper _webHelper;
        private readonly PayrexxManager _payrexxManager;

        #endregion

        #region Ctor

        public PayrexxProcessor(CurrencySettings currencySettings,
            IActionContextAccessor actionContextAccessor,
            IAddressService addressService,
            ICountryService countryService,
            ICurrencyService currencyService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            ISettingService settingService,
            IStoreContext storeContext,
            IUrlHelperFactory urlHelperFactory,
            IWebHelper webHelper,
            PayrexxManager payrexxManager)
        {
            _currencySettings = currencySettings;
            _actionContextAccessor = actionContextAccessor;
            _addressService = addressService;
            _countryService = countryService;
            _currencyService = currencyService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _urlHelperFactory = urlHelperFactory;
            _webHelper = webHelper;
            _payrexxManager = payrexxManager;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the process payment result
        /// </returns>
        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult());
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //prepare URLs
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var successUrl = urlHelper
                .RouteUrl("CheckoutCompleted", new { orderId = postProcessPaymentRequest.Order.Id }, _webHelper.GetCurrentRequestProtocol());
            var failUrl = urlHelper
                .RouteUrl("OrderDetails", new { orderId = postProcessPaymentRequest.Order.Id }, _webHelper.GetCurrentRequestProtocol());

            //try to get previosly created invoice for this order
            var invoiceId = await _genericAttributeService
                .GetAttributeAsync<string>(postProcessPaymentRequest.Order, PayrexxDefaults.InvoiceIdAttribute)
                ?? string.Empty;
            if (!string.IsNullOrEmpty(invoiceId))
            {
                //whether payment link is already created and invoice is pending
                var (invoice, _) = await _payrexxManager.GetGatewayAsync(invoiceId);
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
            var currency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);

            var store = await _storeContext.GetCurrentStoreAsync();
            var billingAddress = await _addressService.GetAddressByIdAsync(postProcessPaymentRequest.Order.BillingAddressId);
            var country = await _countryService.GetCountryByIdAsync(billingAddress?.CountryId ?? 0);

            //prepare request to create gateway
            var request = new CreateGatewayRequest
            {
                TotalAmount = orderTotal,
                VatRate = null, //unknown
                CurrencyCode = currency?.CurrencyCode,
                Sku = null, //there may be several different items in the order
                Purpose = $"{store.Name}. Order #{postProcessPaymentRequest.Order.CustomOrderNumber}",
                SuccessRedirectUrl = successUrl,
                FailedRedirectUrl = failUrl,
                PaymentServiceProviders = null, //pass null to enable all available providers
                PaymentMethods = null, //pass null to enable all payment methods
                Authorized = false,
                Reserved = false,
                ReferenceId = postProcessPaymentRequest.Order.CustomOrderNumber,
                SkipResultPage = true,
                AdditionalFields = billingAddress == null ? null : new List<(string Name, string Value)>
                {
                    { ("forename", billingAddress.FirstName) },
                    { ("surname", billingAddress.LastName) },
                    { ("phone", new string(billingAddress.PhoneNumber?.Where(c => char.IsDigit(c)).ToArray())) },
                    { ("email", billingAddress.Email) },
                    { ("street", billingAddress.Address1) },
                    { ("place", billingAddress.City) },
                    { ("country", country?.TwoLetterIsoCode) },
                    { ("postcode", billingAddress.ZipPostalCode) }
                }
            };

            //create gateway
            var (gateway, errorMessage) = await _payrexxManager.CreateGatewayAsync(request);
            if (gateway?.PaymentLink == null || !string.IsNullOrEmpty(errorMessage))
            {
                _actionContextAccessor.ActionContext.HttpContext.Response.Redirect(failUrl);
                return;
            }

            //save invoice id for the further validation
            await _genericAttributeService.SaveAttributeAsync(postProcessPaymentRequest.Order, PayrexxDefaults.InvoiceIdAttribute, gateway.Id);

            //redirect to payment link
            _actionContextAccessor.ActionContext.HttpContext.Response.Redirect(gateway.PaymentLink);
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the capture payment result
        /// </returns>
        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capture method not supported" } });
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            return Task.FromResult(new RefundPaymentResult { Errors = new[] { "Refund method not supported" } });
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            return Task.FromResult(new VoidPaymentResult { Errors = new[] { "Void method not supported" } });
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the process payment result
        /// </returns>
        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return Task.FromResult(new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the rue - hide; false - display.
        /// </returns>
        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the additional handling fee
        /// </returns>
        public Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(decimal.Zero);
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            return Task.FromResult(true);
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of validating errors
        /// </returns>
        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return Task.FromResult<IList<string>>(new List<string>());
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the payment info holder
        /// </returns>
        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return Task.FromResult(new ProcessPaymentRequest());
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).RouteUrl(PayrexxDefaults.ConfigurationRouteName);
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <returns>View component name</returns>
        public string GetPublicViewComponentName()
        {
            return null;
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            await _settingService.SaveSettingAsync(new PayrexxSettings
            {
                RequestTimeout = 10
            });

            await _localizationService.AddLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Payments.Payrexx.Fields.InstanceName"] = "Instance name",
                ["Plugins.Payments.Payrexx.Fields.InstanceName.Hint"] = "Enter your Payrexx instance name. If you access your Payrexx payment page with example.payrexx.com, the name would be example.",
                ["Plugins.Payments.Payrexx.Fields.SecretKey"] = "API secret key",
                ["Plugins.Payments.Payrexx.Fields.SecretKey.Hint"] = "Enter your Payrexx API secret key.",
                ["Plugins.Payments.Payrexx.PaymentMethodDescription"] = "You will be redirected to Payrexx site to complete the payment"
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            await _settingService.DeleteSettingAsync<PayrexxSettings>();
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Payments.Payrexx");
            await base.UninstallAsync();
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("Plugins.Payments.Payrexx.PaymentMethodDescription");
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

        #endregion
    }
}