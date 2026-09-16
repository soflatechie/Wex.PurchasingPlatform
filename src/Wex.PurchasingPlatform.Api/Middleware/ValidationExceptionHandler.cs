using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Wex.PurchasingPlatform.Api.Middleware;

public class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
            return false;

        var errors = validationException.Errors
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).ToArray());

        logger.LogWarning(
            "Request validation failed: {Errors}",
            string.Join("; ", errors.SelectMany(field => field.Value.Select(message => $"{field.Key}: {message}"))));

        var problemDetails = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred."
        };

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
