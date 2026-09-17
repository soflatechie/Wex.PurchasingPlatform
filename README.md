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


## Running it - Installing from zip file
1. Get the code from Github

2. Go to the /Publish folder.  Copy the Wex.PurchasingPlatform-Release.zip file that you see there to somewhere outside of the repo.  

3. Extract the zip file and run the launch.bat file by clicking it or from the command line.  This will launch the api and both of the client apps.  If you have issues, you may have to run your command as administrator.


## Running it - Using Repo code

### 1. Get the code

Clone the code to your local machine using the Code option above.

### 2. One-time machine setup: trust the HTTPS dev certificate

Everything below runs over HTTPS on `localhost`. If you have never used a .NET HTTPS dev certificate on this machine, run this once:

```
dotnet dev-certs https --trust
```

Without this step, your browser will show a certificate warning (or block the page) when you open Swagger or the web app. You can confirm the certificate is trusted at any time with `dotnet dev-certs https --check --trust`.

### 3. Start the API

In its own terminal window, from the repository root:

```
dotnet run --project src/Wex.PurchasingPlatform.Api --launch-profile https
```

Wait for `Now listening on: https://localhost:7046` in the output before continuing. **Leave this terminal open and running** — it will not print anything further, and closing it or pressing Ctrl+C stops the API. This is expected; a server process is supposed to sit there idle while it serves requests.

`dotnet run` does **not** open a browser automatically (that only happens when launching from Visual Studio/VS Code's debugger). Once you see the "Now listening" line above, manually open a browser and go to:
- Swagger UI: <https://localhost:7046/swagger>

### 4. Start a front end

Open a **second, separate** terminal window (the API must still be running in the first one) and start one of the following:

**Desktop (WinForms, Windows only):**

```
dotnet run --project src/Wex.PurchasingPlatform.Desktop
```

**Web (Blazor):**

```
dotnet run --project src/Wex.PurchasingPlatform.Web --launch-profile https
```

Wait for `Now listening on: https://localhost:7054` in the output, then manually open a browser and go to:
- <https://localhost:7054>


### Troubleshooting

- **`Failed to bind to address ... address already in use`**: an API or Web instance is already running (in another terminal, or left running from Visual Studio). Stop that instance, or close and reopen the terminal, then try again.
- **Browser shows a certificate warning / refuses to connect**: you skipped step 2 — run `dotnet dev-certs https --trust`.
- **Web app or Desktop app can't reach the API**: confirm the API terminal shows `Now listening on: https://localhost:7046` — both front ends are configured to call the API at that exact URL.

## Exercising the requirements directly

If you'd rather skip the front ends, everything is reachable from Swagger once the API is running (step 3 above):

- <https://localhost:7046/swagger>


## Running the tests
```
dotnet test
```
