using Nop.Core;

namespace Nop.Plugin.Payments.Payrexx
{
    /// <summary>
    /// Represents plugin constants
    /// </summary>
    public class PayrexxDefaults
    {
        /// <summary>
        /// Gets the plugin system name
        /// </summary>
        public static string SystemName => "Payments.Payrexx";

        /// <summary>
        /// Gets the user agent used to request third-party services
        /// </summary>
        public static string UserAgent => $"nopCommerce-{NopVersion.CURRENT_VERSION}";

        /// <summary>
        /// Gets the API service URL
        /// </summary>
        public static string ApiServiceUrl => "https://api.payrexx.com/v1.0/";

        /// <summary>
        /// Gets the parameter name of a request signature
        /// </summary>
        public static string RequestSignatureParameter => "ApiSignature";

        /// <summary>
        /// Gets the parameter name of a request instance
        /// </summary>
        public static string RequestInstanceParameter => "instance";

        /// <summary>
        /// Gets the configuration route name
        /// </summary>
        public static string ConfigurationRouteName => "Plugin.Payments.Payrexx.Configure";

        /// <summary>
        /// Gets the webhook route name
        /// </summary>
        public static string WebhookRouteName => "Plugin.Payments.Payrexx.Webhook";

        /// <summary>
        /// Gets the name of a generic attribute to store invoice identifier
        /// </summary>
        public static string InvoiceIdAttribute => "PayrexxInvoiceId";
    }
}