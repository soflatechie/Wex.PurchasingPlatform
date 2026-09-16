using Wex.PurchasingPlatform.Api.Validation;
using Wex.PurchasingPlatform.Models;

namespace Wex.PurchasingPlatform.Tests.Validation;

public class CreatePurchaseTransactionRequestValidatorTests
{
    private readonly CreatePurchaseTransactionRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenDescriptionIsAtMaxLength_HasNoValidationError()
    {
        var request = CreateRequest(description: new string('a', 50));

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenDescriptionExceedsMaxLength_HasValidationError()
    {
        var request = CreateRequest(description: new string('a', 51));

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreatePurchaseTransactionRequest.Description));
    }

    [Fact]
    public void Validate_WhenDescriptionIsEmpty_HasValidationError()
    {
        var request = CreateRequest(description: string.Empty);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenTransactionDateIsDefault_HasValidationError()
    {
        var request = new CreatePurchaseTransactionRequest("Valid description", default, 10.00m);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5.00)]
    public void Validate_WhenPurchaseAmountIsNotPositive_HasValidationError(decimal amount)
    {
        var request = CreateRequest(purchaseAmountUsd: amount);

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

    private static CreatePurchaseTransactionRequest CreateRequest(
        string description = "Office supplies",
        decimal purchaseAmountUsd = 42.50m)
    {
        return new CreatePurchaseTransactionRequest(description, new DateOnly(2026, 1, 15), purchaseAmountUsd);
    }
}
