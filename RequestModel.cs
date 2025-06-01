namespace FLLSlides.Shared;

/// <summary>
/// Represents a request model containing event, judging, robot game configurations, and teams.
/// </summary>
public class RequestModel
{
    /// <summary>
    /// Gets or sets the name of the request.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("templateDetails")]
    public TemplateDetails TemplateDetails { get; set; }

    /// <summary>
    /// Gets or sets the array of teams.
    /// </summary>
    [JsonPropertyName("substitutions")]
    public Dictionary<string, string> Substitutions { get; set; } = [];
}