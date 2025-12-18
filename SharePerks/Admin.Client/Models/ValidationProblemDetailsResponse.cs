namespace Admin.Client.Models;

public class ValidationProblemDetailsResponse
{
    public string? Title { get; set; }

    public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>(StringComparer.Ordinal);
}
