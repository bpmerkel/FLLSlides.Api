namespace FLLSlides.Shared;

/// <summary>
/// Represents the response model containing request details, context, and generation timestamp.
/// </summary>
public class ResponseModel
{
    /// <summary>
    /// Gets or sets the request details.
    /// </summary>
    [JsonPropertyName("request")]
    public RequestModel Request { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the response was generated.
    /// </summary>
    [JsonPropertyName("generated")]
    public DateTime GeneratedUtc { get; set; } = DateTime.UtcNow;
}