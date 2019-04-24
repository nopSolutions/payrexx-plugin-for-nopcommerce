using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Payrexx.Domain
{
    /// <summary>
    /// Represents request to get gateway
    /// </summary>
    public class GetGatewayRequest : Request
    {
        /// <summary>
        /// Gets or sets the identifier of the gateway to get
        /// </summary>
        [JsonIgnore]
        public string Id { get; set; }

        /// <summary>
        /// Get a request path
        /// </summary>
        public override string Path => $"Gateway/{Id}/";

        /// <summary>
        /// Get a request method
        /// </summary>
        public override string Method => HttpMethods.Get;
    }
}