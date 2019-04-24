using Nop.Core;

namespace Nop.Plugin.Payments.Payrexx
{
    /// <summary>
    /// Represents plugin constants
    /// </summary>
    public class PayrexxDefaults
    {
        /// <summary>
        /// Payrexx payment method system name
        /// </summary>
        public static string SystemName => "Payments.Payrexx";

        /// <summary>
        /// User agent used to request Payrexx services
        /// </summary>
        public static string UserAgent => $"nopCommerce-{NopVersion.CurrentVersion}";

        /// <summary>
        /// Payrexx API service URL
        /// </summary>
        public static string ApiServiceUrl => "https://api.payrexx.com/v1.0/";

        /// <summary>
        /// Parameter name of a request signature
        /// </summary>
        public static string RequestSignatureParameter => "ApiSignature";

        /// <summary>
        /// Parameter name of a request instance
        /// </summary>
        public static string RequestInstanceParameter => "instance";

        /// <summary>
        /// Webhook route name
        /// </summary>
        public static string WebhookRouteName => "Plugin.Payments.Payrexx.Webhook";

        /// <summary>
        /// Generic attribute to store invoice identifier
        /// </summary>
        public static string InvoiceIdAttribute => "PayrexxInvoiceId";
    }
}