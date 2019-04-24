using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Payrexx.Domain
{
    /// <summary>
    /// Represents the webhook details
    /// </summary>
    public class Webhook
    {
        /// <summary>
        /// Gets or sets the transaction details
        /// </summary>
        [JsonProperty(PropertyName = "transaction")]
        public Transaction Transaction { get; set; }
    }
}