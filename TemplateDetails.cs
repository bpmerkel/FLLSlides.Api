namespace FLLSlides.Shared;

/// <summary>
/// Defines the details of a template used in the application.
/// </summary>
public class TemplateDetails
{
    /// <summary>
    /// Gets or sets the name of the template.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the name of the file, including its extension.
    /// </summary>
    public string Filename { get; set; }
    
    /// <summary>
    /// Gets or sets the collection of field names.
    /// </summary>
    public string[] Fields { get; set; } = [];
}