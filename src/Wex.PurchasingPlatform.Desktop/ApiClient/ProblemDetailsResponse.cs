namespace Wex.PurchasingPlatform.Desktop.ApiClient;

public class ProblemDetailsResponse
{
    public string? Title { get; set; }

    public string? Detail { get; set; }

    public Dictionary<string, List<string>>? Errors { get; set; }
}
