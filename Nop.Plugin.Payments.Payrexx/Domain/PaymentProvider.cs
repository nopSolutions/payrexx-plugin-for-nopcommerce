using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nop.Plugin.Payments.Payrexx.Domain
{
    /// <summary>
    /// Represents payment service provider details
    /// </summary>
    public class PaymentProvider : ResponseData
    {
        /// <summary>
        /// Gets or sets the payment service provider
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PaymentServiceProvider? PaymentServiceProvider { get; set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets list of payment mean names to display
        /// </summary>
        [JsonProperty(PropertyName = "paymentMethods", ItemConverterType = typeof(StringEnumConverter))]
        public List<PaymentMethodName> PaymentMethods { get; set; }

        /// <summary>
        /// Gets or sets list of active payment mean names to display
        /// </summary>
        [JsonProperty(PropertyName = "paymentMethods", ItemConverterType = typeof(StringEnumConverter))]
        public List<PaymentMethodName> ActivePaymentMethods { get; set; }
    }
}