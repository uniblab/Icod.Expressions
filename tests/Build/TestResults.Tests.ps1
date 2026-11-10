$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$validator = Join-Path $root 'packaging/VerifyTestResults.ps1'
$temporary = Join-Path ([IO.Path]::GetTempPath()) ([guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $temporary | Out-Null
$contract = Join-Path $temporary 'contract.json'
@{
    expectedFrameworks = @('net8.0', 'net9.0', 'net10.0')
    minimumTests = 3
    subsets = @(@{name='frozen'; classPrefix='Legacy.Tests.'; expectedTests=2; methods=@{'Legacy.Tests.Suite.Frozen'=2}})
} | ConvertTo-Json -Depth 8 | Set-Content $contract
$script:checks = 0
function Assert-True($Value, $Message) { if (-not $Value) { throw $Message } }
function Write-Reports {
    Get-ChildItem $temporary -Filter '*.trx' | Remove-Item
    foreach ($framework in @('net8.0','net9.0','net10.0')) {
        $id = [guid]::NewGuid().ToString()
        @"
<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010" id="$id">
<Times start="$([DateTimeOffset]::UtcNow.ToString('o'))" finish="$([DateTimeOffset]::UtcNow.ToString('o'))" />
<TestDefinitions>
<UnitTest id="1" name="Frozen(1)"><TestMethod className="Legacy.Tests.Suite" name="Frozen" /></UnitTest>
<UnitTest id="2" name="Frozen(2)"><TestMethod className="Legacy.Tests.Suite" name="Frozen" /></UnitTest>
<UnitTest id="3" name="Current"><TestMethod className="Current.Tests.Suite" name="Current" /></UnitTest>
</TestDefinitions>
<Results>
<UnitTestResult testId="1" executionId="a" outcome="Passed" />
<UnitTestResult testId="2" executionId="b" outcome="Passed" />
<UnitTestResult testId="3" executionId="c" outcome="Passed" />
</Results>
<ResultSummary outcome="Completed"><Counters total="3" executed="3" passed="3" failed="0" error="0" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0" inProgress="0" pending="0" /></ResultSummary>
</TestRun>
"@ | Set-Content (Join-Path $temporary "full_${framework}_fixture.trx")
    }
}
function Change-Report([scriptblock]$Mutation) {
    $path=Join-Path $temporary 'full_net8.0_fixture.trx'
    [xml]$xml=Get-Content $path -Raw
    & $Mutation $xml
    $xml.Save($path)
}
function Reject([string]$Name, [scriptblock]$Mutation) {
    Write-Reports
    & $Mutation
    $rejected=$false
    try { & $validator -ResultsDirectory $temporary -ContractPath $contract | Out-Null } catch { $rejected=$true }
    Assert-True $rejected "Accepted invalid reports: $Name"
    $script:checks++
    Write-Host "PASS rejection: $Name"
}
try {
    if (-not (Test-Path $validator)) { throw 'Result validator not implemented.' }
    Write-Reports
    $summaries=@(& $validator -ResultsDirectory $temporary -ContractPath $contract)
    Assert-True ($summaries.Count -eq 3) 'Missing framework summaries.'
    foreach ($summary in $summaries) {
        Assert-True ($summary.passed -eq 3 -and $summary.subsets[0].passed -eq 2) 'Incorrect full/subset result.'
    }
    $script:checks++
    Reject 'missing framework' { Remove-Item (Join-Path $temporary 'full_net9.0_fixture.trx') }
    Reject 'duplicate framework' { Copy-Item (Join-Path $temporary 'full_net8.0_fixture.trx') (Join-Path $temporary 'full_net8.0_duplicate.trx') }
    Reject 'unexpected framework' { Copy-Item (Join-Path $temporary 'full_net8.0_fixture.trx') (Join-Path $temporary 'full_net11.0_fixture.trx') }
    Reject 'empty result file' { Set-Content (Join-Path $temporary 'full_net8.0_fixture.trx') '' }
    Reject 'malformed XML' { Set-Content (Join-Path $temporary 'full_net8.0_fixture.trx') '<TestRun>' }
    Reject 'reported failure' { Change-Report { param($x) $x.TestRun.ResultSummary.Counters.SetAttribute('failed','1') } }
    Reject 'reported skip' { Change-Report { param($x) $x.TestRun.ResultSummary.Counters.SetAttribute('notExecuted','1') } }
    Reject 'under minimum' { Change-Report { param($x) $x.TestRun.ResultSummary.Counters.SetAttribute('total','2') } }
    Reject 'incomplete result count' { Change-Report { param($x) $r=$x.TestRun.Results; $null=$r.RemoveChild($r.LastChild) } }
    Reject 'duplicate result ID' { Change-Report { param($x) $x.TestRun.Results.FirstChild.SetAttribute('testId','2') } }
    Reject 'duplicate execution ID' { Change-Report { param($x) $x.TestRun.Results.FirstChild.SetAttribute('executionId','b') } }
    Reject 'missing definition' { Change-Report { param($x) $d=$x.TestRun.TestDefinitions; $null=$d.RemoveChild($d.LastChild) } }
    Reject 'duplicate definition ID' { Change-Report { param($x) $x.TestRun.TestDefinitions.FirstChild.SetAttribute('id','2') } }
    Reject 'unpassed result with passing counters' { Change-Report { param($x) $x.TestRun.Results.FirstChild.SetAttribute('outcome','NotExecuted') } }
    Reject 'aborted run with passing counters' { Change-Report { param($x) $x.TestRun.ResultSummary.SetAttribute('outcome','Aborted') } }
    Reject 'wrong frozen identity at same count' { Change-Report { param($x) $x.TestRun.TestDefinitions.FirstChild.FirstChild.SetAttribute('name','Replacement') } }
    Reject 'wrong namespace at same count' { Change-Report { param($x) $x.TestRun.TestDefinitions.FirstChild.FirstChild.SetAttribute('className','Other.Tests.Suite') } }
    Reject 'duplicate frozen case under new ID' { Change-Report { param($x) $x.TestRun.TestDefinitions.FirstChild.SetAttribute('name','Frozen(2)') } }
    Reject 'duplicate run identity' {
        [xml]$first=Get-Content (Join-Path $temporary 'full_net8.0_fixture.trx') -Raw
        $path=Join-Path $temporary 'full_net9.0_fixture.trx'
        [xml]$second=Get-Content $path -Raw
        $second.TestRun.SetAttribute('id',$first.TestRun.GetAttribute('id'))
        $second.Save($path)
    }
    Write-Reports
    Remove-Item (Join-Path $temporary 'full_net9.0_fixture.trx'),(Join-Path $temporary 'full_net10.0_fixture.trx')
    $shard=@(& $validator -ResultsDirectory $temporary -ContractPath $contract -Framework net8.0)
    Assert-True ($shard.Count -eq 1 -and $shard[0].framework -eq 'net8.0' -and $shard[0].subsets[0].passed -eq 2) 'Lost selected-framework full/frozen checks.'
    $rejected=$false
    try { & $validator -ResultsDirectory $temporary -ContractPath $contract -Framework net11.0 | Out-Null } catch { $rejected=$true }
    Assert-True $rejected 'Accepted a framework outside the contract.'
    $rejected=$false
    try { & $validator -ResultsDirectory $temporary -ContractPath $contract | Out-Null } catch { $rejected=$true }
    Assert-True $rejected 'Accepted a shard as a complete combined run.'
    $script:checks+=3
    Write-Reports
    Change-Report { param($x) $x.TestRun.Times.SetAttribute('start','2000-01-01T00:00:00Z') }
    $rejected=$false
    try { & $validator -ResultsDirectory $temporary -ContractPath $contract -NotBeforeUtc ([DateTimeOffset]::UtcNow.AddMinutes(-1)) | Out-Null } catch { $rejected=$true }
    Assert-True $rejected 'Accepted stale report.'
    $script:checks++
    $runner=Join-Path $root 'packaging/Invoke-Tests.ps1'
    $global:TestCommands=[Collections.Generic.List[object]]::new()
    $global:TestCommandExit=0
    function global:dotnet {
        $global:TestCommands.Add(@($args))
        $global:LASTEXITCODE=$global:TestCommandExit
    }
    try {
        & $runner -Path 'solution.sln'
        $command=$global:TestCommands[0]
        Assert-True ($command -contains 'Debug' -and $command -contains '-p:TestTfmsInParallel=true') 'Missing Debug or framework concurrency.'
        Assert-True ($command -contains '--no-build' -and $command -contains '--no-restore' -and $command -notcontains '-f') 'Runner rebuilt or serialized frameworks.'
        & $runner -Path 'solution.sln' -Configuration Release
        Assert-True ($global:TestCommands[1] -contains 'Release') 'Lost Release configuration.'
        Assert-True ($command -notcontains '--framework' -and $command -notcontains '--results-directory') 'Changed default solution test selection.'
        $resultsPath=Join-Path $temporary 'shard results'
        & $runner -Path 'project.csproj' -Framework net9.0 -ResultsDirectory $resultsPath
        $shardCommand=$global:TestCommands[2]
        $frameworkIndex=[Array]::IndexOf($shardCommand,'--framework')
        $resultsIndex=[Array]::IndexOf($shardCommand,'--results-directory')
        Assert-True ($frameworkIndex -ge 0 -and $shardCommand[$frameworkIndex+1] -eq 'net9.0') 'Lost selected framework.'
        Assert-True ($resultsIndex -ge 0 -and $shardCommand[$resultsIndex+1] -eq $resultsPath) 'Lost results directory or split its spaces.'
        Assert-True (Test-Path -LiteralPath $resultsPath -PathType Container) 'Missing fresh result directory.'
        $before=$global:TestCommands.Count
        $rejected=$false
        try { & $runner -Path 'project.csproj' -Framework net9.0 -ResultsDirectory $resultsPath } catch { $rejected=$true }
        Assert-True ($rejected -and $global:TestCommands.Count -eq $before) 'Reused an existing result directory.'
        $global:TestCommandExit=17
        $rejected=$false
        try { & $runner -Path 'solution.sln' } catch { $rejected=$true }
        Assert-True $rejected 'Suppressed native command failure.'
        $script:checks+=7
    } finally {
        Remove-Item Function:\dotnet
        Remove-Variable TestCommands,TestCommandExit -Scope Global
        $global:LASTEXITCODE=0
    }
    Write-Host "PASS: $script:checks result and runner checks"
} finally { Remove-Item $temporary -Recurse -Force }
