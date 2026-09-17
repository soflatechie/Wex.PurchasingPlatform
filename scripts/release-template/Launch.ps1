# Starts the API, waits until it is actually ready to serve requests, then
# launches both client applications. Ports here must match the values used
# in scripts/Build-Release.ps1's Desktop appsettings.json rewrite step.

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$apiUrl = 'http://localhost:5080'
$webUrl = 'http://localhost:5090'
$readyProbeUrl = "$apiUrl/api/purchasetransactions"
$maxWaitSeconds = 30

function Wait-ForApi {
    param([string]$Url, [int]$TimeoutSeconds)

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        try {
            Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 2 | Out-Null
            return $true
        }
        catch {
            Start-Sleep -Milliseconds 500
        }
    }
    return $false
}

Write-Host 'Starting the Purchasing Platform API...'
$env:ASPNETCORE_URLS = $apiUrl
$apiProcess = Start-Process -FilePath (Join-Path $root 'Api\Wex.PurchasingPlatform.Api.exe') `
    -WorkingDirectory (Join-Path $root 'Api') -PassThru

Write-Host "Waiting for the API to be ready at $apiUrl (up to $maxWaitSeconds seconds)..."
$apiReady = Wait-ForApi -Url $readyProbeUrl -TimeoutSeconds $maxWaitSeconds

if (-not $apiReady) {
    Write-Host ''
    Write-Host "The API did not respond at $apiUrl within $maxWaitSeconds seconds." -ForegroundColor Red
    Write-Host 'Check the API console window for an error (a common cause is another program already using that port).' -ForegroundColor Red
    if (-not $apiProcess.HasExited) {
        Stop-Process -Id $apiProcess.Id -Force
    }
    Read-Host 'Press Enter to close this window'
    exit 1
}

Write-Host 'API is ready.'

Write-Host 'Starting the Desktop application...'
Start-Process -FilePath (Join-Path $root 'Desktop\Wex.PurchasingPlatform.Desktop.exe') `
    -WorkingDirectory (Join-Path $root 'Desktop')

Write-Host 'Starting the Web application...'
$env:ASPNETCORE_URLS = $webUrl
$env:Api__BaseUrl = "$apiUrl/"
$webProcess = Start-Process -FilePath (Join-Path $root 'Web\Wex.PurchasingPlatform.Web.exe') `
    -WorkingDirectory (Join-Path $root 'Web') -PassThru

Write-Host "Waiting for the Web app to be ready at $webUrl..."
Wait-ForApi -Url $webUrl -TimeoutSeconds $maxWaitSeconds | Out-Null

Start-Process $webUrl

Write-Host ''
Write-Host 'Everything is running:'
Write-Host "  API: $apiUrl (background service; use the Web or Desktop app to interact with it)"
Write-Host "  Web: $webUrl"
Write-Host '  Desktop: a separate window has opened.'
Write-Host ''
Read-Host 'Press Enter to stop the API and Web app (close the Desktop window yourself)'

if (-not $apiProcess.HasExited) { Stop-Process -Id $apiProcess.Id -Force }
if (-not $webProcess.HasExited) { Stop-Process -Id $webProcess.Id -Force }
