using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Payrexx.Domain
{
    /// <summary>
    /// Represents request to charge a pre-authorized / reserved transaction
    /// </summary>
    public class CaptureTransactionRequest : Request
    {
        /// <summary>
        /// Gets or sets the identifier 
        /// </summary>
        [JsonIgnore]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the total amount (in cents)
        /// </summary>
        [JsonProperty(PropertyName = "amount")]
        public int? TotalAmount { get; set; }

        /// <summary>
        /// Get a request path
        /// </summary>
        public override string Path => $"Transaction/{Id}";

        /// <summary>
        /// Get a request method
        /// </summary>
        public override string Method => HttpMethods.Post;
    }
}