using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Payrexx.Domain
{
    /// <summary>
    /// Represents the metadata
    /// </summary>
    public class Metadata : ResponseData
    {
        /// <summary>
        /// Gets or sets the PayPal billing agreement identifier. 
        /// If you have activated the option "Create Billing Agreement" in the payment service provider settings of PayPal in your PayPal merchant backend, you will receive the created BillingAgreementId and you will be able to perform charges on behalf of your application
        /// </summary>
        [JsonProperty(PropertyName = "paypalBillingAgreementId")]
        public string PaypalBillingAgreementId { get; set; }
    }
}