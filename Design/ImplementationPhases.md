# Implementation Phases

Companion to [`TechnicalDesign.md`](./TechnicalDesign.md). Each phase produces something runnable/testable before the next begins — no phase depends on code that doesn't exist yet further down the list.

## Phase 0 — Solution scaffolding

- Create `Wex.PurchasingPlatform.slnx` and the empty projects from `TechnicalDesign.md` §1 (`Api`, `Models`, `Desktop`, `Web`, and `Tests`), with `Api`'s internal folders from §2 stubbed out (`Controllers/`, `Services/`, `Repositories/`, `Entities/`, `Data/`, `ExternalServices/`, `Validation/`).
- `Api` boots with Swagger UI reachable, no endpoints yet, and `Microsoft.Extensions.Logging` (Console/Debug providers, the ASP.NET Core default) already active — see `TechnicalDesign.md` §9.
- **Exit criteria**: `dotnet build` succeeds across the whole solution; `dotnet run --project src/Wex.PurchasingPlatform.Api` serves `/swagger`.

## Phase 1 — Persistence

- `PurchaseTransaction` and `CurrencyOption` in `Api/Entities/`. `PurchaseTransaction.TransactionNumber` (autoincrementing `INTEGER`) is the actual database primary key; `Id` (GUID) is a separate, uniquely-indexed column and remains the identifier used everywhere in the API — see `DatabaseDesign.md`.
- `AppDbContext`, EF Core configuration (decimal precision, unique index on `CurrencyOption`), and the first migration, per `DatabaseDesign.md`, in `Api/Data/`.
- `IRepository<TEntity>` + `Repository<TEntity>`, then `PurchaseTransactionRepository` and `CurrencyOptionRepository` (plain concrete subclasses, no per-entity interface), in `Api/Repositories/`. `PurchaseTransactionRepository` overrides `GetByIdAsync`/`DeleteAsync` to look up by `Id` rather than the primary key.
- **Exit criteria**: running the app creates `purchasing.db` with both tables; unit tests (mocked, no real database — per `InitialDesign.md` §4) cover the service/repository layer.

## Phase 2 — Purchase transaction CRUD (Requirement #1 + added update/delete scope)

- `Models`: `CreatePurchaseTransactionRequest`, `UpdatePurchaseTransactionRequest`, `PurchaseTransactionDto`.
- `Api/Validation/`: FluentValidation validators (50-char description, positive rounded amount, valid date). `Api/Services/`: `PurchaseTransactionService`.
- `Api/Controllers/`: `PurchaseTransactionsController` — `POST`/`GET`/`GET {id}`/`PUT {id}`/`DELETE {id}`, all through the service only.
- **Exit criteria**: Requirement #1 fully satisfied and demonstrable via Swagger; unit tests on validation boundaries and on all five controller actions (including 404s), each with its service dependency mocked — no test host.

## Phase 3 — Treasury exchange rate integration

- `Api/ExternalServices/`: `IExchangeRateProvider` + `TreasuryExchangeRateClient` (HTTP client against the Fiscal Data API), with two responsibilities:
  - `GetLatestRateOnOrBeforeAsync(country, currencyName, onOrBeforeDate)` — one server-side-filtered call (`filter=country_currency_desc:eq:...,record_date:lte:...&sort=-record_date&page[size]=1`) that returns exactly the row a conversion needs, or nothing. No rate values are ever stored locally — see `TechnicalDesign.md` §10.
  - `FetchAllCurrencyOptionsAsync()` — a field-selected (`country`,`currency` only) crawl used solely to build the small `CurrencyOption` identity cache, never to cache rate values.
- `Api/Entities/CurrencyOption.cs` + `CurrencyOptionRepository` in `Api/Repositories/`: a small local cache (a few hundred rows at most) of every distinct `(Country, CurrencyName)` pair Treasury has ever published — identity only.
- `ICurrencyOptionCacheService`/`CurrencyOptionCacheService` + `CurrencyOptionCacheHostedService` in `Api/Services/`: at startup, populates `CurrencyOption` **only if the table is empty**, via `FetchAllCurrencyOptionsAsync()` — a one-time crawl, not a per-startup sync. `POST /api/currencies/refresh` (`TechnicalDesign.md` §6) triggers it manually if Treasury ever adds a new currency.
- `SelectRate` logic in `CurrencyConversionService`: calls `IExchangeRateProvider.GetLatestRateOnOrBeforeAsync` directly and live, once per conversion request — most recent rate on or before the transaction date, `isStale` past Treasury's documented 3-month window — see `TechnicalDesign.md` §5 / `InitialDesign.md` §2.2. No local rate table is read.
- **Exit criteria**: a fresh run of the app starts instantly, making no Treasury call until `CurrencyOption` is empty, at which point it performs one one-time identity crawl; a conversion request makes exactly one live HTTP call to Treasury and returns the correct rate/staleness for the transaction's date; the app still starts and Requirement #1 endpoints still work if Treasury is unreachable (a conversion request instead returns a clear `502`, distinct from the `422` "no rate ever published" case); unit tests over `SelectRate` covering the edge cases in `InitialDesign.md` §4 (exact-date match, 3-month boundary, older-but-still-returned, no rate at all) using a mocked `IExchangeRateProvider` — no live network calls in the test suite.

