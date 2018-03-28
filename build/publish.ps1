# Publishes our build assets to nuget, myget, dotnet/versions, etc ..
#
# The publish operation is best visioned as an optional yet repeatable post build operation. It can be
# run anytime after build or automatically as a post build step. But it is an operation that focuses on
# build outputs and hence can't rely on source code from the build being available
#
# Repeatable is important here because we have to assume that publishes can and will fail with some
# degree of regularity.
[CmdletBinding(PositionalBinding = $false)]
Param(
    # Standard options
    [string]$nugetPath = "",
    [string]$uploadUrl = "",
    [switch]$test,

    # Credentials
    [string]$apiKey = ""
)

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

# Publish the nuget packages to the specified URL
function Publish-Nuget() {
    Write-Host "Publishing nuget to $uploadUrl"
    if (-not (Test-Path $nugetPath)) {
        throw "nuget $nugetPath does not exist"
    }

    Write-Host "  Publishing '$nugetPath'"
    if (-not $test) {
        $response = Invoke-WebRequest -Uri $uploadUrl -Headers @{"X-NuGet-ApiKey" = $apiKey} -ContentType 'multipart/form-data' -InFile $nugetPath -Method Post -UseBasicParsing
        if ($response.StatusCode -ne 201) {
            throw "Failed to upload nuget package: $nugetPath. Upload failed with Status code: $response.StatusCode"
        }
    }
}

try {
    if ($nugetPath -eq "") {
        Write-Host "Must provide the path to the nuget package with -nugetPath"
        exit 1
    }

    if ($uploadUrl -eq "") {
        Write-Host "Must provide the URL to upload the nuget package to -uploadUrl"
        exit 1
    }

    Publish-Nuget
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}
