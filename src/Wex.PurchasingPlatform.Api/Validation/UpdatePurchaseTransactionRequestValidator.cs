using FluentValidation;
using Wex.PurchasingPlatform.Api.Common;
using Wex.PurchasingPlatform.Models;

namespace Wex.PurchasingPlatform.Api.Validation;

public class UpdatePurchaseTransactionRequestValidator : AbstractValidator<UpdatePurchaseTransactionRequest>
{
    private const int DescriptionMaxLength = 50;

    public UpdatePurchaseTransactionRequestValidator()
    {
        RuleFor(request => request.Description)
            .NotEmpty()
            .MaximumLength(DescriptionMaxLength);

        RuleFor(request => request.TransactionDate)
            .NotEqual(default(DateOnly))
            .WithMessage("Transaction date is required.");

        RuleFor(request => request.PurchaseAmountUsd)
            .GreaterThan(0)
            .WithMessage("Purchase amount must be a positive value.")
            .Must(amount => amount == MoneyRounding.ToCurrency(amount))
            .WithMessage("Purchase amount must not have more than two decimal places.");
    }
}
