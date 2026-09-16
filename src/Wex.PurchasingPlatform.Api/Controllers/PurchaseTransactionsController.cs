using Microsoft.AspNetCore.Mvc;
using Wex.PurchasingPlatform.Api.Services.Interfaces;
using Wex.PurchasingPlatform.Models;

namespace Wex.PurchasingPlatform.Api.Controllers;

[ApiController]
[Route("api/purchasetransactions")]
public class PurchaseTransactionsController(
    IPurchaseTransactionService transactionService,
    ICurrencyConversionService conversionService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PurchaseTransactionDto>> Create(
        CreatePurchaseTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var createdTransaction = await transactionService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = createdTransaction.Id }, createdTransaction);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PurchaseTransactionDto>>> GetAll(CancellationToken cancellationToken)
    {
        var transactions = await transactionService.GetAllAsync(cancellationToken);

        return Ok(transactions);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PurchaseTransactionDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var transaction = await transactionService.GetByIdAsync(id, cancellationToken);

        if (transaction is null)
            return NotFound();

        return Ok(transaction);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PurchaseTransactionDto>> Update(
        Guid id,
        UpdatePurchaseTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var updatedTransaction = await transactionService.UpdateAsync(id, request, cancellationToken);

        if (updatedTransaction is null)
            return NotFound();

        return Ok(updatedTransaction);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var wasDeleted = await transactionService.DeleteAsync(id, cancellationToken);

        if (!wasDeleted)
            return NotFound();

        return NoContent();
    }

    [HttpGet("{id:guid}/conversion")]
    public async Task<ActionResult<ConvertedPurchaseTransactionDto>> GetConversion(
        Guid id,
        [FromQuery] string country,
        [FromQuery] string currency,
        CancellationToken cancellationToken)
    {
        var convertedTransaction = await conversionService.GetConvertedAsync(id, country, currency, cancellationToken);

        if (convertedTransaction is null)
            return NotFound();

        return Ok(convertedTransaction);
    }
}
