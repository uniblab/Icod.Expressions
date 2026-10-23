param(
    [Parameter(Mandatory = $true)]
    [string]$PackageFeed
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$feed = (Resolve-Path $PackageFeed).Path
$package = Join-Path $feed 'Icod.Expressions.0.1.0-alpha.1.nupkg'
if (-not (Test-Path $package)) { throw "Candidate package missing: $package" }

# macOS aliases /var to /private/var. Keep project restores and builds under
# one physical checkout path so MSBuild sees the same project references.
$scratch = Join-Path $root ('artifacts/consumer-bridge-' + [guid]::NewGuid().ToString('N'))
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

function Invoke-Checked {
    param([string]$Executable, [string[]]$Arguments)
    & $Executable @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$Executable exited $LASTEXITCODE`: $($Arguments -join ' ')"
    }
}

function Fetch-PinnedSource {
    param([string]$Target, [string]$Url, [string]$Revision)
    New-Item -ItemType Directory -Path $Target | Out-Null
    Invoke-Checked -Executable 'git' -Arguments @('-C', $Target, 'init', '-q')
    Invoke-Checked -Executable 'git' -Arguments @('-C', $Target, 'fetch', '-q', '--depth', '1', $Url, $Revision)
    Invoke-Checked -Executable 'git' -Arguments @('-C', $Target, 'checkout', '-q', '--detach', 'FETCH_HEAD')
    $actual = (& git -C $Target rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0 -or $actual -ne $Revision) {
        throw "Pinned source mismatch for $Url`: expected $Revision, got $actual"
    }
}

function Assert-FiftyPassingTests {
    param([string]$TrxPath)
    [xml]$results = Get-Content $TrxPath -Raw
    $counters = $results.SelectSingleNode("//*[local-name()='Counters']")
    if ($null -eq $counters -or [int]$counters.GetAttribute('total') -ne 50 -or
        [int]$counters.GetAttribute('passed') -ne 50 -or
        [int]$counters.GetAttribute('failed') -ne 0) {
        throw "CoreUtils expr test count or result changed: $TrxPath"
    }
}

try {
    $frameworkRoot = Join-Path $scratch 'Icod.CommandFramework'
    $coreutilsRoot = Join-Path $scratch 'Icod.CoreUtils'
    Fetch-PinnedSource -Target $frameworkRoot -Url 'https://github.com/uniblab/Icod.CommandFramework.git' `
        -Revision 'e1e118f18246eed290bee4758325a20e302bfb92'
    Fetch-PinnedSource -Target $coreutilsRoot -Url 'https://github.com/uniblab/Icod.CoreUtils.git' `
        -Revision '2c47cd67bae490dedf16234d8460c8b9da8d3404'

    $exprTests = Join-Path $coreutilsRoot 'tests/Expr.Tests/Icod.CoreUtils.Expr.Tests.csproj'
    $originalResults = Join-Path $scratch 'original-results'
    Invoke-Checked -Executable 'dotnet' -Arguments @('restore', $exprTests, '-m:1', '--configfile', $config)
    Invoke-Checked -Executable 'dotnet' -Arguments @('test', $exprTests, '-c', 'Staging', '--no-restore',
        '-m:1', '--logger', 'trx;LogFileName=original.trx', '--results-directory', $originalResults)
    Assert-FiftyPassingTests -TrxPath (Join-Path $originalResults 'original.trx')
    $originalOutput = Join-Path $coreutilsRoot 'tests/Expr.Tests/bin/Staging/net10.0'

    $bridgeProject = Join-Path $root 'tools/ConsumerBridgeProbe/Bridge/Bridge.csproj'
    $bridgeRootProperty = "-p:CommandFrameworkRoot=$frameworkRoot"
    Invoke-Checked -Executable 'dotnet' -Arguments @('restore', $bridgeProject, '-m:1',
        $bridgeRootProperty, '--configfile', $config)
    Invoke-Checked -Executable 'dotnet' -Arguments @('build', $bridgeProject, '-c', 'Staging',
        '--no-restore', '-m:1', $bridgeRootProperty)
    $bridgeDll = Join-Path $root 'tools/ConsumerBridgeProbe/Bridge/bin/Staging/net10.0/Icod.CommandFramework.dll'

    $probeProject = Join-Path $root 'tools/ConsumerBridgeProbe/Probe/Probe.csproj'
    $probeOutputProperty = "-p:OriginalConsumerOutput=$originalOutput"
    Invoke-Checked -Executable 'dotnet' -Arguments @('restore', $probeProject, '-m:1',
        $probeOutputProperty, '--configfile', $config)
    Invoke-Checked -Executable 'dotnet' -Arguments @('build', $probeProject, '-c', 'Staging',
        '--no-restore', '-m:1', $probeOutputProperty)
    $probeOutput = Join-Path $root 'tools/ConsumerBridgeProbe/Probe/bin/Staging/net10.0'
    $probeDll = Join-Path $probeOutput 'Probe.dll'

    $negativeOutput = Join-Path $scratch 'unbridged-probe.txt'
    $previousErrorAction = $ErrorActionPreference
    try {
        $ErrorActionPreference = 'Continue'
        & dotnet $probeDll *> $negativeOutput
        $negativeExitCode = $LASTEXITCODE
    } finally {
        $ErrorActionPreference = $previousErrorAction
    }
    if ($negativeExitCode -eq 0 -or
        -not (Get-Content $negativeOutput -Raw).Contains('Unexpected assembly origins: regex=')) {
        throw "Unbridged identity control failed: $(Get-Content $negativeOutput -Raw)"
    }
    Copy-Item $bridgeDll -Destination (Join-Path $probeOutput 'Icod.CommandFramework.dll') -Force
    Invoke-Checked -Executable 'dotnet' -Arguments @($probeDll)

    $bridgedOutput = Join-Path $scratch 'ExprTests'
    New-Item -ItemType Directory -Path $bridgedOutput | Out-Null
    Copy-Item (Join-Path $originalOutput '*') -Destination $bridgedOutput -Recurse -Force
    Copy-Item $bridgeDll -Destination (Join-Path $bridgedOutput 'Icod.CommandFramework.dll') -Force
    Copy-Item (Join-Path $probeOutput 'Icod.Expressions.dll') -Destination $bridgedOutput -Force
    $bridgedResults = Join-Path $scratch 'bridged-results'
    Invoke-Checked -Executable 'dotnet' -Arguments @('vstest',
        (Join-Path $bridgedOutput 'Icod.CoreUtils.Expr.Tests.dll'),
        '--Logger:trx;LogFileName=bridged.trx', "--ResultsDirectory:$bridgedResults")
    Assert-FiftyPassingTests -TrxPath (Join-Path $bridgedResults 'bridged.trx')
    Write-Host 'Original CoreUtils expr binaries passed 50/50 with published and package-backed bridge assemblies.'
} finally {
    if ($null -eq $previousPackages) {
        Remove-Item Env:NUGET_PACKAGES -ErrorAction SilentlyContinue
    } else {
        $env:NUGET_PACKAGES = $previousPackages
    }
    Remove-Item $scratch -Recurse -Force -ErrorAction SilentlyContinue
}
exit 0
