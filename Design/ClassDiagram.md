# Class Diagram

Companion to [`TechnicalDesign.md`](./TechnicalDesign.md). Covers the controller, service, repository, entity, and DTO layers, and how they connect. Everything except the DTOs (`ConvertedPurchaseTransactionDto` etc., in the `Models` project) lives in the `Api` project as folders — see `TechnicalDesign.md` §1–§2 for the physical layout; this diagram shows the logical layering regardless of which folder each class sits in. `Guid`/`DateOnly`/`decimal` are used throughout for identifiers, dates, and money per the financial-rigor requirement.

**Note on generics:** Mermaid's class diagram syntax renders `IRepository~TEntity~` and `Repository~TEntity~` as open generics. In code, `PurchaseTransactionRepository` and `CurrencyOptionRepository` each close `Repository<TEntity>` over a concrete entity (`Repository<PurchaseTransaction>` and `Repository<CurrencyOption>` respectively) — shown generically here for readability. There is deliberately no per-entity repository interface (`IPurchaseTransactionRepository`, `ICurrencyOptionRepository`): the one generic `IRepository<TEntity>` is the only repository interface in the codebase, and any entity-specific queries live as plain methods directly on the concrete subclass.

![Class diagram](./ClassDiagram.svg)

The image above is an SVG (scales to any zoom level without blurring) rendered from the Mermaid diagram below, for anyone viewing this file without Mermaid support. A high-resolution `ClassDiagram.png` is also in this folder for tools that don't display SVG well (e.g., pasting into Office documents). Edit the Mermaid source, not the images, and re-render both if the diagram changes.

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
        -ICurrencyOptionCacheService cacheService
        +GetCountries() ActionResult~List~string~~
        +GetAvailableCurrencies(string country, DateOnly transactionDate) ActionResult~List~CurrencyOptionDto~~
        +TriggerRefresh() ActionResult
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
        -PurchaseTransactionRepository repository
        -IValidator~CreatePurchaseTransactionRequest~ createValidator
        -IValidator~UpdatePurchaseTransactionRequest~ updateValidator
    }

    class ICurrencyConversionService {
        <<interface>>
        +GetConvertedAsync(Guid transactionId, string country, string currencyName) Task~ConvertedPurchaseTransactionDto~
        +GetAvailableCountriesAsync() Task~List~string~~
        +GetAvailableCurrenciesAsync(string country, DateOnly transactionDate) Task~List~CurrencyOptionDto~~
    }

    class CurrencyConversionService {
        -PurchaseTransactionRepository transactionRepository
        -CurrencyOptionRepository currencyOptionRepository
        -IExchangeRateProvider exchangeRateProvider
        -SelectRate(ExchangeRateLookupResult quote, DateOnly transactionDate) RateSelectionResult
    }

    class RateSelectionResult {
        +decimal ExchangeRate
        +DateOnly RateDate
        +bool IsStale
    }

    class ICurrencyOptionCacheService {
        <<interface>>
        +RefreshAsync(CancellationToken ct) Task
    }

    class CurrencyOptionCacheService {
        -CurrencyOptionRepository currencyOptionRepository
        -IExchangeRateProvider exchangeRateProvider
    }

    class CurrencyOptionCacheHostedService {
        <<BackgroundService>>
        -ICurrencyOptionCacheService cacheService
        -CurrencyOptionRepository currencyOptionRepository
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

    class PurchaseTransactionRepository {
        +GetByIdAsync(object id) Task~PurchaseTransaction~
        +DeleteAsync(object id) Task~bool~
    }

    class CurrencyOptionRepository {
        +GetByCountryAsync(string country) Task~List~CurrencyOption~~
        +ReplaceAllAsync(List~CurrencyOption~ options) Task
    }

    %% ---------- External integration ----------
    class IExchangeRateProvider {
        <<interface>>
        +GetLatestRateOnOrBeforeAsync(string country, string currencyName, DateOnly onOrBeforeDate) Task~ExchangeRateLookupResult~
        +FetchAllCurrencyOptionsAsync() Task~List~CurrencyOptionDto~~
    }

    class TreasuryExchangeRateClient {
        -HttpClient httpClient
    }

    class ExchangeRateLookupResult {
        +string Country
        +string CurrencyName
        +DateOnly RecordDate
        +decimal ExchangeRate
    }

    %% ---------- Domain entities ----------
    class PurchaseTransaction {
        +int TransactionNumber
        +Guid Id
        +string Description
        +DateOnly TransactionDate
        +decimal PurchaseAmountUsd
        +DateTime CreatedAtUtc
    }

    class CurrencyOption {
        +int Id
        +string Country
        +string CurrencyName
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
        +int TransactionNumber
        +Guid Id
        +string Description
        +DateOnly TransactionDate
        +decimal PurchaseAmountUsd
    }

    class ConvertedPurchaseTransactionDto {
        +int TransactionNumber
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
    CurrenciesController --> ICurrencyOptionCacheService

    IPurchaseTransactionService <|.. PurchaseTransactionService
    ICurrencyConversionService <|.. CurrencyConversionService

    PurchaseTransactionService --> PurchaseTransactionRepository
    CurrencyConversionService --> PurchaseTransactionRepository
    CurrencyConversionService --> CurrencyOptionRepository
    CurrencyConversionService --> IExchangeRateProvider
    CurrencyConversionService ..> RateSelectionResult
    CurrencyConversionService ..> ExchangeRateLookupResult

    ICurrencyOptionCacheService <|.. CurrencyOptionCacheService
    CurrencyOptionCacheHostedService --> ICurrencyOptionCacheService
    CurrencyOptionCacheHostedService --> CurrencyOptionRepository
    CurrencyOptionCacheService --> CurrencyOptionRepository
    CurrencyOptionCacheService --> IExchangeRateProvider

    IRepository~TEntity~ <|.. Repository~TEntity~
    Repository~TEntity~ <|-- PurchaseTransactionRepository
    Repository~TEntity~ <|-- CurrencyOptionRepository

    IExchangeRateProvider <|.. TreasuryExchangeRateClient

    PurchaseTransactionRepository ..> PurchaseTransaction
    CurrencyOptionRepository ..> CurrencyOption

    PurchaseTransactionService ..> PurchaseTransactionDto
    PurchaseTransactionService ..> CreatePurchaseTransactionRequest
    PurchaseTransactionService ..> UpdatePurchaseTransactionRequest
    CurrencyConversionService ..> ConvertedPurchaseTransactionDto
    CurrencyConversionService ..> CurrencyOptionDto
```

## Changelog

- Reversed the proactive exchange-rate sync in favor of live per-request lookups (see `TechnicalDesign.md`'s changelog). Removed `ExchangeRateQuote`, `IExchangeRateRepository`/`ExchangeRateRepository`, `IExchangeRateSyncService`/`ExchangeRateSyncService`/`ExchangeRateSyncHostedService`, and `ExchangeRatesController`.
- Added `CurrencyOption` (identity-only: `Country` + `CurrencyName`), `ICurrencyOptionRepository`/`CurrencyOptionRepository`, `ICurrencyOptionCacheService`/`CurrencyOptionCacheService`/`CurrencyOptionCacheHostedService`, and `ExchangeRateLookupResult` (the plain, non-persisted result of a single live Treasury lookup). `CurrencyConversionService` now depends on `IExchangeRateProvider` directly instead of a rate repository. `CurrenciesController` gained `GetCountries()` and `TriggerRefresh()`, and `GetAvailableCurrencies` now takes a `country` parameter.
- ~~Added `ExchangeRateSyncService`, `ExchangeRateSyncHostedService`, and `ExchangeRatesController` for the proactive exchange-rate sync.~~
- ~~Removed the `ExchangeRateSyncState` entity; its watermark is now `IExchangeRateRepository.GetLatestRecordDateAsync()`.~~
- Classes reorganized from separate class-library projects into folders within one `Api` project; the diagram's classes and relationships are unchanged by this, only where they physically live (`TechnicalDesign.md` §1–§2).
- Added `ICurrencyConversionService.GetAvailableCountriesAsync()`, backing `CurrenciesController.GetCountries()` — this diagram previously omitted the interface method `GetCountries()` actually calls.
- Removed `IPurchaseTransactionRepository` and `ICurrencyOptionRepository` — both added nothing over the single generic `IRepository<TEntity>` (the first was an empty marker interface; the second's two extra methods now live directly on the concrete `CurrencyOptionRepository` class). `PurchaseTransactionRepository` and `CurrencyOptionRepository` are now the only repository types services depend on, registered in DI as themselves rather than against a per-entity interface. `Repository<TEntity>`'s methods are `virtual` so these concrete classes stay mockable in unit tests without an interface.
- Added `PurchaseTransaction.TransactionNumber` (and to `PurchaseTransactionDto`/`ConvertedPurchaseTransactionDto`) — a sequential integer for display, since a GUID isn't something worth showing a user. `TransactionNumber`, not `Id`, is now the actual database primary key (see `DatabaseDesign.md`), so `PurchaseTransactionRepository` overrides `GetByIdAsync`/`DeleteAsync` to look up by `Id` instead of relying on the base class's primary-key-based lookup. `Id` is otherwise unchanged — still the identifier in every route, DTO, and front-end reference.
