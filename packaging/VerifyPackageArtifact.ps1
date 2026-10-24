param(
    [Parameter(Mandatory = $true)]
    [string]$ArtifactDirectory,

    [ValidateSet('Debug', 'Staging', 'Release')]
    [string]$Configuration = 'Release',

    [string]$ExpectedVersion = '',

    [string]$ExpectedId = '',

    [int]$ExpectedPackageCount = -1,

    [switch]$AllowNoPackages,

    [string]$GitHubOutputPath = ''
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
Import-Module (Join-Path $PSScriptRoot 'RepositoryTools.psm1') -Force

if (-not [System.IO.Path]::IsPathRooted($ArtifactDirectory)) {
    $ArtifactDirectory = Join-Path $repositoryRoot $ArtifactDirectory
}
$ArtifactDirectory = [System.IO.Path]::GetFullPath($ArtifactDirectory)
if (-not (Test-Path -LiteralPath $ArtifactDirectory -PathType Container)) {
    throw "Artifact directory '$ArtifactDirectory' does not exist."
}

$packages = @(
    Get-ChildItem -LiteralPath $ArtifactDirectory -Filter '*.nupkg' -File |
        Where-Object { -not $_.Name.EndsWith('.symbols.nupkg', [System.StringComparison]::OrdinalIgnoreCase) } |
        Sort-Object Name
)

if (0 -eq $packages.Count -and -not $AllowNoPackages) {
    $suffix = if ([string]::IsNullOrWhiteSpace($ExpectedVersion)) { '' } else { " with version '$ExpectedVersion'" }
    throw "No NuGet packages$suffix were found in '$ArtifactDirectory'."
}
if (0 -le $ExpectedPackageCount -and $packages.Count -ne $ExpectedPackageCount) {
    throw "Expected $ExpectedPackageCount NuGet package(s) in '$ArtifactDirectory'; found $($packages.Count)."
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
foreach ($package in $packages) {
    $metadata = Get-PackageMetadata -PackagePath $package.FullName
    if (-not [string]::IsNullOrWhiteSpace($ExpectedVersion) -and $metadata.Version -cne $ExpectedVersion) {
        throw "Package '$($package.Name)' has version '$($metadata.Version)', expected '$ExpectedVersion'."
    }
    if (-not [string]::IsNullOrWhiteSpace($ExpectedId)) {
        if ($metadata.Id -cne $ExpectedId -or
            $package.Name -cne "$ExpectedId.$ExpectedVersion.nupkg") {
            throw "Package '$($package.Name)' does not match $ExpectedId $ExpectedVersion."
        }
    }
    Write-Host "Verifying $($metadata.Id) $($metadata.Version): $($package.FullName)"

    $archive = [System.IO.Compression.ZipFile]::OpenRead($package.FullName)
    try {
        if (0 -eq $archive.Entries.Count) {
            throw "Package '$($package.FullName)' is empty."
        }

        if (-not [string]::IsNullOrWhiteSpace($metadata.Readme)) {
            $readmeEntry = $archive.Entries |
                Where-Object { $_.FullName -eq $metadata.Readme } |
                Select-Object -First 1
            if ($null -eq $readmeEntry) {
                throw "Package '$($package.FullName)' declares missing readme '$($metadata.Readme)'."
            }
        }

        if (-not [string]::IsNullOrWhiteSpace($ExpectedId)) {
            $actualDlls = @(
                $archive.Entries |
                    Where-Object { $_.FullName -like 'lib/*.dll' } |
                    ForEach-Object { $_.FullName } |
                    Sort-Object
            )
            $expectedDlls = @(
                "lib/net8.0/$ExpectedId.dll",
                "lib/net9.0/$ExpectedId.dll",
                "lib/net10.0/$ExpectedId.dll"
            ) | Sort-Object
            if (($actualDlls -join '|') -cne ($expectedDlls -join '|')) {
                throw "Package '$($package.Name)' does not contain only the expected net8.0/net9.0/net10.0 library DLLs."
            }
        }

        $toolSettings = @(
            $archive.Entries |
                Where-Object { $_.FullName.EndsWith('/DotnetToolSettings.xml', [System.StringComparison]::OrdinalIgnoreCase) }
        )
        if (1 -lt $toolSettings.Count) {
            throw "Package '$($package.FullName)' contains multiple DotnetToolSettings.xml files."
        }
    } finally {
        $archive.Dispose()
    }
}

if (-not [string]::IsNullOrWhiteSpace($GitHubOutputPath)) {
    "package_count=$($packages.Count)" >> $GitHubOutputPath
    "has_packages=$((0 -lt $packages.Count).ToString().ToLowerInvariant())" >> $GitHubOutputPath
}

Write-Host "Exact package verification completed successfully for $($packages.Count) package(s) ($Configuration)."
