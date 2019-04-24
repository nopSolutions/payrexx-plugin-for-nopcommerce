using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nop.Plugin.Payments.Payrexx.Domain
{
    /// <summary>
    /// Represents request to create gateway
    /// </summary>
    public class CreateGatewayRequest : Request
    {
        /// <summary>
        /// Gets or sets the total amount (in cents)
        /// </summary>
        [JsonProperty(PropertyName = "amount")]
        public int? TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the tax VAT rate percentage
        /// </summary>
        [JsonProperty(PropertyName = "vatRate")]
        public decimal? VatRate { get; set; }

        /// <summary>
        /// Gets or sets the ISO currency code
        /// </summary>
        [JsonProperty(PropertyName = "currency")]
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the SKU (product stock keeping unit)
        /// </summary>
        [JsonProperty(PropertyName = "sku")]
        public string Sku { get; set; }

        /// <summary>
        /// Gets or sets the purpose of the payment
        /// </summary>
        [JsonProperty(PropertyName = "purpose")]
        public string Purpose { get; set; }

        /// <summary>
        /// Gets or sets URL to redirect to after successful payment
        /// </summary>
        [JsonProperty(PropertyName = "successRedirectUrl")]
        public string SuccessRedirectUrl { get; set; }

        /// <summary>
        /// Gets or sets URL to redirect to after failed payment
        /// </summary>
        [JsonProperty(PropertyName = "failedRedirectUrl")]
        public string FailedRedirectUrl { get; set; }

        /// <summary>
        /// Gets or sets list of payment service providers, if empty all available PSPs are provided
        /// </summary>
        [JsonProperty(PropertyName = "psp", ItemConverterType = typeof(StringEnumConverter))]
        public List<PaymentServiceProvider> PaymentServiceProviders { get; set; }

        /// <summary>
        /// Gets or sets list of payment mean names to display
        /// </summary>
        [JsonProperty(PropertyName = "pm", ItemConverterType = typeof(StringEnumConverter))]
        public List<PaymentMethodName> PaymentMethods { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the payment is authorized (charge payment manually at a later date (type authorization))
        /// </summary>
        [JsonProperty(PropertyName = "preAuthorization")]
        [JsonConverter(typeof(BoolConverter))]
        public bool Authorized { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the payment is reserved (charge payment manually at a later date (type reservation))
        /// </summary>
        [JsonProperty(PropertyName = "reservation")]
        [JsonConverter(typeof(BoolConverter))]
        public bool Reserved { get; set; }

        /// <summary>
        /// Gets or sets an internal reference identifier 
        /// </summary>
        [JsonProperty(PropertyName = "referenceId")]
        public string ReferenceId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to skip result page and directly redirect to success or failed URL
        /// </summary>
        [JsonProperty(PropertyName = "skipResultPage")]
        [JsonConverter(typeof(BoolConverter))]
        public bool SkipResultPage { get; set; }

        /// <summary>
        /// Gets or sets additional fields
        /// </summary>
        [JsonIgnore]
        public List<(string Name, string Value)> AdditionalFields { get; set; }

        /// <summary>
        /// Get a request path
        /// </summary>
        public override string Path => $"Gateway/";

        /// <summary>
        /// Get a request method
        /// </summary>
        public override string Method => HttpMethods.Post;
    }
}