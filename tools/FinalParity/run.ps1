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

function Assert-SourceManifest {
    param([string]$ManifestPath, [string]$OriginalRoot, [string]$ExtractedRoot)
    $sourceCount = 0
    foreach ($line in (Get-Content $ManifestPath)) {
        $parts = $line -split ' ', 2
        if ($parts.Count -ne 2) { throw "Malformed source manifest line: $line" }
        $oldPath = $parts[1] -replace '^src/Icod\.Expressions/', 'src/'
        $oldHash = (& git -C $OriginalRoot rev-parse "HEAD:$oldPath").Trim()
        $newHash = (& git -C $ExtractedRoot rev-parse "HEAD:$($parts[1])").Trim()
        if ($LASTEXITCODE -ne 0 -or $oldHash -ne $parts[0] -or $newHash -ne $parts[0]) {
            throw "A05 production source mismatch: $oldPath"
        }
        $sourceCount++
    }
    if ($sourceCount -ne 35) { throw "Expected 35 A05 production blobs; found $sourceCount" }
}

function Assert-RejectsManifest {
    param([string]$ManifestPath, [string]$ExpectedError,
        [string]$OriginalRoot, [string]$ExtractedRoot)
    $rejected = $false
    try {
        Assert-SourceManifest $ManifestPath $OriginalRoot $ExtractedRoot
    } catch {
        if ($_.Exception.Message -ne $ExpectedError) { throw }
        $rejected = $true
    }
    if (-not $rejected) { throw "Mutated source manifest was accepted: $ManifestPath" }
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
    $manifestLines = @(Get-Content $sourceManifest)
    if ($manifestLines.Count -ne 35) { throw 'The frozen source manifest no longer contains 35 entries' }
    $firstEntry = $manifestLines[0] -split ' ', 2
    if ($firstEntry.Count -ne 2) { throw 'The frozen source manifest starts with a malformed entry' }
    $changedDigit = if ($firstEntry[0][0] -eq '0') { '1' } else { '0' }
    $badHashLines = [string[]]$manifestLines.Clone()
    $badHashLines[0] = $changedDigit + $firstEntry[0].Substring(1) + ' ' + $firstEntry[1]
    $badHashManifest = Join-Path $scratch 'bad-hash-manifest.txt'
    [System.IO.File]::WriteAllLines($badHashManifest, $badHashLines)
    $firstOldPath = $firstEntry[1] -replace '^src/Icod\.Expressions/', 'src/'
    Assert-RejectsManifest $badHashManifest "A05 production source mismatch: $firstOldPath" $oldRoot $root

    $badCountManifest = Join-Path $scratch 'bad-count-manifest.txt'
    [System.IO.File]::WriteAllLines($badCountManifest, [string[]]$manifestLines[0..33])
    Assert-RejectsManifest $badCountManifest 'Expected 35 A05 production blobs; found 34' $oldRoot $root
    Assert-SourceManifest $sourceManifest $oldRoot $root
    Write-Host 'Source manifest guards rejected scratch hash and count mutations.'

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
