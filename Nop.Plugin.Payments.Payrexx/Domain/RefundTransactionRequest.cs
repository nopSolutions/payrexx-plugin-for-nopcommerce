using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Payrexx.Domain;

/// <summary>
/// Represents request to refund transaction
/// </summary>
public class RefundTransactionRequest : Request
{
    /// <summary>
    /// Gets or sets the identifier 
    /// </summary>
    [JsonIgnore]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the custom amount to refund (in cents). If not set, the whole amount will be refunded
    /// </summary>
    [JsonProperty(PropertyName = "amount")]
    public int? AmountToRefund { get; set; }

    /// <summary>
    /// Get a request path
    /// </summary>
    public override string Path => $"Transaction/{Id}/refund";

    /// <summary>
    /// Get a request method
    /// </summary>
    public override string Method => HttpMethods.Post;
}