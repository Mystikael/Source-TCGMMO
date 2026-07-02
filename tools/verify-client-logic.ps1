# Verifies shipped client constants and catalog structure (no false-positive regex counts)
$root = if ($PSScriptRoot) { Split-Path $PSScriptRoot -Parent } else { Get-Location }
Set-Location $root
$errors = @()

$economy = Get-Content "Assets\Source\Core\EconomyConfig.cs" -Raw
$catalog = Get-Content "Assets\Source\Data\GameCatalog.cs" -Raw
$tests = Get-Content "Assets\Source\Tests\EconomyTests.cs" -Raw
$worldMap = Get-Content "Assets\Source\UI\WorldMapController.cs" -Raw

if ($economy -notmatch 'DailyExtractLimit = 3') { $errors += 'DailyExtractLimit' }
if ($economy -notmatch 'KiStartCost = 100') { $errors += 'KiStartCost' }
if ($economy -notmatch 'ResourceGatherCost = 250') { $errors += 'ResourceGatherCost' }
if ($economy -notmatch 'CollectionRadiusM = 40') { $errors += 'CollectionRadiusM' }

if ($catalog -notmatch 'AffinityCount = 12') { $errors += 'AffinityCount const' }
if ($catalog -notmatch 'ResourceCount = 27') { $errors += 'ResourceCount const' }
if ($catalog -notmatch 'for \(var tier = 0; tier <= 8; tier\+\+\)') { $errors += 'tier loop 0-8' }
$perTierAdds = ([regex]::Matches($catalog, 'list\.Add\(new ResourceDef')).Count
if ($perTierAdds -ne 3) { $errors += "per-tier resource adds $perTierAdds (expected 3 in loop)" }
$affinityDefs = ([regex]::Matches($catalog, 'new KiAffinityDef')).Count
if ($affinityDefs -ne 12) { $errors += "affinity defs $affinityDefs" }

if ($tests -notmatch 'AreEqual\(GameCatalog\.AffinityCount, GameCatalog\.Affinities\.Count\)') { $errors += 'EconomyTests affinity count' }
if ($tests -notmatch 'AreEqual\(GameCatalog\.ResourceCount, GameCatalog\.Resources\.Count\)') { $errors += 'EconomyTests resource count' }
if ($tests -notmatch 'GetAffinityName_ReturnsCorrectName') { $errors += 'EconomyTests affinity name' }

if ($worldMap -notmatch 'BuildRuntimeUi') { $errors += 'WorldMap runtime UI' }
if ($worldMap -notmatch 'gatherButton\?\.onClick\.AddListener\(OnGatherNearestResource\)') { $errors += 'gather wire' }
if ($worldMap -notmatch 'kiButton\?\.onClick\.AddListener\(OnStartKi\)') { $errors += 'ki wire' }
if ($worldMap -notmatch 'GetAffinityName') { $errors += 'affinity name lookup' }

if ($errors.Count -gt 0) {
    Write-Output "FAIL: $($errors -join ', ')"
    exit 1
}
# Run real NUnit tests against shipped .cs files (not regex-only)
$dotnetTest = dotnet test tests/ClientLogicTests/ClientLogic.Tests.csproj --no-restore 2>&1
if ($LASTEXITCODE -ne 0) {
    # first run may need restore
    $dotnetTest = dotnet test tests/ClientLogicTests/ClientLogic.Tests.csproj 2>&1
}
$dotnetTest | ForEach-Object { Write-Output $_ }
if ($LASTEXITCODE -ne 0) {
    Write-Output 'FAIL: dotnet NUnit client logic tests'
    exit 1
}

Write-Output 'PASS: client logic constants, catalog structure, runtime UI wiring, NUnit tests verified'
exit 0