param(
    [Parameter(Mandatory)][string]$Path,
    [ValidateSet('Debug','Staging','Release')][string]$Configuration = 'Debug',
    [string]$ContractPath,
    [ValidatePattern('^[A-Za-z0-9][A-Za-z0-9._-]*$')][string]$Framework,
    [string]$ResultsDirectory
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$arguments = @('test', $Path, '-c', $Configuration, '--no-build', '--no-restore', '-p:TestTfmsInParallel=true')
$started = [DateTimeOffset]::UtcNow
if ($Framework) { $arguments += @('--framework',$Framework) }
$directory = $null
if ($ResultsDirectory) {
    if (Test-Path -LiteralPath $ResultsDirectory) { throw 'The result directory must be fresh.' }
    $directory = [IO.Path]::GetFullPath($ResultsDirectory)
    New-Item -ItemType Directory -Path $directory | Out-Null
}
if ($ContractPath) {
    if ([IO.Path]::GetExtension($Path) -ne '.csproj') { throw 'A result contract requires one test project.' }
    if (-not (Test-Path -LiteralPath $ContractPath -PathType Leaf)) { throw 'Missing test result contract.' }
    if (-not $directory) {
        $directory = Join-Path ([IO.Path]::GetTempPath()) ('icod-test-results-' + [guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $directory | Out-Null
    }
    $arguments += @('--logger','trx;LogFilePrefix=full','--results-directory',$directory)
} else {
    # Preserve separate default project result directories for multi-project solutions.
    $arguments += @('--logger','trx')
    if ($directory) { $arguments += @('--results-directory',$directory) }
}
& dotnet @arguments
if ($LASTEXITCODE -ne 0) { throw "Test command failed with exit code $LASTEXITCODE." }
if ($ContractPath) {
    $verification = @{ResultsDirectory=$directory; ContractPath=$ContractPath; NotBeforeUtc=$started}
    if ($Framework) { $verification.Framework = $Framework }
    & (Join-Path $PSScriptRoot 'VerifyTestResults.ps1') @verification
}
