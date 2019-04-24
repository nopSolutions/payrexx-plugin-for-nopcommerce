using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nop.Plugin.Payments.Payrexx.Domain
{
    /// <summary>
    /// Represents request to create invoice
    /// </summary>
    public class CreateInvoiceRequest : Request
    {
        /// <summary>
        /// Gets or sets the page title which will be shown on the payment page
        /// </summary>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a description which will be shown on the payment page
        /// </summary>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the payment service provider which should be used for the payment
        /// </summary>
        [JsonProperty(PropertyName = "psp")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PaymentServiceProvider? PaymentServiceProvider { get; set; }

        /// <summary>
        /// Gets or sets the internal reference identifier
        /// </summary>
        [JsonProperty(PropertyName = "referenceId")]
        public string ReferenceId { get; set; }

        /// <summary>
        /// Gets or sets the purpose of the payment 
        /// </summary>
        [JsonProperty(PropertyName = "purpose")]
        public string Purpose { get; set; }

        /// <summary>
        /// Gets or sets the total amount (in cents)
        /// </summary>
        [JsonProperty(PropertyName = "amount")]
        public int TotalAmount { get; set; }

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
        /// Gets or sets an internal name of the payment page 
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to hide the whole contact fields section on invoice page
        /// </summary>
        [JsonProperty(PropertyName = "hideFields")]
        [JsonConverter(typeof(BoolConverter))]
        public bool HideAdditionalFields { get; set; }

        /// <summary>
        /// Gets or sets additional fields
        /// </summary>
        [JsonIgnore]
        public List<(string Name, string Value)> AdditionalFields { get; set; }

        /// <summary>
        /// Get a request path
        /// </summary>
        public override string Path => $"Invoice/";

        /// <summary>
        /// Get a request method
        /// </summary>
        public override string Method => HttpMethods.Post;
    }
}