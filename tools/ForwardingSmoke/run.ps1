param(
    [Parameter(Mandatory = $true)]
    [string]$PackageFeed
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$feed = (Resolve-Path $PackageFeed).Path
$package = Join-Path $feed 'Icod.Expressions.0.1.0-alpha.1.nupkg'
if (-not (Test-Path $package)) { throw "Candidate package missing: $package" }

$scratch = Join-Path ([System.IO.Path]::GetTempPath()) ('icod-forwarding-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scratch | Out-Null
$previousPackages = $env:NUGET_PACKAGES
$env:NUGET_PACKAGES = Join-Path $scratch 'packages'
$config = Join-Path $scratch 'NuGet.Config'
$escapedFeed = [System.Security.SecurityElement]::Escape($feed)
@"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="candidate" value="$escapedFeed" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
"@ | Set-Content -Path $config -Encoding UTF8

function Invoke-CheckedDotnet {
    param([string[]]$Arguments)
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet exited $LASTEXITCODE`: $($Arguments -join ' ')" }
}

try {
    $clientProject = Join-Path $root 'tools/ForwardingSmoke/LegacyClient/LegacyClient.csproj'
    $bridgeProject = Join-Path $root 'tools/ForwardingSmoke/ThinBridge/ThinBridge.csproj'
    foreach ($project in @($clientProject, $bridgeProject)) {
        Invoke-CheckedDotnet -Arguments @('restore', $project, '-m:1', '--configfile', $config)
        Invoke-CheckedDotnet -Arguments @('build', $project, '-c', 'Staging', '-m:1', '--no-restore')
    }

    $clientOutput = Join-Path $root 'tools/ForwardingSmoke/LegacyClient/bin/Staging/net10.0'
    $bridgeDll = Join-Path $root 'tools/ForwardingSmoke/ThinBridge/bin/Staging/net10.0/Icod.CommandFramework.dll'
    $isolated = Join-Path $scratch 'client'
    New-Item -ItemType Directory -Path $isolated | Out-Null
    Copy-Item (Join-Path $clientOutput '*') -Destination $isolated -Recurse -Force
    $clientDll = Join-Path $isolated 'LegacyClient.dll'

    Invoke-CheckedDotnet -Arguments @($clientDll, '--original')
    Copy-Item $bridgeDll -Destination (Join-Path $isolated 'Icod.CommandFramework.dll') -Force
    Invoke-CheckedDotnet -Arguments @($clientDll, '--forwarded')

    Remove-Item (Join-Path $isolated 'Icod.Expressions.dll') -Force
    $negativeOutput = Join-Path $scratch 'missing-package.txt'
    $previousErrorAction = $ErrorActionPreference
    try {
        # A missing forwarded assembly must fail. PowerShell versions differ in
        # whether a nonzero native exit also becomes a terminating error.
        $ErrorActionPreference = 'Continue'
        & dotnet $clientDll --forwarded *> $negativeOutput
        $negativeExitCode = $LASTEXITCODE
    } finally {
        $ErrorActionPreference = $previousErrorAction
    }
    if ($negativeExitCode -eq 0) { throw 'Forwarded client unexpectedly ran without Icod.Expressions.dll.' }
    $failure = Get-Content $negativeOutput -Raw
    if (-not $failure.Contains('Icod.Expressions')) {
        throw "Missing-package control failed for an unrelated reason: $failure"
    }
    Write-Host 'Missing forwarded assembly rejected as expected.'
} finally {
    if ($null -eq $previousPackages) {
        Remove-Item Env:NUGET_PACKAGES -ErrorAction SilentlyContinue
    } else {
        $env:NUGET_PACKAGES = $previousPackages
    }
    Remove-Item $scratch -Recurse -Force -ErrorAction SilentlyContinue
}
exit 0
