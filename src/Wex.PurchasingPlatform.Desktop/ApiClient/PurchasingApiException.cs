using System.Net;

namespace Wex.PurchasingPlatform.Desktop.ApiClient;

public class PurchasingApiException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
