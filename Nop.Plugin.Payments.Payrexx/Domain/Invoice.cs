using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nop.Plugin.Payments.Payrexx.Domain
{
    /// <summary>
    /// Represents the invoice
    /// </summary>
    public class Invoice : ResponseData
    {
        /// <summary>
        /// Gets or sets the identifier 
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the status 
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public InvoiceStatus? Status { get; set; }

        /// <summary>
        /// Gets or sets the hash 
        /// </summary>
        [JsonProperty(PropertyName = "hash")]
        public string Hash { get; set; }

        /// <summary>
        /// Gets or sets the reference identifier 
        /// </summary>
        [JsonProperty(PropertyName = "referenceId")]
        public string ReferenceId { get; set; }

        /// <summary>
        /// Gets or sets the payment link 
        /// </summary>
        [JsonProperty(PropertyName = "link")]
        public string PaymentLink { get; set; }

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
        /// Gets or sets the name 
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the payment handled by API
        /// </summary>
        [JsonProperty(PropertyName = "api")]
        [JsonConverter(typeof(BoolConverter))]
        public bool ApiUsed { get; set; }

        /// <summary>
        /// Gets or sets the payment service provider
        /// </summary>
        [JsonProperty(PropertyName = "psp", ItemConverterType = typeof(StringEnumConverter))]
        public List<PaymentServiceProvider> PaymentServiceProvider { get; set; }

        /// <summary>
        /// Gets or sets the purpose 
        /// </summary>
        [JsonProperty(PropertyName = "purpose")]
        public string Purpose { get; set; }

        /// <summary>
        /// Gets or sets the total amount (in cents)
        /// </summary>
        [JsonProperty(PropertyName = "amount")]
        public decimal? TotalAmount { get; set; }

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
        /// Gets or sets the SKU 
        /// </summary>
        [JsonProperty(PropertyName = "sku")]
        public string Sku { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the payment created by subscription
        /// </summary>
        [JsonProperty(PropertyName = "subscriptionState")]
        [JsonConverter(typeof(BoolConverter))]
        public bool IsSubscription { get; set; }

        /// <summary>
        /// Gets or sets the subscription interval 
        /// </summary>
        [JsonProperty(PropertyName = "subscriptionInterval")]
        public string SubscriptionInterval { get; set; }

        /// <summary>
        /// Gets or sets the subscription period 
        /// </summary>
        [JsonProperty(PropertyName = "subscriptionPeriod")]
        public string SubscriptionPeriod { get; set; }

        /// <summary>
        /// Gets or sets the subscription period min amount 
        /// </summary>
        [JsonProperty(PropertyName = "subscriptionPeriodMinAmount")]
        public string SubscriptionPeriodMinAmount { get; set; }

        /// <summary>
        /// Gets or sets the subscription cancellation interval 
        /// </summary>
        [JsonProperty(PropertyName = "subscriptionCancellationInterval")]
        public string SubscriptionCancellationInterval { get; set; }

        /// <summary>
        /// Gets or sets the date of creation 
        /// </summary>
        [JsonProperty(PropertyName = "createdAt")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the comma separated product names
        /// </summary>
        [JsonProperty(PropertyName = "number")]
        public string ProductNames { get; set; }

        /// <summary>
        /// Gets or sets the products details
        /// </summary>
        [JsonProperty(PropertyName = "products")]
        public List<object> ProductDetails { get; set; }

        /// <summary>
        /// Gets or sets the discount details
        /// </summary>
        [JsonProperty(PropertyName = "discount")]
        public object DiscountDetails { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the payment has been processed in sandbox
        /// </summary>
        [JsonProperty(PropertyName = "test")]
        [JsonConverter(typeof(BoolConverter))]
        public bool IsTestInvoice { get; set; }

        /// <summary>
        /// Gets or sets the identifier of invoice created through API
        /// </summary>
        [JsonProperty(PropertyName = "paymentRequestId")]
        public string InvoiceId { get; set; }

        /// <summary>
        /// Gets or sets the payment link details
        /// </summary>
        [JsonProperty(PropertyName = "paymentLink")]
        public object PaymentLinkDetails { get; set; }

        /// <summary>
        /// Gets or sets the additional fields
        /// </summary>
        [JsonProperty(PropertyName = "fields")]
        public object AdditionalFields { get; set; }
    }
}