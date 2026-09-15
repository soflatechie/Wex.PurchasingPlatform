# Class Diagram

Companion to [`TechnicalDesign.md`](./TechnicalDesign.md). Covers the controller, service, repository, entity, and DTO layers, and how they connect. Everything except the DTOs (`ConvertedPurchaseTransactionDto` etc., in the `Models` project) lives in the `Api` project as folders — see `TechnicalDesign.md` §1–§2 for the physical layout; this diagram shows the logical layering regardless of which folder each class sits in. `Guid`/`DateOnly`/`decimal` are used throughout for identifiers, dates, and money per the financial-rigor requirement.

**Note on generics:** Mermaid's class diagram syntax renders `IRepository~TEntity~` as an open generic. In code, `IPurchaseTransactionRepository` and `IExchangeRateRepository` each close it over a concrete entity (`IRepository<PurchaseTransaction>` and `IRepository<ExchangeRateQuote>` respectively) — shown generically here for readability.

![Class diagram](./ClassDiagram.png)

The image above is a rendered copy of the Mermaid diagram below, for anyone viewing this file without Mermaid support. Edit the Mermaid source, not the image, and re-render if the diagram changes.

```mermaid
classDiagram
    %% ---------- API layer ----------
    class PurchaseTransactionsController {
        -IPurchaseTransactionService transactionService
        -ICurrencyConversionService conversionService
        +Create(CreatePurchaseTransactionRequest) ActionResult~PurchaseTransactionDto~
        +GetById(Guid id) ActionResult~PurchaseTransactionDto~
        +GetAll() ActionResult~List~PurchaseTransactionDto~~
        +Update(Guid id, UpdatePurchaseTransactionRequest) ActionResult~PurchaseTransactionDto~
        +Delete(Guid id) ActionResult
        +GetConversion(Guid id, string country, string currency) ActionResult~ConvertedPurchaseTransactionDto~
    }

    class CurrenciesController {
        -ICurrencyConversionService conversionService
        +GetAvailableCurrencies(DateOnly transactionDate) ActionResult~List~CurrencyOptionDto~~
    }

    class ExchangeRatesController {
        -IExchangeRateSyncService syncService
        +TriggerSync() ActionResult
    }

    %% ---------- Application / service layer ----------
    class IPurchaseTransactionService {
        <<interface>>
        +CreateAsync(CreatePurchaseTransactionRequest) Task~PurchaseTransactionDto~
        +GetByIdAsync(Guid id) Task~PurchaseTransactionDto~
        +GetAllAsync() Task~List~PurchaseTransactionDto~~
        +UpdateAsync(Guid id, UpdatePurchaseTransactionRequest) Task~PurchaseTransactionDto~
        +DeleteAsync(Guid id) Task~bool~
    }

    class PurchaseTransactionService {
        -IPurchaseTransactionRepository repository
        -IValidator~CreatePurchaseTransactionRequest~ createValidator
        -IValidator~UpdatePurchaseTransactionRequest~ updateValidator
    }

    class ICurrencyConversionService {
        <<interface>>
        +GetConvertedAsync(Guid transactionId, string country, string currencyName) Task~ConvertedPurchaseTransactionDto~
        +GetAvailableCurrenciesAsync(DateOnly transactionDate) Task~List~CurrencyOptionDto~~
    }

    class CurrencyConversionService {
        -IPurchaseTransactionRepository transactionRepository
        -IExchangeRateRepository exchangeRateRepository
        -SelectRate(List~ExchangeRateQuote~ quotes, DateOnly transactionDate) RateSelectionResult
    }

    class RateSelectionResult {
        +decimal ExchangeRate
        +DateOnly RateDate
        +bool IsStale
    }

    class IExchangeRateSyncService {
        <<interface>>
        +SyncAsync(CancellationToken ct) Task
    }

    class ExchangeRateSyncService {
        -IExchangeRateRepository exchangeRateRepository
        -IExchangeRateProvider exchangeRateProvider
    }

    class ExchangeRateSyncHostedService {
        <<BackgroundService>>
        -IExchangeRateSyncService syncService
        +StartAsync(CancellationToken ct) Task
    }

    %% ---------- Repository layer ----------
    class IRepository~TEntity~ {
        <<interface>>
        +GetByIdAsync(object id) Task~TEntity~
        +GetAllAsync() Task~List~TEntity~~
        +AddAsync(TEntity entity) Task~TEntity~
        +UpdateAsync(TEntity entity) Task
        +DeleteAsync(object id) Task~bool~
    }

    class Repository~TEntity~ {
        #AppDbContext context
    }

    class IPurchaseTransactionRepository {
        <<interface>>
    }

    class PurchaseTransactionRepository

    class IExchangeRateRepository {
        <<interface>>
        +GetRatesOnOrBeforeAsync(string country, string currencyName, DateOnly date) Task~List~ExchangeRateQuote~~
        +UpsertRangeAsync(List~ExchangeRateQuote~ quotes) Task
        +GetDistinctCurrencyOptionsAsync(DateOnly onOrBeforeDate) Task~List~CurrencyOptionDto~~
        +GetLatestRecordDateAsync() Task~DateOnly?~
    }

    class ExchangeRateRepository

    %% ---------- External integration ----------
    class IExchangeRateProvider {
        <<interface>>
        +FetchRatesAsync(DateTime sinceUtc) Task~List~ExchangeRateQuote~~
    }

    class TreasuryExchangeRateClient {
        -HttpClient httpClient
    }

    %% ---------- Domain entities ----------
    class PurchaseTransaction {
        +Guid Id
        +string Description
        +DateOnly TransactionDate
        +decimal PurchaseAmountUsd
        +DateTime CreatedAtUtc
    }

    class ExchangeRateQuote {
        +int Id
        +string Country
        +string CurrencyName
        +DateOnly RecordDate
        +decimal ExchangeRate
        +DateTime FetchedAtUtc
    }

    %% ---------- Models (DTOs) ----------
    class CreatePurchaseTransactionRequest {
        +string Description
        +DateOnly TransactionDate
        +decimal PurchaseAmountUsd
    }

    class UpdatePurchaseTransactionRequest {
        +string Description
        +DateOnly TransactionDate
        +decimal PurchaseAmountUsd
    }

    class PurchaseTransactionDto {
        +Guid Id
        +string Description
        +DateOnly TransactionDate
        +decimal PurchaseAmountUsd
    }

    class ConvertedPurchaseTransactionDto {
        +Guid Id
        +string Description
        +DateOnly TransactionDate
        +decimal PurchaseAmountUsd
        +string Country
        +string CurrencyName
        +decimal ExchangeRate
        +DateOnly RateDate
        +bool IsStale
        +decimal ConvertedAmount
    }

    class CurrencyOptionDto {
        +string Country
        +string CurrencyName
    }

    %% ---------- Relationships ----------
    PurchaseTransactionsController --> IPurchaseTransactionService
    PurchaseTransactionsController --> ICurrencyConversionService
    CurrenciesController --> ICurrencyConversionService
    ExchangeRatesController --> IExchangeRateSyncService

    IPurchaseTransactionService <|.. PurchaseTransactionService
    ICurrencyConversionService <|.. CurrencyConversionService

    PurchaseTransactionService --> IPurchaseTransactionRepository
    CurrencyConversionService --> IPurchaseTransactionRepository
    CurrencyConversionService --> IExchangeRateRepository
    CurrencyConversionService ..> RateSelectionResult

    IExchangeRateSyncService <|.. ExchangeRateSyncService
    ExchangeRateSyncHostedService --> IExchangeRateSyncService
    ExchangeRateSyncService --> IExchangeRateRepository
    ExchangeRateSyncService --> IExchangeRateProvider

    IRepository~TEntity~ <|-- IPurchaseTransactionRepository
    IRepository~TEntity~ <|-- IExchangeRateRepository
    IRepository~TEntity~ <|.. Repository~TEntity~
    Repository~TEntity~ <|-- PurchaseTransactionRepository
    Repository~TEntity~ <|-- ExchangeRateRepository
    IPurchaseTransactionRepository <|.. PurchaseTransactionRepository
    IExchangeRateRepository <|.. ExchangeRateRepository

    IExchangeRateProvider <|.. TreasuryExchangeRateClient

    PurchaseTransactionRepository ..> PurchaseTransaction
    ExchangeRateRepository ..> ExchangeRateQuote

    PurchaseTransactionService ..> PurchaseTransactionDto
    PurchaseTransactionService ..> CreatePurchaseTransactionRequest
    PurchaseTransactionService ..> UpdatePurchaseTransactionRequest
    CurrencyConversionService ..> ConvertedPurchaseTransactionDto
    CurrencyConversionService ..> CurrencyOptionDto
```

## Changelog

- Added `ExchangeRateSyncService`, `ExchangeRateSyncHostedService`, and `ExchangeRatesController` for the proactive exchange-rate sync.
- Removed the `ExchangeRateSyncState` entity; its watermark is now `IExchangeRateRepository.GetLatestRecordDateAsync()`.
- Classes reorganized from separate class-library projects into folders within one `Api` project; the diagram's classes and relationships are unchanged by this, only where they physically live (`TechnicalDesign.md` §1–§2).
