using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nop.Plugin.Payments.Payrexx.Domain
{
    /// <summary>
    /// Represents the transaction
    /// </summary>
    public class Transaction : ResponseData
    {
        /// <summary>
        /// Gets or sets the identifier 
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the public transaction identifier 
        /// </summary>
        [JsonProperty(PropertyName = "uuid")]
        public string Uuid { get; set; }

        /// <summary>
        /// Gets or sets the date of creation 
        /// </summary>
        [JsonProperty(PropertyName = "time")]
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the status 
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public InvoiceStatus? Status { get; set; }

        /// <summary>
        /// Gets or sets the language code
        /// </summary>
        [JsonProperty(PropertyName = "lang")]
        public string LanguageCode { get; set; }

        /// <summary>
        /// Gets or sets the payment service provider name
        /// </summary>
        [JsonProperty(PropertyName = "psp")]
        public string PaymentServiceProviderName { get; set; }

        /// <summary>
        /// Gets or sets the payment service provider
        /// </summary>
        [JsonProperty(PropertyName = "pspId")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PaymentServiceProvider? PaymentServiceProvider { get; set; }

        /// <summary>
        /// Gets or sets the transaction fee charged by Payrexx (does not include fees from acquiring or interchange fees)
        /// </summary>
        [JsonProperty(PropertyName = "payrexx_fee")]
        public int? ServiceFee { get; set; }

        /// <summary>
        /// Gets or sets the payment method names
        /// </summary>
        [JsonProperty(PropertyName = "payment")]
        public object PaymentMethodNames { get; set; }

        /// <summary>
        /// Gets or sets the metadata details
        /// </summary>
        [JsonProperty(PropertyName = "metadata")]
        public List<Metadata> Metadata { get; set; }

        /// <summary>
        /// Gets or sets the subscription details
        /// </summary>
        [JsonProperty(PropertyName = "subscription")]
        public Subscription Subscription { get; set; }

        /// <summary>
        /// Gets or sets the invoice details
        /// </summary>
        [JsonProperty(PropertyName = "invoice")]
        public Invoice Invoice { get; set; }

        /// <summary>
        /// Gets or sets the contact details
        /// </summary>
        [JsonProperty(PropertyName = "contact")]
        public Contact Contact { get; set; }
    }
}