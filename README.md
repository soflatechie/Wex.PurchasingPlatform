# WEX Purchasing Platform

A project implementing two requirements from the hiring exercise

## Project artifacts

Please see the following documents:

- [InitialDesignPrompt.md](Design/InitialDesignPrompt.md)
- [InitialRequirements.md](Design/InitialRequirements.md)
- [InitialDesign.md](Design/InitialDesign.md)
- [TechnicalDesign.md](Design/TechnicalDesign.md)
- [DatabaseDesign.png](Design/DatabaseDesign.png)
- [ClassDiagram.png](Design/ClassDiagram.png)
- [CodingStandards.md](Design/CodingStandards.md)

- [ClaudeUsage.md](Design/ClaudeUsage.md)


## Running it

### 1. Get the code

Clone the code to your local machine using the Code option above


### 2. Start the API
```
dotnet run --project src/Wex.PurchasingPlatform.Api
```

### 3. Start a front end

**Desktop (WinForms, Windows only):**

```
dotnet run --project src/Wex.PurchasingPlatform.Desktop
```

**Web (Blazor):**

```
dotnet run --project src/Wex.PurchasingPlatform.Web
```

- <https://localhost:7054>

## Exercising the requirements directly

If you'd rather skip the front ends, everything is reachable from Swagger at `https://localhost:7046/swagger`:


## Running the tests
```
dotnet test
```
