# Implementation Phases

Companion to [`TechnicalDesign.md`](./TechnicalDesign.md). Each phase produces something runnable/testable before the next begins — no phase depends on code that doesn't exist yet further down the list.

## Phase 0 — Solution scaffolding

- Create `Wex.PurchasingPlatform.slnx` and the empty projects from `TechnicalDesign.md` §1 (`Api`, `Models`, `Desktop`, `Web`, and `Tests`), with `Api`'s internal folders from §2 stubbed out (`Controllers/`, `Services/`, `Repositories/`, `Entities/`, `Data/`, `ExternalServices/`, `Validation/`).
- `Api` boots with Swagger UI reachable, no endpoints yet, and `Microsoft.Extensions.Logging` (Console/Debug providers, the ASP.NET Core default) already active — see `TechnicalDesign.md` §9.
- **Exit criteria**: `dotnet build` succeeds across the whole solution; `dotnet run --project src/Wex.PurchasingPlatform.Api` serves `/swagger`.

## Phase 1 — Persistence

- `PurchaseTransaction` and `ExchangeRateQuote` in `Api/Entities/`.
- `AppDbContext`, EF Core configuration (decimal precision, unique index on `ExchangeRateQuote`), and the first migration, per `DatabaseDesign.md`, in `Api/Data/`.
- `IRepository<TEntity>` + `Repository<TEntity>`, then `IPurchaseTransactionRepository`/`PurchaseTransactionRepository` and `IExchangeRateRepository`/`ExchangeRateRepository`, in `Api/Repositories/`.
- **Exit criteria**: running the app creates `purchasing.db` with both tables; repository tests (in `Tests`) pass against a real SQLite file.

## Phase 2 — Purchase transaction CRUD (Requirement #1 + added update/delete scope)

- `Models`: `CreatePurchaseTransactionRequest`, `UpdatePurchaseTransactionRequest`, `PurchaseTransactionDto`.
- `Api/Validation/`: FluentValidation validators (50-char description, positive rounded amount, valid date). `Api/Services/`: `PurchaseTransactionService`.
- `Api/Controllers/`: `PurchaseTransactionsController` — `POST`/`GET`/`GET {id}`/`PUT {id}`/`DELETE {id}`, all through the service only.
- **Exit criteria**: Requirement #1 fully satisfied and demonstrable via Swagger; unit tests on validation boundaries; integration tests on all five CRUD routes (including 404s).

## Phase 3 — Treasury exchange rate integration

- `Api/ExternalServices/`: `IExchangeRateProvider` + `TreasuryExchangeRateClient` (HTTP client against the Fiscal Data API).
- `IExchangeRateRepository.GetLatestRecordDateAsync()` (a `MAX(RecordDate)` query); `ExchangeRateSyncService` (reads that watermark, pulls anything newer from `IExchangeRateProvider`, upserts into `ExchangeRateRepository`) and `ExchangeRateSyncHostedService` to run it once at API startup, both in `Api/Services/` — see `TechnicalDesign.md` §5 and §8.1.
- `SelectRate` logic in `CurrencyConversionService`, reading only from the already-synced `ExchangeRateRepository`: most recent rate on or before the transaction date, `isStale` past Treasury's documented 3-month window — see `TechnicalDesign.md` §5 / `InitialDesign.md` §2.2.
- `POST /api/exchange-rates/sync` for on-demand manual re-sync (`TechnicalDesign.md` §6).
- Pagination loop in `TreasuryExchangeRateClient` and batched bulk-insert in the sync path (`TechnicalDesign.md` §10) — verified against the real ~15–20K-row full history on first run, not just a small fixture.
- **Exit criteria**: a fresh run of the app performs one full sync on first startup (populating `ExchangeRateQuote` with no user action) in a few seconds, not minutes; a second startup immediately after performs an incremental no-op sync; the app still starts and Requirement #1 endpoints still work if the initial sync fails (e.g., no network); unit tests over `SelectRate` covering the edge cases in `InitialDesign.md` §4 (exact-date match, 3-month boundary, older-but-still-returned, no rate at all) using pre-seeded local data — no live network calls in the test suite.

## Phase 4 — Currency conversion endpoint (Requirement #2)

- `Models`: `ConvertedPurchaseTransactionDto`, `CurrencyOptionDto`.
- `Api/Controllers/`: `GET /api/purchasetransactions/{id}/conversion` and `GET /api/currencies` (country/currency options filtered to what's usable as of a given transaction date, per `InitialDesign.md` §2.2's Eurozone/legacy-currency discussion).
- **Exit criteria**: Requirement #2 fully satisfied and demonstrable via Swagger against live Treasury data; integration tests for supported currency, unknown currency, and nonexistent transaction id.

## Phase 5 — Authentication (added scope)

- Entra ID app registration (dev tenant), JWT bearer validation wired into `Api` behind `Authentication:Enabled` (default `false`).
- MSAL.NET token acquisition in each front end's own `ApiClient/` folder so it can attach a bearer token when auth is turned on.
- **Exit criteria**: with the flag on, unauthenticated calls get `401`/`403` and a valid test-issued token gets `200`; with the flag off (the reviewer default), nothing in Phases 0–4 changes behavior.

## Phase 6 — Front ends

- `Desktop` (WinForms): transaction list/grid, create/edit form, delete confirmation, country→currency cascading picker (filtered by the selected transaction's date via `GET /api/currencies`), conversion display showing rate, rate date, and a visible staleness indicator.
- `Web` (Blazor): the same set of screens, browser-based.
- Each front end's `ApiClient/` folder holds its own thin HTTP wrapper around the `Api` endpoints, using `Models` for request/response types.
- **Exit criteria**: both front ends can complete the full flow — create a transaction, list it, edit it, convert it to a foreign currency and see the staleness indicator, delete it — against a running `Api` instance.

## Phase 7 — Polish and hand-off

- Log level/message consistency pass across services and the exception-handling middleware; final pass on `ProblemDetails` error responses for consistency across all endpoints.
- `README.md` at the repo root: how to run the API and each front end, how to toggle auth on, and the still-open rounding-convention question surfaced prominently for the reviewer.
- Full test suite run and coverage sanity check on the money/rate-selection logic specifically (the areas called out as "essential" in `InitialDesign.md` §4).
- **Exit criteria**: a reviewer can clone the repo, run one command per process (API + a front end), and exercise both requirements end to end with zero configuration.

## Changelog

- Rewritten for the simplified solution structure: `Domain`/`Contracts`/`Application`/`Infrastructure` collapsed into `Api` (folders), `Contracts` renamed `Models`, four test projects collapsed into one `Tests`, shared `ApiClient` project removed in favor of a per-front-end `ApiClient/` folder.
