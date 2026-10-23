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

function Assert-PassingTests {
    param([string]$TrxPath, [int]$ExpectedCount)
    [xml]$results = Get-Content $TrxPath -Raw
    $counters = $results.SelectSingleNode("//*[local-name()='Counters']")
    if ($null -eq $counters -or [int]$counters.GetAttribute('total') -ne $ExpectedCount -or
        [int]$counters.GetAttribute('passed') -ne $ExpectedCount -or
        [int]$counters.GetAttribute('failed') -ne 0) {
        throw "Consumer test count or result changed: expected $ExpectedCount passes in $TrxPath"
    }
}

try {
    $frameworkRoot = Join-Path $scratch 'Icod.CommandFramework'
    $coreutilsRoot = Join-Path $scratch 'Icod.CoreUtils'
    Fetch-PinnedSource -Target $frameworkRoot -Url 'https://github.com/uniblab/Icod.CommandFramework.git' `
        -Revision 'e1e118f18246eed290bee4758325a20e302bfb92'
    Fetch-PinnedSource -Target $coreutilsRoot -Url 'https://github.com/uniblab/Icod.CoreUtils.git' `
        -Revision '2c47cd67bae490dedf16234d8460c8b9da8d3404'
    $procpsRoot = Join-Path $scratch 'Icod.ProcPs'
    Fetch-PinnedSource -Target $procpsRoot -Url 'https://github.com/uniblab/Icod.ProcPs.git' `
        -Revision 'e802ac03d941b1ba9fbe30e8f9c68fc7efcf3cff'
    $diffutilsRoot = Join-Path $scratch 'Icod.DiffUtils'
    Fetch-PinnedSource -Target $diffutilsRoot -Url 'https://github.com/uniblab/Icod.DiffUtils.git' `
        -Revision '8192ec43e1f6add42bc759b225c814861e18c843'
    $grepRoot = Join-Path $scratch 'Icod.Grep'
    Fetch-PinnedSource -Target $grepRoot -Url 'https://github.com/uniblab/Icod.Grep.git' `
        -Revision '2ef1c11f11b77eb876242eb0c92a88d20857db64'
    $lineEditorRoot = Join-Path $scratch 'Icod.LineEditor'
    Fetch-PinnedSource -Target $lineEditorRoot -Url 'https://github.com/uniblab/Icod.LineEditor.git' `
        -Revision '1705e1dc5107eb2ede6b5ecec5dad7ea18c93061'

    $exprTests = Join-Path $coreutilsRoot 'tests/Expr.Tests/Icod.CoreUtils.Expr.Tests.csproj'
    $originalResults = Join-Path $scratch 'original-results'
    Invoke-Checked -Executable 'dotnet' -Arguments @('restore', $exprTests, '-m:1', '--configfile', $config)
    Invoke-Checked -Executable 'dotnet' -Arguments @('test', $exprTests, '-c', 'Staging', '--no-restore',
        '-m:1', '--logger', 'trx;LogFileName=original.trx', '--results-directory', $originalResults)
    Assert-PassingTests -TrxPath (Join-Path $originalResults 'original.trx') -ExpectedCount 50
    $originalOutput = Join-Path $coreutilsRoot 'tests/Expr.Tests/bin/Staging/net10.0'

    $coreutilsSuites = @(
        @{ Name = 'NL'; ExpectedCases = 35 },
        @{ Name = 'CSplit'; ExpectedCases = 43 },
        @{ Name = 'Ptx'; ExpectedCases = 46 }
    )
    foreach ($suite in $coreutilsSuites) {
        $name = $suite.Name
        $project = Join-Path $coreutilsRoot "tests/$name.Tests/Icod.CoreUtils.$name.Tests.csproj"
        $results = Join-Path $scratch "CoreUtils.$name-original-results"
        Invoke-Checked -Executable 'dotnet' -Arguments @('restore', $project, '-m:1', '--configfile', $config)
        Invoke-Checked -Executable 'dotnet' -Arguments @('test', $project, '-c', 'Staging', '--no-restore',
            '-m:1', '--logger', 'trx;LogFileName=original.trx', '--results-directory', $results)
        Assert-PassingTests -TrxPath (Join-Path $results 'original.trx') -ExpectedCount $suite.ExpectedCases
    }

    $procpsSuites = @(
        @{ Name = 'Pgrep'; ExpectedCases = 11 },
        @{ Name = 'Pkill'; ExpectedCases = 7 },
        @{ Name = 'PidWait'; ExpectedCases = 6 }
    )
    foreach ($suite in $procpsSuites) {
        $name = $suite.Name
        $project = Join-Path $procpsRoot "tests/ProcPs.$name.Tests/Icod.ProcPs.$name.Tests.csproj"
        $results = Join-Path $scratch "ProcPs.$name-original-results"
        Invoke-Checked -Executable 'dotnet' -Arguments @('restore', $project, '-m:1', '--configfile', $config)
        Invoke-Checked -Executable 'dotnet' -Arguments @('test', $project, '-c', 'Staging', '--no-restore',
            '-m:1', '--logger', 'trx;LogFileName=original.trx', '--results-directory', $results)
        Assert-PassingTests -TrxPath (Join-Path $results 'original.trx') -ExpectedCount $suite.ExpectedCases
    }

    $diffSuites = @(
        @{ Name = 'Diff'; ExpectedCases = 22 },
        @{ Name = 'SDiff'; ExpectedCases = 40 }
    )
    foreach ($suite in $diffSuites) {
        $name = $suite.Name
        $project = Join-Path $diffutilsRoot "tests/$name.Tests/Icod.DiffUtils.$name.Tests.csproj"
        $results = Join-Path $scratch "$name-original-results"
        Invoke-Checked -Executable 'dotnet' -Arguments @('restore', $project, '-m:1', '--configfile', $config)
        Invoke-Checked -Executable 'dotnet' -Arguments @('test', $project, '-c', 'Staging', '--no-restore',
            '-m:1', '--logger', 'trx;LogFileName=original.trx', '--results-directory', $results)
        Assert-PassingTests -TrxPath (Join-Path $results 'original.trx') -ExpectedCount $suite.ExpectedCases
    }

    $grepProject = Join-Path $grepRoot 'tests/Grep.Tests/Icod.Grep.Tests.csproj'
    $grepOriginalResults = Join-Path $scratch 'Grep-original-results'
    Invoke-Checked -Executable 'dotnet' -Arguments @('restore', $grepProject, '-m:1', '--configfile', $config)
    Invoke-Checked -Executable 'dotnet' -Arguments @('test', $grepProject, '-c', 'Staging', '--no-restore',
        '-m:1', '--logger', 'trx;LogFileName=original.trx', '--results-directory', $grepOriginalResults)
    Assert-PassingTests -TrxPath (Join-Path $grepOriginalResults 'original.trx') -ExpectedCount 113

    $lineEditorSuites = @(
        @{ Name = 'Sed'; ExpectedCases = 116 },
        @{ Name = 'Ed.Shared'; ExpectedCases = 68 },
        @{ Name = 'Ed'; ExpectedCases = 26 },
        @{ Name = 'Red'; ExpectedCases = 28 }
    )
    foreach ($suite in $lineEditorSuites) {
        $name = $suite.Name
        $project = Join-Path $lineEditorRoot "tests/$name.Tests/Icod.LineEditor.$name.Tests.csproj"
        $results = Join-Path $scratch "$name-original-results"
        Invoke-Checked -Executable 'dotnet' -Arguments @('restore', $project, '-m:1', '--configfile', $config)
        Invoke-Checked -Executable 'dotnet' -Arguments @('test', $project, '-c', 'Staging', '--no-restore',
            '-m:1', '--logger', 'trx;LogFileName=original.trx', '--results-directory', $results)
        Assert-PassingTests -TrxPath (Join-Path $results 'original.trx') -ExpectedCount $suite.ExpectedCases
    }

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
    Assert-PassingTests -TrxPath (Join-Path $bridgedResults 'bridged.trx') -ExpectedCount 50

    foreach ($suite in $coreutilsSuites) {
        $name = $suite.Name
        $publishedOutput = Join-Path $coreutilsRoot "tests/$name.Tests/bin/Staging/net10.0"
        $substitutedOutput = Join-Path $scratch "CoreUtils.$name.Tests"
        New-Item -ItemType Directory -Path $substitutedOutput | Out-Null
        Copy-Item (Join-Path $publishedOutput '*') -Destination $substitutedOutput -Recurse -Force
        Copy-Item $bridgeDll -Destination (Join-Path $substitutedOutput 'Icod.CommandFramework.dll') -Force
        Copy-Item (Join-Path $probeOutput 'Icod.Expressions.dll') -Destination $substitutedOutput -Force
        $results = Join-Path $scratch "CoreUtils.$name-bridged-results"
        Invoke-Checked -Executable 'dotnet' -Arguments @('vstest',
            (Join-Path $substitutedOutput "Icod.CoreUtils.$name.Tests.dll"),
            '--Logger:trx;LogFileName=bridged.trx', "--ResultsDirectory:$results")
        Assert-PassingTests -TrxPath (Join-Path $results 'bridged.trx') -ExpectedCount $suite.ExpectedCases
    }

    foreach ($suite in $procpsSuites) {
        $name = $suite.Name
        $publishedOutput = Join-Path $procpsRoot "tests/ProcPs.$name.Tests/bin/Staging/net10.0"
        $substitutedOutput = Join-Path $scratch "ProcPs.$name.Tests"
        New-Item -ItemType Directory -Path $substitutedOutput | Out-Null
        Copy-Item (Join-Path $publishedOutput '*') -Destination $substitutedOutput -Recurse -Force
        Copy-Item $bridgeDll -Destination (Join-Path $substitutedOutput 'Icod.CommandFramework.dll') -Force
        Copy-Item (Join-Path $probeOutput 'Icod.Expressions.dll') -Destination $substitutedOutput -Force
        $results = Join-Path $scratch "ProcPs.$name-bridged-results"
        Invoke-Checked -Executable 'dotnet' -Arguments @('vstest',
            (Join-Path $substitutedOutput "Icod.ProcPs.$name.Tests.dll"),
            '--Logger:trx;LogFileName=bridged.trx', "--ResultsDirectory:$results")
        Assert-PassingTests -TrxPath (Join-Path $results 'bridged.trx') -ExpectedCount $suite.ExpectedCases
    }

    foreach ($suite in $diffSuites) {
        $name = $suite.Name
        $publishedOutput = Join-Path $diffutilsRoot "tests/$name.Tests/bin/Staging/net10.0"
        $substitutedOutput = Join-Path $scratch "$name.Tests"
        New-Item -ItemType Directory -Path $substitutedOutput | Out-Null
        Copy-Item (Join-Path $publishedOutput '*') -Destination $substitutedOutput -Recurse -Force
        Copy-Item $bridgeDll -Destination (Join-Path $substitutedOutput 'Icod.CommandFramework.dll') -Force
        Copy-Item (Join-Path $probeOutput 'Icod.Expressions.dll') -Destination $substitutedOutput -Force
        $results = Join-Path $scratch "$name-bridged-results"
        Invoke-Checked -Executable 'dotnet' -Arguments @('vstest',
            (Join-Path $substitutedOutput "Icod.DiffUtils.$name.Tests.dll"),
            '--Logger:trx;LogFileName=bridged.trx', "--ResultsDirectory:$results")
        Assert-PassingTests -TrxPath (Join-Path $results 'bridged.trx') -ExpectedCount $suite.ExpectedCases
    }

    $grepPublishedOutput = Join-Path $grepRoot 'tests/Grep.Tests/bin/Staging/net10.0'
    $grepBridgedOutput = Join-Path $scratch 'Grep.Tests'
    New-Item -ItemType Directory -Path $grepBridgedOutput | Out-Null
    Copy-Item (Join-Path $grepPublishedOutput '*') -Destination $grepBridgedOutput -Recurse -Force
    Copy-Item $bridgeDll -Destination (Join-Path $grepBridgedOutput 'Icod.CommandFramework.dll') -Force
    Copy-Item (Join-Path $probeOutput 'Icod.Expressions.dll') -Destination $grepBridgedOutput -Force
    $grepBridgedResults = Join-Path $scratch 'Grep-bridged-results'
    Invoke-Checked -Executable 'dotnet' -Arguments @('vstest',
        (Join-Path $grepBridgedOutput 'Icod.Grep.Tests.dll'),
        '--Logger:trx;LogFileName=bridged.trx', "--ResultsDirectory:$grepBridgedResults")
    Assert-PassingTests -TrxPath (Join-Path $grepBridgedResults 'bridged.trx') -ExpectedCount 113

    foreach ($suite in $lineEditorSuites) {
        $name = $suite.Name
        $publishedOutput = Join-Path $lineEditorRoot "tests/$name.Tests/bin/Staging/net10.0"
        $substitutedOutput = Join-Path $scratch "LineEditor.$name.Tests"
        New-Item -ItemType Directory -Path $substitutedOutput | Out-Null
        Copy-Item (Join-Path $publishedOutput '*') -Destination $substitutedOutput -Recurse -Force
        Copy-Item $bridgeDll -Destination (Join-Path $substitutedOutput 'Icod.CommandFramework.dll') -Force
        Copy-Item (Join-Path $probeOutput 'Icod.Expressions.dll') -Destination $substitutedOutput -Force
        $results = Join-Path $scratch "$name-bridged-results"
        Invoke-Checked -Executable 'dotnet' -Arguments @('vstest',
            (Join-Path $substitutedOutput "Icod.LineEditor.$name.Tests.dll"),
            '--Logger:trx;LogFileName=bridged.trx', "--ResultsDirectory:$results")
        Assert-PassingTests -TrxPath (Join-Path $results 'bridged.trx') -ExpectedCount $suite.ExpectedCases
    }
    Write-Host 'All 611 original CoreUtils, ProcPs, DiffUtils, Grep, and LineEditor binaries passed with published and package-backed bridge assemblies.'
} finally {
    if ($null -eq $previousPackages) {
        Remove-Item Env:NUGET_PACKAGES -ErrorAction SilentlyContinue
    } else {
        $env:NUGET_PACKAGES = $previousPackages
    }
    Remove-Item $scratch -Recurse -Force -ErrorAction SilentlyContinue
}
exit 0
