using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Wex.PurchasingPlatform.Api.Common.Exceptions;

namespace Wex.PurchasingPlatform.Api.Middleware;

public class ExchangeRateExceptionHandler(ILogger<ExchangeRateExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            ExchangeRateUnavailableException unavailable => new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "No exchange rate available.",
                Detail = unavailable.Message
            },
            ExchangeRateProviderUnavailableException providerUnavailable => new ProblemDetails
            {
                Status = StatusCodes.Status502BadGateway,
                Title = "The exchange rate provider is unreachable.",
                Detail = providerUnavailable.Message
            },
            _ => null
        };

        if (problemDetails is null)
            return false;

        logger.LogWarning(exception, "Exchange rate lookup failed with status {StatusCode}.", problemDetails.Status);

        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
