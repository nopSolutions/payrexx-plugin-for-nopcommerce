using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.Payrexx
{
    /// <summary>
    /// Represents plugin settings
    /// </summary>
    public class PayrexxSettings : ISettings
    {
        /// <summary>
        /// Gets or sets instance name
        /// </summary>
        public string InstanceName { get; set; }

        /// <summary>
        /// Gets or sets API secret key
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// Gets or sets an identifier of the payment transaction type (authorization only or authorization and capture in a single request)
        /// </summary>
        public int PaymentTransactionTypeId { get; set; }
    }
}