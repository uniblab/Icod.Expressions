$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$scratch = Join-Path $root ('artifacts/final-parity-' + [guid]::NewGuid().ToString('N'))
$originalPackages = $env:NUGET_PACKAGES
New-Item -ItemType Directory -Path $scratch | Out-Null
$env:NUGET_PACKAGES = Join-Path $scratch 'packages'

function Invoke-Checked {
    param([string]$Executable, [string[]]$Arguments)
    & $Executable @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$Executable exited $LASTEXITCODE`: $($Arguments -join ' ')"
    }
}

function Read-PassingTests {
    param([string]$TrxPath)
    [xml]$trx = Get-Content $TrxPath -Raw
    $counters = $trx.SelectSingleNode("//*[local-name()='Counters']")
    if ($null -eq $counters -or [int]$counters.GetAttribute('total') -ne 214 -or
        [int]$counters.GetAttribute('passed') -ne 214 -or
        [int]$counters.GetAttribute('failed') -ne 0) {
        throw "Focused test count or result changed: expected 214 passes in $TrxPath"
    }
    $outcomes = @($trx.SelectNodes("//*[local-name()='UnitTestResult']") |
        ForEach-Object { $_.GetAttribute('testName') + "`t" + $_.GetAttribute('outcome') } |
        Sort-Object -CaseSensitive)
    if ($outcomes.Count -ne 214) { throw "Missing test identities in $TrxPath" }
    return $outcomes
}

try {
    $oldRoot = Join-Path $scratch 'Icod.CommandFramework'
    New-Item -ItemType Directory -Path $oldRoot | Out-Null
    $revision = '532850103e69e5d3d09367c64f929ddecaf1fe69'
    Invoke-Checked 'git' @('-C', $oldRoot, 'init', '-q')
    Invoke-Checked 'git' @('-C', $oldRoot, 'fetch', '-q', '--depth', '1',
        'https://github.com/uniblab/Icod.CommandFramework.git', $revision)
    Invoke-Checked 'git' @('-C', $oldRoot, 'checkout', '-q', '--detach', 'FETCH_HEAD')
    $actualRevision = (& git -C $oldRoot rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0 -or $actualRevision -ne $revision) {
        throw "In-situ audit commit changed: $actualRevision"
    }
    $regexTree = (& git -C $oldRoot rev-parse HEAD:src/RegularExpressions).Trim()
    if ($LASTEXITCODE -ne 0 -or $regexTree -ne 'bfe11d5f5ff6a75d75de68573edea6125706b87e') {
        throw "In-situ production regex tree changed: $regexTree"
    }

    $sourceManifest = Join-Path $root 'doc/Icod.Expressions-A05-Source-Blobs.txt'
    $sourceCount = 0
    foreach ($line in (Get-Content $sourceManifest)) {
        $parts = $line -split ' ', 2
        if ($parts.Count -ne 2) { throw "Malformed source manifest line: $line" }
        $oldPath = $parts[1] -replace '^src/Icod\.Expressions/', 'src/'
        $oldHash = (& git -C $oldRoot rev-parse "HEAD:$oldPath").Trim()
        $newHash = (& git -C $root rev-parse "HEAD:$($parts[1])").Trim()
        if ($LASTEXITCODE -ne 0 -or $oldHash -ne $parts[0] -or $newHash -ne $parts[0]) {
            throw "A05 production source mismatch: $oldPath"
        }
        $sourceCount++
    }
    if ($sourceCount -ne 35) { throw "Expected 35 A05 production blobs; found $sourceCount" }

    $newTests = Get-ChildItem (Join-Path $root 'tests/Icod.Expressions.Tests/RegularExpressions') -Filter '*.cs'
    if (@($newTests).Count -ne 13) { throw 'Expected 13 focused C# test files' }
    foreach ($test in $newTests) {
        $oldPath = "tests/CommandFramework.Tests/src/RegularExpressions/$($test.Name)"
        $newPath = "tests/Icod.Expressions.Tests/RegularExpressions/$($test.Name)"
        $oldHash = (& git -C $oldRoot rev-parse "HEAD:$oldPath").Trim()
        $newHash = (& git -C $root rev-parse "HEAD:$newPath").Trim()
        if ($LASTEXITCODE -ne 0 -or $oldHash -ne $newHash) {
            throw "Focused test source mismatch: $($test.Name)"
        }
    }

    $filter = (($newTests | ForEach-Object { 'FullyQualifiedName~' + $_.BaseName }) -join '|')
    $oldProject = Join-Path $oldRoot 'tests/CommandFramework.Tests/Icod.CommandFramework.Tests.csproj'
    $newProject = Join-Path $root 'tests/Icod.Expressions.Tests/Icod.Expressions.Tests.csproj'
    $oldResults = Join-Path $scratch 'old-results'
    $newResults = Join-Path $scratch 'new-results'
    Invoke-Checked 'dotnet' @('restore', $oldProject, '-m:1')
    Invoke-Checked 'dotnet' @('test', $oldProject, '-c', 'Staging', '--no-restore', '-m:1',
        '--filter', $filter, '--logger', 'trx;LogFileName=old.trx', '--results-directory', $oldResults)
    Invoke-Checked 'dotnet' @('test', $newProject, '-c', 'Staging', '-f', 'net10.0',
        '--no-build', '--no-restore', '--logger', 'trx;LogFileName=new.trx',
        '--results-directory', $newResults)
    $oldOutcomes = @(Read-PassingTests (Join-Path $oldResults 'old.trx'))
    $newOutcomes = @(Read-PassingTests (Join-Path $newResults 'new.trx'))
    $differences = @(Compare-Object $oldOutcomes $newOutcomes -CaseSensitive)
    if ($differences.Count -ne 0) {
        throw "Final focused test identities/outcomes differ: $($differences[0])"
    }

    $baseline = Join-Path $root 'doc/Icod.Expressions-A04-Parity-Baseline.jsonl'
    if (@([System.IO.File]::ReadAllLines($baseline)).Count -ne 44) {
        throw 'The frozen JSONL does not contain 44 cases'
    }
    $oldTool = Join-Path $oldRoot 'tools/RegularExpressionParityBaseline/RegularExpressionParityBaseline.csproj'
    $newTool = Join-Path $root 'tools/RegularExpressionParityBaseline/RegularExpressionParityBaseline.csproj'
    Invoke-Checked 'dotnet' @('restore', $oldTool, '-m:1')
    Invoke-Checked 'dotnet' @('run', '--project', $oldTool, '-c', 'Staging', '--no-restore',
        '--', '--verify', $baseline)
    Invoke-Checked 'dotnet' @('run', '--project', $newTool, '-c', 'Staging', '-f', 'net10.0',
        '--no-build', '--no-restore', '--', '--verify', $baseline)
    Write-Host 'Final A05 parity: 35 production blobs, 13 focused test blobs, 214 identical passing test identities, and identical 44-case JSONL verified.'
} finally {
    if ($null -eq $originalPackages) {
        Remove-Item Env:NUGET_PACKAGES -ErrorAction SilentlyContinue
    } else {
        $env:NUGET_PACKAGES = $originalPackages
    }
    Remove-Item $scratch -Recurse -Force -ErrorAction SilentlyContinue
}
exit 0
