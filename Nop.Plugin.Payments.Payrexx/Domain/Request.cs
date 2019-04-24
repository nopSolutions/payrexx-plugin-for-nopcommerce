using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Payrexx.Domain
{
    /// <summary>
    /// Represents base request to service
    /// </summary>
    public abstract class Request
    {
        /// <summary>
        /// Get a request path
        /// </summary>
        [JsonIgnore]
        public abstract string Path { get; }

        /// <summary>
        /// Get a request method
        /// </summary>
        [JsonIgnore]
        public abstract string Method { get; }
    }
}