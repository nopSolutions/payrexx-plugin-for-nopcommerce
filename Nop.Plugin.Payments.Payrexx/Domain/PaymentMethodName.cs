using System.Runtime.Serialization;

namespace Nop.Plugin.Payments.Payrexx.Domain
{
    /// <summary>
    /// Represents payment method name enumeration
    /// </summary>
    public enum PaymentMethodName
    {
        /// <summary>
        /// Visa
        /// </summary>
        [EnumMember(Value = "visa")]
        Visa,

        /// <summary>
        /// Mastercard
        /// </summary>
        [EnumMember(Value = "mastercard")]
        Mastercard,

        /// <summary>
        /// American express
        /// </summary>
        [EnumMember(Value = "american_express")]
        AmericanExpress,

        /// <summary>
        /// Discover
        /// </summary>
        [EnumMember(Value = "discover")]
        Discover,

        /// <summary>
        /// JCB
        /// </summary>
        [EnumMember(Value = "jcb")]
        JCB,

        /// <summary>
        /// Diners club
        /// </summary>
        [EnumMember(Value = "diners_club")]
        DinersClub,

        /// <summary>
        /// Maestro
        /// </summary>
        [EnumMember(Value = "maestro")]
        Maestro,

        /// <summary>
        /// Paypal
        /// </summary>
        [EnumMember(Value = "paypal")]
        Paypal,

        /// <summary>
        /// Airplus
        /// </summary>
        [EnumMember(Value = "airplus")]
        Airplus,

        /// <summary>
        /// Bancontact
        /// </summary>
        [EnumMember(Value = "bancontact")]
        Bancontact,

        /// <summary>
        /// CB
        /// </summary>
        [EnumMember(Value = "cb")]
        CB,

        /// <summary>
        /// Postfinance card
        /// </summary>
        [EnumMember(Value = "postfinance_card")]
        PostfinanceCard,

        /// <summary>
        /// Postfinance efinance
        /// </summary>
        [EnumMember(Value = "postfinance_efinance")]
        PostfinanceEfinance
    }
}