using Microsoft.AspNetCore.Http;

namespace Nop.Plugin.Payments.Payrexx.Domain
{
    /// <summary>
    /// Represents request to get all available payment providers
    /// </summary>
    public class GetPaymentProvidersRequest : Request
    {
        /// <summary>
        /// Get a request path
        /// </summary>
        public override string Path => $"PaymentProvider/";

        /// <summary>
        /// Get a request method
        /// </summary>
        public override string Method => HttpMethods.Get;
    }
}