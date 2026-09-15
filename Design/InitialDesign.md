# WEX Purchasing Platform — Initial Design

## 1. Problem Statement

The client needs an application with two required capabilities:

1. **Store a purchase transaction** — description (≤ 50 chars), transaction date (valid date), and a purchase amount in USD (positive, rounded to the nearest cent). Each stored transaction is assigned a unique identifier.
2. **Retrieve a stored transaction converted into a foreign currency** — given a transaction id and a target country/currency supported by the [Treasury Reporting Rates of Exchange API](https://fiscaldata.treasury.gov/datasets/treasury-reporting-rates-exchange/treasury-reporting-rates-of-exchange), return the id, description, date, original USD amount, the exchange rate used, the date Treasury published that rate, whether the rate is stale relative to Treasury's own documented 3-month validity window, and the converted amount.

Two capabilities are added beyond what was strictly asked for, as a deliberate choice to round out the exercise rather than a misreading of scope (see §2.3 and §2.4 for the design of each):

3. **Update and delete a stored transaction** — not required by the client's text, but a natural extension of "store a purchase transaction" and cheap to add correctly once create/read exist.
4. **Authentication/authorization on the API** — not required for a take-home evaluated in a trusted context, but included specifically to demonstrate real OAuth 2.0 / JWT integration (Microsoft Entra ID) rather than leaving the API open, while staying toggle-able so a reviewer can still run the whole thing with zero external accounts.

Underlying hard constraints from the hiring manager:

- **Financial rigor** — correct data types and rounding at every step; this is money, not floating-point telemetry.
- **Smart conversion** — correct interpretation of "the rate active for the date of the purchase," including what to do when no rate exists for that exact date.
- **Production quality** — clean layering, validation, error handling, and a test suite that actually defends the business rules.
- **Plug and play** — no external database engine or app server to install; a reviewer should be able to clone and run it with one command.

## 2. Solution Approach

### 2.1 Shape of the solution

An API-first design: a single ASP.NET Core Web API is the source of truth for both use cases, fronted by one or more thin clients. This satisfies "multiple front ends" cheaply — every UI is just an HTTP client — and keeps the financial logic (validation, rounding, rate selection) in exactly one place instead of duplicated per client.

```
[Client(s)] --HTTP/JSON--> [Web API] --EF Core--> [SQLite file]
                                  |
                                  +--HTTP--> [Treasury Fiscal Data API] (rate lookups)
```

### 2.2 Core domain logic

**Storing a transaction**
- `Description`: required, ≤ 50 characters.
- `TransactionDate`: required, valid calendar date; stored as a date (no time-of-day/timezone ambiguity, since exchange rates are published per calendar day, not per instant).
- `PurchaseAmount`: required, `decimal`, > 0, rounded to 2 decimal places using `MidpointRounding.AwayFromZero` (standard currency rounding) at the boundary — the API rejects amounts that don't already round cleanly rather than silently truncating a client's typo.
- `Id`: server-generated `Guid` (v7 if available for index-friendliness), returned to the caller on create.

**Retrieving a converted transaction — rate selection rule**
The Treasury dataset's own documentation is explicit and is treated as authoritative here rather than as a starting guess: each quarterly release states its rates are for use "as of the date of this report and **for the ensuing three months**." That is the one real, sourced number available, so it is the number used — no separate assumption or configurable default is introduced in its place.

Selection rule:

> Use the most recent published rate (by `record_date`) that is **on or before** the transaction date. Compute its age as the number of months between `record_date` and the transaction date. The rate is **current** if that age is **≤ 3 months**, and **stale** otherwise — but it is used, and returned, either way.

Rationale: Requirement #2 asks for "the exchange rate active for the date of the purchase," not "reject the purchase if no exchange rate happens to still be officially current." The most recent rate on or before the transaction date *is* the correct rate to use under the Treasury's own publication scheme — its only documented distinction past 3 months is that Treasury no longer stands behind it as current, which is exactly the information the `isStale` flag surfaces to the caller instead of silently hiding it or blocking the conversion outright.

- **Response fields**: id, description, transaction date, USD amount, `exchangeRate`, `rateDate` (the Treasury `record_date` the rate was published under), `isStale` (`true` when `rateDate` is more than 3 months before the transaction date), and `convertedAmount`.
- **Genuine failure case**: if no published rate exists on or before the transaction date at all for the requested currency (e.g., the transaction predates the first time Treasury ever reported that currency, or the currency code doesn't appear in the dataset), conversion is impossible outright — the API returns `404`/`422` with a clear message. This is a distinct case from staleness: staleness means "we have an answer, flagged"; this means "we have no answer."
- Conversion math: `converted = round(purchaseAmount * exchangeRate, 2, AwayFromZero)`. Both operands are `decimal`; the exchange rate itself is stored/transmitted at full published precision, only the final converted total is rounded.

**Target currency selection — country and currency, not currency alone**
The Treasury `currency` field is a plain-language name, not a unique code, and it is not unique per country: "Pound" alone is ambiguous between (at least) the United Kingdom, Egypt, Lebanon, and Sudan, whose rates differ enormously; "Dollar" is similarly shared by Australia, Canada, Fiji, Jamaica, Singapore, and others. So the client selects **country and currency together** (equivalent to the dataset's `country_currency_desc`), not currency in isolation.

A plain two-level "country → any currency that country has ever used" cascade isn't quite enough on its own, though: several countries have used more than one currency over time or concurrently — every Eurozone country has a legacy pre-Euro currency plus the Euro (e.g., Germany: `Mark` then `Euro`), and a few (Zimbabwe, El Salvador, Ecuador) had genuinely overlapping currencies during a transition period. Since the transaction's date is already fixed by the time the user is picking a currency to convert into, the currency list offered for a given country should be **filtered to currencies that actually have a usable rate for that transaction's date** (i.e., at least one Treasury row on or before that date), rather than every currency that country has ever used. This avoids letting a user pick, say, "Mark" for a 2015 German transaction and hit a dead end.

### 2.3 Update and delete (added scope)

Not required, but added as `PUT /api/purchases/{id}` and `DELETE /api/purchases/{id}`, reusing exactly the same field validation as create (§2.2) — an update can't bypass the 50-character description limit or the positive-rounded-amount rule just because it's an edit instead of a create. Both return `404` for an unknown id. No soft-delete, audit trail, or optimistic-concurrency (ETag/`RowVersion`) handling is added — real production concerns, but beyond what this exercise needs, and easy to layer in later without reshaping anything already built.

### 2.4 Authentication and authorization (added scope)

Not required for a take-home evaluated by a hiring manager running it locally, but included to demonstrate real OAuth 2.0/OIDC integration rather than an unauthenticated API:

- **Identity provider**: Microsoft Entra ID (the OAuth 2.0/OpenID Connect platform formerly called Azure AD). An App Registration in a tenant exposes one custom scope (e.g., `Purchases.ReadWrite`) that callers must request.
- **API side**: standard **JWT bearer** validation via `Microsoft.Identity.Web`/`Microsoft.AspNetCore.Authentication.JwtBearer` — the API validates the token's signature, issuer, audience, and expiry against the tenant's OpenID Connect discovery document; no custom token logic is written. `[Authorize]` is applied to the purchase endpoints.
- **Client side**: each .NET front end acquires tokens via **MSAL.NET** — the Blazor app uses the standard web-app authorization-code flow (interactive browser sign-in redirect), the WinForms app uses MSAL's public-client interactive/broker flow (a native sign-in window, no browser redirect plumbing to build by hand).
- **Keeping it plug-and-play**: real Entra ID requires a real tenant and app registration, which a reviewer without one can't stand up on the spot. So authentication is behind a single configuration switch (e.g., `Authentication:Enabled`), **off by default** for local/reviewer runs (the API and both front ends work exactly as already described, with no sign-in step), and fully wired up to flip on with real Entra ID values dropped into configuration. This keeps the zero-external-account "clone and run" story intact while still having working, inspectable OAuth/JWT code in the repository, not just a description of how it would be added.

## 3. Technology Recommendations

| Concern | Recommendation | Why |
|---|---|---|
| API runtime | **.NET 10 / ASP.NET Core Web API** | Client's own suggestion; strong `decimal` support, mature EF Core, minimal APIs keep boilerplate low, single self-contained executable is easy to hand a reviewer. |
| Persistence | **EF Core + SQLite** (single `.db` file, `Microsoft.Data.Sqlite`) | Zero install, zero server process — the file ships inside the working directory or is created on first run. Meets "plug and play" directly; still real SQL/ACID, real migrations, real query surface. |
| Money & rate representation | `decimal` end-to-end (C# properties, EF column types, DTOs) — **never `double`/`float`** | Binary floating point cannot represent most base-10 currency amounts exactly; `decimal` is the only type in play that satisfies "financial rigor." |
| Exchange rate source | HTTP client (`IHttpClientFactory`) against the Treasury Fiscal Data API, wrapped behind an `IExchangeRateProvider` interface, with results **cached locally in SQLite** | The live Treasury API is a runtime dependency the requirements explicitly ask us to call — that's unavoidable and is not the same thing as an install-time server dependency. Caching each currency/quarter locally once fetched (a) makes the app resilient to Treasury API downtime or rate limiting on repeat lookups, and (b) makes tests deterministic without live network calls. |
| Validation | FluentValidation (or data annotations if the team prefers fewer dependencies) + RFC 7807 `ProblemDetails` error responses | Keeps validation rules declarative, testable in isolation, and gives clients machine-readable error shapes instead of bespoke error strings. |
| Auth (added scope) | **Microsoft Entra ID** OAuth 2.0/OIDC + JWT bearer validation (`Microsoft.Identity.Web`), MSAL.NET in the front ends, toggled off by default via config | Real, standard OAuth/JWT integration rather than a bespoke scheme, without forcing every reviewer to provision a tenant just to run the app (see §2.4). |
| API docs | Swagger / OpenAPI (`Microsoft.AspNetCore.OpenApi` + Swashbuckle or Scalar UI) | Free with ASP.NET Core; doubles as living documentation and a manual-testing UI for reviewers. |
| Architecture | Light layering: `Domain` (entities, value objects, rate-selection logic) → `Application` (use cases/services) → `Infrastructure` (EF Core, Treasury client) → `Api` (endpoints/DTOs) | Keeps the money math and rate-selection rule unit-testable without spinning up EF Core or HTTP, without over-engineering a small project into a full hexagonal/CQRS stack. |
| Testing | xUnit with plain `Assert` for unit tests (no FluentAssertions — its v8+ license requires a paid tier for larger organizations); `WebApplicationFactory<Program>` + SQLite (file or `:memory:` with a kept-open connection) for integration tests; a fake `IExchangeRateProvider` for deterministic rate-selection tests (including the boundary cases: exact-date match, 3-month edge, no rate available) | Rounding and rate-selection edge cases are exactly the kind of logic that silently rots without tests; the take-home explicitly asks for "the automated tests you consider essential." |

### 3.1 Front end(s)

The client suggested picking from console, Angular, WinForms, mobile, or a Python script, possibly more than one. Front ends are built as **separate projects in the same solution**, each a standalone process that talks to the API purely over HTTP — the same relationship any external client would have. This keeps the API as the single source of truth for validation/rounding/rate-selection logic, and makes adding a further front end later purely additive (no API changes required).

Recommendation:

1. **Windows Forms desktop app (.NET)** — chosen over a CLI because the eventual user's technical comfort level is unknown, and a form with labeled fields, a grid of stored transactions, and a currency dropdown is self-explanatory in a way a command-line tool is not. This directly serves the "ease of use" half of the client's hard requirement, not just portability. The trade-off is explicit: WinForms is Windows-only, which cuts against the "portability" half of that same requirement pairing. That's judged acceptable here because (a) a purchasing-transaction tool is plausibly an internal, Windows-desktop-only enterprise audience, and (b) the API itself remains fully cross-platform — only this one optional client is Windows-bound, and nothing about the API or the other front end depends on it.
2. **Minimal web UI** — a small **Blazor** app over Angular. Reasoning: Angular needs Node/npm as a separate build toolchain and a separate language/ecosystem, which cuts against "plug and play, no external dependencies" — a reviewer without Node installed can't build it. Blazor stays inside the .NET SDK the API already requires, and still demonstrates a real browser front end with data binding and forms, and (unlike WinForms) keeps at least one client cross-platform. If the client specifically wants to see Angular/React skill demonstrated, that's a reasonable one-line ask to add as a third client later — it's additive, not a rework, because the API doesn't change.

No CLI is planned — with a WinForms app covering the "quick, no-toolchain-needed" niche and Blazor covering the cross-platform/browser niche, a third console client wouldn't add coverage worth its maintenance cost. A native mobile app is not recommended either: it adds an entire platform SDK/toolchain for a take-home whose value is in the API's correctness, not its UI reach.

### 3.2 Solution structure

```
Wex.PurchasingPlatform.slnx
├── src/
│   ├── Wex.PurchasingPlatform.Api        (the whole server side — controllers, services, repositories, entities, EF Core, Treasury client, auth — organized as folders within one project; see TechnicalDesign.md §2)
│   ├── Wex.PurchasingPlatform.Models     (DTOs shared by the API and both front ends — no logic)
│   ├── Wex.PurchasingPlatform.Desktop    (WinForms front end, net10.0-windows, references Models)
│   └── Wex.PurchasingPlatform.Web        (Blazor front end, references Models)
└── tests/
    └── Wex.PurchasingPlatform.Tests      (all unit and integration tests, one project)
```

Running it stays dependency-free: each project is started independently (`dotnet run --project ...`), or Visual Studio's "multiple startup projects" launches the API and a front end together with one F5. Nothing here requires anything beyond the .NET SDK.

## 4. Testing Strategy (essential coverage)

- **Unit — validation**: description length boundary (50/51 chars), non-date input, zero/negative/rounding-ambiguous amounts (e.g. `10.005`).
- **Unit — rate selection**: exact-date rate exists; nearest prior rate is chosen over an older one when multiple predate the transaction; rate exactly at the 3-month boundary (not stale) vs. one day past it (stale); rate far older than 3 months is still returned with `isStale = true` rather than rejected; no rate at all on or before the transaction date → error; requested currency absent from dataset entirely → error.
- **Unit — conversion math**: rounding direction on `.5` cent boundaries; large amounts don't overflow/lose precision.
- **Integration**: POST a transaction → 201 with id; GET the same transaction in a supported currency → correct converted amount and echoed rate; GET in an unsupported/unknown currency → 4xx; GET a nonexistent id → 404; PUT an existing id with valid/invalid fields → 200/400; PUT a nonexistent id → 404; DELETE an existing id then GET it → 404.
- **Contract/fake**: Treasury client tested against a fixture of real sample API responses so a schema change in the live API is caught without depending on network access in CI.
- **Auth**: with `Authentication:Enabled=true` in a test host, a request without a token → 401, with a token missing the required scope → 403, with a valid token (issued by a fake/test JWT issuer, not a live Entra tenant) → 200; with `Authentication:Enabled=false`, endpoints remain open, confirming the toggle actually gates access rather than just being decorative.

## 5. Decisions and Open Questions

### 5.1 Resolved

- **Staleness threshold** — matches Treasury's own documented 3-month validity window exactly (see §2.2): the most recent rate on or before the transaction date is always used and returned, flagged `isStale` when it's more than 3 months old. No conversion is ever blocked purely for being stale.
- **Currency identifier** — the client selects **country and currency together**, not currency alone (see §2.2), because the Treasury `currency` field is not unique per country (e.g., "Pound" spans the UK, Egypt, Lebanon, Sudan). The currency options offered for a chosen country are filtered to what's actually usable for the transaction's date, to account for countries with legacy/transitional currencies (Eurozone predecessors, Zimbabwe, El Salvador, Ecuador).
- **Update/delete** — added, though not required by the client's text (see §2.3). `PUT`/`DELETE` on a stored transaction, reusing create's validation rules exactly.
- **Auth** — added, though not required for this evaluation context (see §2.4). Microsoft Entra ID OAuth 2.0/OIDC with JWT bearer validation, toggled off by default so the app still runs with zero external accounts.

### 5.2 Still Open

1. **Rounding convention** — is `MidpointRounding.AwayFromZero` (standard "round half up" for currency) acceptable, or does the client require banker's rounding (`ToEven`) to match a specific accounting system?

## Changelog

- Staleness threshold resolved to Treasury's documented 3-month validity window.
- Currency selection resolved to country + currency together, filtered to what's usable as of the transaction's date.
- Update/delete and Microsoft Entra ID authentication added to scope, beyond what the client's requirements strictly asked for.
- Front end changed from a CLI to a Windows Forms desktop app; Angular replaced with Blazor.
