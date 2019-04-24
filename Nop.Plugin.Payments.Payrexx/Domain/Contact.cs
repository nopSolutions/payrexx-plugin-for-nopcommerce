using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Payrexx.Domain
{
    /// <summary>
    /// Represents the contact
    /// </summary>
    public class Contact : ResponseData
    {
        /// <summary>
        /// Gets or sets the internal shopper identifier
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the title (mister|miss)
        /// </summary>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the country code
        /// </summary>
        [JsonProperty(PropertyName = "countryISO")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the country name in the shopper's language
        /// </summary>
        [JsonProperty(PropertyName = "country")]
        public string CountryName { get; set; }

        /// <summary>
        /// Gets or sets the delivery country code
        /// </summary>
        [JsonProperty(PropertyName = "delivery_countryISO")]
        public string DeliveryCountryCode { get; set; }

        /// <summary>
        /// Gets or sets the delivery country name in the shopper's language
        /// </summary>
        [JsonProperty(PropertyName = "delivery_country")]
        public string DeliveryCountryName { get; set; }
    }
}