namespace FLLSlides.Api;

/// <summary>
/// Represents a class that handles HTTP triggers.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="HttpTrigger"/> class.
/// </remarks>
public partial class API
{
    /// <summary>
    /// Runs the HTTP trigger.
    /// </summary>
    /// <param name="req">The HTTP request data.</param>
    /// <param name="executionContext">The context in which the function is executed.</param>
    /// <returns>The HTTP response data.</returns>
    [Function(nameof(GetTemplateDetails))]
    public static async Task<HttpResponseData> GetTemplateDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req, FunctionContext executionContext)
    {
        var sw = Stopwatch.StartNew();

        var logger = executionContext.GetLogger("HttpTrigger1");
        logger.LogInformation("GetTemplateDetails function processed a request.");

        var request = await req.ReadFromJsonAsync<TemplateRequest>();

        // validate the incoming request
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var response = req.CreateResponse(HttpStatusCode.OK);

        // generate the response
        var tr = new TemplateResponse
        {
            Request = request
        };

        // get files in the templates folder
        // open each and find all the fields
        // return the fields
        var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
        var files = Directory.GetFiles(folder, "*.pptx");
        tr.Templates = files
            .Select(f =>
            {
                using var pres = new Presentation(f);
                var fields = pres.Slides
                    .SelectMany(slide => slide.GetTextBoxes()
                        .Where(textbox => textbox.Text.Contains('{'))
                        .SelectMany(textbox => Regex.Matches(textbox.Text, @"\{(.*)\}", RegexOptions.Multiline)
                            .Cast<Match>()
                            .Select(m => m.Groups[1].Value)
                            .ToArray()
                        )
                    )
                    .ToArray();
                return new TemplateDetails
                {
                    Name = Path.GetFileNameWithoutExtension(f),
                    Filename = Path.GetFileName(f),
                    Fields = fields
                };
            })
            .ToArray();

        await response.WriteAsJsonAsync(tr);
        logger.LogMetric("TransactionTimeMS", sw.Elapsed.TotalMilliseconds);
        return response;
    }

    /// <summary>
    /// Runs the HTTP trigger.
    /// </summary>
    /// <param name="req">The HTTP request data.</param>
    /// <param name="executionContext">The context in which the function is executed.</param>
    /// <returns>The HTTP response data.</returns>
    [Function(nameof(GenerateDeck))]
    public static async Task<HttpResponseData> GenerateDeck([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req, FunctionContext executionContext)
    {
        var sw = Stopwatch.StartNew();

        var logger = executionContext.GetLogger("HttpTrigger1");
        logger.LogInformation("GenerateDeck function processed a request.");

        var request = await req.ReadFromJsonAsync<RequestModel>();

        // validate the incoming request
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var response = req.CreateResponse(HttpStatusCode.OK);
        ProcessRequest(request, response.Body);
        logger.LogMetric("TransactionTimeMS", sw.Elapsed.TotalMilliseconds);
        return response;
    }

    /// <summary>
    /// Lists the directory tree of the application's base directory, including file paths, sizes, and last modified
    /// timestamps.
    /// </summary>
    /// <remarks>This function processes an HTTP GET request and returns a response containing the directory
    /// tree of the application's base directory. Each file is listed with its relative path, size (in bytes), and last
    /// modified timestamp.</remarks>
    /// <param name="req">The HTTP request data, which must include a valid JSON payload.</param>
    /// <param name="executionContext">The execution context for the function, used to access logging and other runtime information.</param>
    /// <returns>An <see cref="HttpResponseData"/> object containing the directory tree information in the response body. The
    /// response has a status code of <see cref="HttpStatusCode.OK"/>.</returns>
    [Function(nameof(ListTree))]
    public static async Task<HttpResponseData> ListTree([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req, FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("HttpTrigger1");
        logger.LogInformation("ListTree function processed a request.");

        var request = await req.ReadFromJsonAsync<RequestModel>();

        // validate the incoming request
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var response = req.CreateResponse(HttpStatusCode.OK);
        // Save the directory tree to response.Body
        using var writer = new StreamWriter(response.Body);
        var folder = AppDomain.CurrentDomain.BaseDirectory;
        var files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(folder, file);
            var fi = new FileInfo(file);
            await writer.WriteLineAsync($"{relativePath} {fi.Length} {fi.LastWriteTime:g}");
        }
        return response;
    }

    /// <summary>
    /// Processes the request and generates a PowerPoint presentation.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="outstream"></param>
    private static void ProcessRequest(RequestModel request, Stream outstream)
    {
        var template = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", request.TemplateDetails.Filename);
        using var pres = new Presentation(template);
        var edits = pres.Slides
            .SelectMany(slide => slide.GetTextBoxes()
                .Where(textbox => textbox.Text.Contains('{'))
                .Select(textbox => new
                {
                    textbox,
                    groups = Regex.Matches(textbox.Text, @"\{(.*)\}", RegexOptions.Multiline)
                        .Cast<Match>()
                        .Select(m => m.Groups.Cast<Group>().ToArray())
                        .ToArray()
                })
            )
            .ToArray();

        foreach (var edit in edits)
        {
            foreach (var paragraph in edit.textbox.Paragraphs)
            {
                foreach (var group in edit.groups)
                {
                    var find = group[0].Value;
                    var key = group[1].Value;
                    var replacement = request.Substitutions.TryGetValue(key, out string sub) ? sub : string.Empty;
                    paragraph.ReplaceText(find, replacement);
                }
            }
        }

        pres.Save(outstream);
    }
}