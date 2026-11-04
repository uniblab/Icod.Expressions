param(
    [Parameter(Mandatory)][string]$Path,
    [ValidateSet('Debug','Staging','Release')][string]$Configuration = 'Debug',
    [string]$ContractPath
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$arguments = @('test', $Path, '-c', $Configuration, '--no-build', '--no-restore', '-p:TestTfmsInParallel=true')
$started = [DateTimeOffset]::UtcNow
if ($ContractPath) {
    if ([IO.Path]::GetExtension($Path) -ne '.csproj') { throw 'A result contract requires one test project.' }
    if (-not (Test-Path -LiteralPath $ContractPath -PathType Leaf)) { throw 'Missing test result contract.' }
    $directory = Join-Path ([IO.Path]::GetTempPath()) ('icod-test-results-' + [guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $directory | Out-Null
    $arguments += @('--logger','trx;LogFilePrefix=full','--results-directory',$directory)
} else {
    # Preserve separate default project result directories for multi-project solutions.
    $arguments += @('--logger','trx')
}
& dotnet @arguments
if ($LASTEXITCODE -ne 0) { throw "Test command failed with exit code $LASTEXITCODE." }
if ($ContractPath) {
    & (Join-Path $PSScriptRoot 'VerifyTestResults.ps1') -ResultsDirectory $directory -ContractPath $ContractPath -NotBeforeUtc $started
}
