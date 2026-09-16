namespace Wex.PurchasingPlatform.Api.Common.Exceptions;

public class ExchangeRateProviderUnavailableException(string message, Exception innerException)
    : Exception(message, innerException);