## Phase 4 — Currency conversion endpoint (Requirement #2)

- `Models`: `ConvertedPurchaseTransactionDto`, `CurrencyOptionDto`.
- `Api/Controllers/`: `GET /api/purchasetransactions/{id}/conversion`, `GET /api/countries` (distinct countries from the `CurrencyOption` cache — no live Treasury calls), and `GET /api/currencies?country=...&transactionDate=...` (that country's cached candidate currencies, live-checked one at a time against Treasury and filtered to what's usable as of the transaction's date, per `InitialDesign.md` §2.2's Eurozone/legacy-currency discussion). Splitting country and currency into two calls keeps the live-check count bounded by "currencies per country" (1–3) rather than every currency Treasury has ever published.
- **Exit criteria**: Requirement #2 fully satisfied and demonstrable via Swagger against live Treasury data (a manual reviewer check, not an automated test); unit tests for the conversion controller action covering supported currency, unknown currency, and nonexistent transaction id, each with `ICurrencyConversionService` mocked.

## Phase 5.1 — Desktop front end (WinForms)

- `Desktop` (WinForms): transaction list/grid, create/edit form, delete confirmation, country→currency cascading picker (filtered by the selected transaction's date via `GET /api/currencies`), conversion display showing rate, rate date, and a visible staleness indicator.
- `Desktop/ApiClient/` holds its own thin HTTP wrapper around the `Api` endpoints, using `Models` for request/response types.
- **Exit criteria**: the Desktop app can complete the full flow — create a transaction, list it, edit it, convert it to a foreign currency and see the staleness indicator, delete it — against a running `Api` instance. Held for manual testing/sign-off before Phase 5.2 begins.

## Phase 5.2 — Web front end (Blazor)

- `Web` (Blazor): the same set of screens as Phase 5.1, browser-based.
- `Web/ApiClient/` holds its own thin HTTP wrapper around the `Api` endpoints, using `Models` for request/response types — a separate implementation from `Desktop/ApiClient/`, not a shared project (per `TechnicalDesign.md` §1).
- **Exit criteria**: the Blazor app can complete the same full flow as Phase 5.1, against a running `Api` instance.

## Phase 6 — Polish and hand-off

- Log level/message consistency pass across services and the exception-handling middleware; final pass on `ProblemDetails` error responses for consistency across all endpoints.
- `README.md` at the repo root: how to run the API and each front end, and the still-open rounding-convention question surfaced prominently for the reviewer.
- Full test suite run and coverage sanity check on the money/rate-selection logic specifically (the areas called out as "essential" in `InitialDesign.md` §4).
- **Exit criteria**: a reviewer can clone the repo, run one command per process (API + a front end), and exercise both requirements end to end with zero configuration.

## Changelog

- Rewritten for the simplified solution structure: `Domain`/`Contracts`/`Application`/`Infrastructure` collapsed into `Api` (folders), `Contracts` renamed `Models`, four test projects collapsed into one `Tests`, shared `ApiClient` project removed in favor of a per-front-end `ApiClient/` folder.
- Phase 1/3/4 revised: dropped the proactive full-history sync (`ExchangeRateQuote`, ~15–20K rows) in favor of one live, filtered Treasury call per conversion request, plus a small local `CurrencyOption` cache of just the distinct country/currency identities (not rates) to drive the currency picker without re-crawling Treasury's full history on every request — see `TechnicalDesign.md`'s changelog for the full rationale.
- Phase 1/2/3 updated to reflect the repository-pattern fix: no per-entity repository interfaces, entity-specific queries live directly on the concrete repository class.
- Phase 2/4 exit criteria updated to reflect unit-tests-only (no test host, every dependency mocked) rather than a separate integration-test category.
- Split Phase 5 (Front ends) into Phase 5.1 (Desktop/WinForms) and Phase 5.2 (Web/Blazor) so the Desktop app can be built and manually tested first, before starting the Blazor app.
- Phase 1 updated: `PurchaseTransaction` gained `TransactionNumber` as the actual database primary key, so users have a short sequential number to reference instead of a raw GUID; `Id` is unaffected everywhere else — see `DatabaseDesign.md`'s changelog.
