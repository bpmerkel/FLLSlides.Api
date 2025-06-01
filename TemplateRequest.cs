namespace FLLSlides.Shared;

/// <summary>
/// Defines the details of a template used in the application.
/// </summary>
public class TemplateRequest
{
    /// <summary>
    /// Gets or sets the name of the request.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
}