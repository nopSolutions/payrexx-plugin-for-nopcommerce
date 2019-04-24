using System.Runtime.Serialization;

namespace Nop.Plugin.Payments.Payrexx.Domain
{
    /// <summary>
    /// Represents invoice status enumeration
    /// </summary>
    public enum InvoiceStatus
    {
        /// <summary>
        /// Invoice is pending
        /// </summary>
        [EnumMember(Value = "waiting")]
        Pending,

        /// <summary>
        /// Invoice is confirmed
        /// </summary>
        [EnumMember(Value = "confirmed")]
        Confirmed,

        /// <summary>
        /// Invoice is cancelled
        /// </summary>
        [EnumMember(Value = "cancelled")]
        Cancelled,

        /// <summary>
        /// Invoice is declined
        /// </summary>
        [EnumMember(Value = "declined")]
        Declined,

        /// <summary>
        /// Invoice is authorized
        /// </summary>
        [EnumMember(Value = "authorized")]
        Authorized,

        /// <summary>
        /// Invoice is reserved
        /// </summary>
        [EnumMember(Value = "reserved")]
        Reserved,

        /// <summary>
        /// Invoice is refunded
        /// </summary>
        [EnumMember(Value = "refunded")]
        Refunded,

        /// <summary>
        /// Invoice is partially refunded 
        /// </summary>
        [EnumMember(Value = "partially-refunded")]
        PartiallyRefunded,

        /// <summary>
        /// Invoice is chargebacked
        /// </summary>
        [EnumMember(Value = "chargeback")]
        Chargeback,

        /// <summary>
        /// Error is occurred
        /// </summary>
        [EnumMember(Value = "error")]
        Error
    }
}