using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nop.Plugin.Payments.Payrexx.Domain
{
    /// <summary>
    /// Represents response from service
    /// </summary>
    public class Response<TResponseData> where TResponseData : ResponseData
    {
        /// <summary>
        /// Gets or sets a response status
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ResponseStatus? Status { get; set; }

        /// <summary>
        /// Gets or sets an error message
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets response payload
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public List<TResponseData> DataCollection { get; set; }

        /// <summary>
        /// Gets response data
        /// </summary>
        [JsonIgnore]
        public TResponseData Data => DataCollection?.FirstOrDefault();
    }
}