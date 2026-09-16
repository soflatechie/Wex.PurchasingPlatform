using Wex.PurchasingPlatform.Api.Validation;
using Wex.PurchasingPlatform.Models;

namespace Wex.PurchasingPlatform.Tests.Validation;

public class UpdatePurchaseTransactionRequestValidatorTests
{
    private readonly UpdatePurchaseTransactionRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenDescriptionExceedsMaxLength_HasValidationError()
    {
        var request = CreateRequest(description: new string('a', 51));

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenTransactionDateIsDefault_HasValidationError()
    {
        var request = new UpdatePurchaseTransactionRequest("Valid description", default, 10.00m);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenPurchaseAmountHasMoreThanTwoDecimalPlaces_HasValidationError()
    {
        var request = CreateRequest(purchaseAmountUsd: 10.005m);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenAllFieldsAreValid_HasNoValidationError()
    {
        var request = CreateRequest();

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    private static UpdatePurchaseTransactionRequest CreateRequest(
        string description = "Office supplies",
        decimal purchaseAmountUsd = 42.50m)
    {
        return new UpdatePurchaseTransactionRequest(description, new DateOnly(2026, 1, 15), purchaseAmountUsd);
    }
}
