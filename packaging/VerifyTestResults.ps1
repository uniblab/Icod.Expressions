param(
    [Parameter(Mandatory)][string]$ResultsDirectory,
    [Parameter(Mandatory)][string]$ContractPath,
    [DateTimeOffset]$NotBeforeUtc = [DateTimeOffset]::MinValue,
    [string]$Framework
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$contract = Get-Content -LiteralPath $ContractPath -Raw | ConvertFrom-Json -AsHashtable
$frameworks = @($contract.expectedFrameworks)
if ($frameworks.Count -eq 0 -or @($frameworks | Select-Object -Unique).Count -ne $frameworks.Count) {
    throw 'The contract requires distinct expectedFrameworks.'
}
if ($Framework) {
    if ($frameworks -cnotcontains $Framework) { throw 'Selected framework is outside the result contract.' }
    $frameworks = @($Framework)
}
if ($contract.minimumTests -isnot [long] -and $contract.minimumTests -isnot [int]) { throw 'minimumTests must be an integer.' }
if ($contract.minimumTests -le 0) { throw 'minimumTests must be positive.' }
$subsets = @($contract.subsets)
$subsetNames = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($subset in $subsets) {
    if (-not $subsetNames.Add($subset.name) -or [string]::IsNullOrWhiteSpace($subset.name) -or
        [string]::IsNullOrWhiteSpace($subset.classPrefix) -or $subset.expectedTests -le 0 -or $subset.methods.Count -eq 0) {
        throw 'Each subset requires a unique name, classPrefix, expectedTests and methods.'
    }
    $sum = 0
    foreach ($key in $subset.methods.Keys) {
        $count = $subset.methods[$key]
        if (-not $key.StartsWith($subset.classPrefix, [StringComparison]::Ordinal) -or
            ($count -isnot [long] -and $count -isnot [int]) -or $count -le 0) { throw "Invalid subset method: $key" }
        $sum += $count
    }
    if ($sum -ne $subset.expectedTests) { throw 'Subset method counts do not equal expectedTests.' }
}
$files = @(Get-ChildItem -LiteralPath $ResultsDirectory -Filter '*.trx' -File)
if ($files.Count -ne $frameworks.Count) { throw 'Missing or extra framework result files.' }
$usedFiles = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$runIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($framework in $frameworks) {
    if ($framework -notmatch '^[A-Za-z0-9][A-Za-z0-9._-]*$') { throw 'Invalid framework name.' }
    $matching = @($files | Where-Object { $_.Name.StartsWith("full_${framework}_", [StringComparison]::Ordinal) })
    if ($matching.Count -ne 1 -or -not $usedFiles.Add($matching[0].FullName)) { throw "Missing or duplicate framework: $framework" }
    $settings = [Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [Xml.DtdProcessing]::Prohibit
    $settings.XmlResolver = $null
    $reader = [Xml.XmlReader]::Create($matching[0].FullName, $settings)
    try {
        $xml = [Xml.XmlDocument]::new()
        $xml.XmlResolver = $null
        $xml.Load($reader)
    } finally { $reader.Dispose() }
    $root = $xml.DocumentElement
    if ($root.LocalName -ne 'TestRun' -or -not $runIds.Add($root.GetAttribute('id')) -or
        [string]::IsNullOrWhiteSpace($root.GetAttribute('id'))) { throw 'Missing or duplicate TestRun identity.' }
    $times = @($root.SelectNodes('./*[local-name()="Times"]'))
    if ($times.Count -ne 1) { throw 'Missing run timestamps.' }
    $started = [DateTimeOffset]::Parse($times[0].GetAttribute('start'), [Globalization.CultureInfo]::InvariantCulture)
    $finished = [DateTimeOffset]::Parse($times[0].GetAttribute('finish'), [Globalization.CultureInfo]::InvariantCulture)
    if ($started -lt $NotBeforeUtc -or $finished -lt $started -or $finished -gt [DateTimeOffset]::UtcNow.AddMinutes(5)) { throw 'Stale or invalid run timestamps.' }
    $summaries = @($root.SelectNodes('./*[local-name()="ResultSummary"]'))
    if ($summaries.Count -ne 1 -or $summaries[0].GetAttribute('outcome') -notin @('Completed','Passed')) { throw 'Incomplete or failed TestRun.' }
    $counters = @($summaries[0].SelectNodes('./*[local-name()="Counters"]'))
    if ($counters.Count -ne 1) { throw 'Missing result counters.' }
    $counts = @{}
    foreach ($attribute in $counters[0].Attributes) {
        if ($attribute.Value -notmatch '^\d+$') { throw 'Invalid result counter.' }
        $counts[$attribute.Name] = [long]$attribute.Value
    }
    foreach ($required in @('total','executed','passed','failed')) {
        if (-not $counts.ContainsKey($required)) { throw "Missing counter: $required" }
    }
    $total = $counts.total
    if ($total -lt $contract.minimumTests -or $counts.executed -ne $total -or $counts.passed -ne $total) { throw 'Incomplete full test results.' }
    foreach ($key in $counts.Keys) {
        if ($key -notin @('total','executed','passed','completed') -and $counts[$key] -ne 0) { throw "Nonzero unsuccessful counter: $key" }
    }
    if ($counts.ContainsKey('completed') -and $counts.completed -notin @(0,$total)) { throw 'Invalid completed count.' }
    $definitions = [Collections.Generic.Dictionary[string,object]]::new([StringComparer]::Ordinal)
    foreach ($definition in $root.SelectNodes('./*[local-name()="TestDefinitions"]/*[local-name()="UnitTest"]')) {
        $id = $definition.GetAttribute('id')
        if ([string]::IsNullOrWhiteSpace($id) -or $definitions.ContainsKey($id)) { throw 'Missing or duplicate test definition identity.' }
        $method = @($definition.SelectNodes('./*[local-name()="TestMethod"]'))
        if ($method.Count -ne 1) { throw 'Missing test method identity.' }
        $class = $method[0].GetAttribute('className')
        $name = $method[0].GetAttribute('name')
        if ([string]::IsNullOrWhiteSpace($class) -or [string]::IsNullOrWhiteSpace($name)) { throw 'Empty test method identity.' }
        $definitions.Add($id, @{class=$class; method="$class.$name"; case=$definition.GetAttribute('name')})
    }
    $results = @($root.SelectNodes('./*[local-name()="Results"]/*[local-name()="UnitTestResult"]'))
    if ($results.Count -ne $total -or $definitions.Count -ne $total) { throw 'Counters, definitions and results disagree.' }
    $seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    $executions = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    $selected = @{}
    $cases = @{}
    foreach ($subset in $subsets) {
        $selected[$subset.name] = [Collections.Generic.Dictionary[string,int]]::new([StringComparer]::Ordinal)
        $cases[$subset.name] = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    }
    foreach ($result in $results) {
        $id = $result.GetAttribute('testId')
        $execution = $result.GetAttribute('executionId')
        if (-not $seen.Add($id) -or -not $definitions.ContainsKey($id) -or
            [string]::IsNullOrWhiteSpace($execution) -or -not $executions.Add($execution) -or
            $result.GetAttribute('outcome') -ne 'Passed') { throw 'Missing, duplicate or unpassed test execution.' }
        $definition = $definitions[$id]
        foreach ($subset in $subsets) {
            if (-not $definition.class.StartsWith($subset.classPrefix, [StringComparison]::Ordinal)) { continue }
            if (-not $subset.methods.ContainsKey($definition.method) -or
                [string]::IsNullOrWhiteSpace($definition.case) -or -not $cases[$subset.name].Add($definition.case)) { throw 'Unexpected or duplicate subset test case identity.' }
            $map = $selected[$subset.name]
            if (-not $map.ContainsKey($definition.method)) { $map.Add($definition.method,0) }
            $map[$definition.method]++
        }
    }
    $subsetResults = @()
    foreach ($subset in $subsets) {
        $map = $selected[$subset.name]
        foreach ($method in $subset.methods.Keys) {
            if (-not $map.ContainsKey($method) -or $map[$method] -ne $subset.methods[$method]) { throw "Missing or changed subset method: $method" }
        }
        if ($cases[$subset.name].Count -ne $subset.expectedTests) { throw 'Subset test count drift.' }
        $subsetResults += @{name=$subset.name; passed=$subset.expectedTests; methods=$map.Count}
    }
    $summary = [pscustomobject]@{kind='test-results'; framework=$framework; total=$total; passed=$total; failed=0; skipped=0; subsets=$subsetResults}
    Write-Host ($summary | ConvertTo-Json -Depth 6 -Compress)
    Write-Output $summary
}
