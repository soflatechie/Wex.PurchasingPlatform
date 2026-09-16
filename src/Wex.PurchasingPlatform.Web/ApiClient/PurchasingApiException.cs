using System.Net;

namespace Wex.PurchasingPlatform.Web.ApiClient;

public class PurchasingApiException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
