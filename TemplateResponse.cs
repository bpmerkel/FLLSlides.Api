namespace FLLSlides.Shared;

/// <summary>
/// Defines the details of a template used in the application.
/// </summary>
public class TemplateResponse
{
    /// <summary>
    /// Gets or sets the request details.
    /// </summary>
    [JsonPropertyName("request")]
    public TemplateRequest Request { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the response was generated.
    /// </summary>
    [JsonPropertyName("generated")]
    public DateTime GeneratedUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the collection of templates available for use.
    /// </summary>
    public TemplateDetails[] Templates { get; set; }
}
