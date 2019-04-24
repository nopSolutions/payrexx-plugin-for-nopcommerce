using System.Runtime.Serialization;

namespace Nop.Plugin.Payments.Payrexx.Domain
{
    /// <summary>
    /// Represents response status enumeration
    /// </summary>
    public enum ResponseStatus
    {
        /// <summary>
        /// Request was successful
        /// </summary>
        [EnumMember(Value = "success")]
        Success,

        /// <summary>
        /// Request failed
        /// </summary>
        [EnumMember(Value = "error")]
        Error
    }
}