# Publishes self-contained, single-file win-x64 builds of Api, Web, and
# Desktop, stamps the Desktop build's config to point at the release API port,
# adds the Launch scripts, and zips the result into publish/ (git-ignored)
# ready to attach to a GitHub Release. The reviewer never runs this script;
# they only receive the zip it produces.

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$stageDir = Join-Path $repoRoot 'publish\stage'
$zipPath = Join-Path $repoRoot 'publish\Wex.PurchasingPlatform-Release.zip'
$templateDir = Join-Path $repoRoot 'scripts\release-template'

# Must match the ports hardcoded in scripts/release-template/Launch.ps1.
$apiBaseUrl = 'http://localhost:5080/'

function Publish-Project {
    param([string]$ProjectPath, [string]$OutputDir)

    Write-Host "Publishing $ProjectPath ..."
    dotnet publish $ProjectPath `
        -c Release `
        -r win-x64 `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -o $OutputDir

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed for $ProjectPath"
    }
}

if (Test-Path (Join-Path $repoRoot 'publish')) {
    Remove-Item (Join-Path $repoRoot 'publish') -Recurse -Force
}
New-Item -ItemType Directory -Path $stageDir | Out-Null

Publish-Project -ProjectPath (Join-Path $repoRoot 'src\Wex.PurchasingPlatform.Api\Wex.PurchasingPlatform.Api.csproj') -OutputDir (Join-Path $stageDir 'Api')
Publish-Project -ProjectPath (Join-Path $repoRoot 'src\Wex.PurchasingPlatform.Web\Wex.PurchasingPlatform.Web.csproj') -OutputDir (Join-Path $stageDir 'Web')
Publish-Project -ProjectPath (Join-Path $repoRoot 'src\Wex.PurchasingPlatform.Desktop\Wex.PurchasingPlatform.Desktop.csproj') -OutputDir (Join-Path $stageDir 'Desktop')

Write-Host 'Pointing the Desktop build at the release API port...'
$desktopSettingsPath = Join-Path $stageDir 'Desktop\appsettings.json'
$desktopSettings = Get-Content $desktopSettingsPath -Raw | ConvertFrom-Json
$desktopSettings.Api.BaseUrl = $apiBaseUrl
$desktopSettings | ConvertTo-Json -Depth 10 | Set-Content $desktopSettingsPath -Encoding utf8

Write-Host 'Adding launcher scripts...'
Copy-Item (Join-Path $templateDir 'Launch.ps1') $stageDir
Copy-Item (Join-Path $templateDir 'Launch.bat') $stageDir

Write-Host "Zipping release to $zipPath ..."
Compress-Archive -Path (Join-Path $stageDir '*') -DestinationPath $zipPath -Force

Write-Host ''
Write-Host "Done: $zipPath" -ForegroundColor Green
Write-Host 'Attach this zip to a GitHub Release.'
