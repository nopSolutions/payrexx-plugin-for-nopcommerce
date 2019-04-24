using System.Runtime.Serialization;

namespace Nop.Plugin.Payments.Payrexx.Domain
{
    /// <summary>
    /// Represents payment service provider enumeration
    /// </summary>
    public enum PaymentServiceProvider
    {
        /// <summary>
        /// Payrexx Direct (recommended)
        /// </summary>
        [EnumMember(Value = "36")]
        PayrexxDirect = 1,

        /// <summary>
        /// Postfinance E-Commerce
        /// </summary>
        [EnumMember(Value = "2")]
        PostfinanceECommerce = 2,

        /// <summary>
        /// PayPal
        /// </summary>
        [EnumMember(Value = "3")]
        PayPal = 3,

        /// <summary>
        /// Paymill
        /// </summary>
        [EnumMember(Value = "4")]
        Paymill = 4,

        /// <summary>
        /// Stripe
        /// </summary>
        [EnumMember(Value = "5")]
        Stripe = 5,

        /// <summary>
        /// Ogone Basic
        /// </summary>
        [EnumMember(Value = "6")]
        OgoneBasic = 6,

        /// <summary>
        /// Giropay
        /// </summary>
        [EnumMember(Value = "7")]
        Giropay = 7,

        /// <summary>
        /// Ogone Alias Gateway
        /// </summary>
        [EnumMember(Value = "8")]
        OgoneAliasGateway = 8,

        /// <summary>
        /// Concardis Pay Engine
        /// </summary>
        [EnumMember(Value = "9")]
        ConcardisPayEngine = 9,

        /// <summary>
        /// Concardis Basic
        /// </summary>
        [EnumMember(Value = "10")]
        ConcardisBasic = 10,

        /// <summary>
        /// Coinbase
        /// </summary>
        [EnumMember(Value = "11")]
        Coinbase = 11,

        /// <summary>
        /// Postfinance Alias Gateway
        /// </summary>
        [EnumMember(Value = "12")]
        PostfinanceAliasGateway = 12,

        /// <summary>
        /// Braintree
        /// </summary>
        [EnumMember(Value = "13")]
        Braintree = 13,

        /// <summary>
        /// Sofort
        /// </summary>
        [EnumMember(Value = "14")]
        Sofort = 14,

        /// <summary>
        /// Invoice
        /// </summary>
        [EnumMember(Value = "15")]
        Invoice = 15,

        /// <summary>
        /// Billpay
        /// </summary>
        [EnumMember(Value = "16")]
        Billpay = 16,

        /// <summary>
        /// Twint
        /// </summary>
        [EnumMember(Value = "17")]
        Twint = 17,

        /// <summary>
        /// Saferpay
        /// </summary>
        [EnumMember(Value = "18")]
        Saferpay = 18,

        /// <summary>
        /// Datatrans
        /// </summary>
        [EnumMember(Value = "20")]
        Datatrans = 20,

        /// <summary>
        /// CCAvenue
        /// </summary>
        [EnumMember(Value = "21")]
        CCAvenue = 21,

        /// <summary>
        /// Viveum Basic
        /// </summary>
        [EnumMember(Value = "22")]
        ViveumBasic = 22,

        /// <summary>
        /// REKA Check
        /// </summary>
        [EnumMember(Value = "23")]
        REKACheck = 23,

        /// <summary>
        /// Swissbilling
        /// </summary>
        [EnumMember(Value = "24")]
        Swissbilling = 24,

        /// <summary>
        /// Payone
        /// </summary>
        [EnumMember(Value = "25")]
        Payone = 25,

        /// <summary>
        /// Payrexx Payments by Stripe
        /// </summary>
        [EnumMember(Value = "26")]
        PayrexxPaymentsByStripe = 26,

        /// <summary>
        /// Vorkasse
        /// </summary>
        [EnumMember(Value = "27")]
        Vorkasse = 27,

        /// <summary>
        /// Razorpay
        /// </summary>
        [EnumMember(Value = "28")]
        Razorpay = 28,

        /// <summary>
        /// Concardis Payengine NEW
        /// </summary>
        [EnumMember(Value = "29")]
        ConcardisPayengineNEW = 29,

        /// <summary>
        /// WIRpay
        /// </summary>
        [EnumMember(Value = "30")]
        WIRpay = 30,

        /// <summary>
        /// Mollie
        /// </summary>
        [EnumMember(Value = "31")]
        Mollie = 31,

        /// <summary>
        /// Skrill
        /// </summary>
        [EnumMember(Value = "32")]
        Skrill = 32,

        /// <summary>
        /// VR pay
        /// </summary>
        [EnumMember(Value = "33")]
        VRPay = 33
    }
}