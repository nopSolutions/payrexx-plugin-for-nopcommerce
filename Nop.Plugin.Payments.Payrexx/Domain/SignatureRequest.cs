using Microsoft.AspNetCore.Http;

namespace Nop.Plugin.Payments.Payrexx.Domain
{
    /// <summary>
    /// Represents request to check signature
    /// </summary>
    public class SignatureRequest : Request
    {
        /// <summary>
        /// Get a request path
        /// </summary>
        public override string Path => "SignatureCheck/";

        /// <summary>
        /// Get a request method
        /// </summary>
        public override string Method => HttpMethods.Get;
    }
}