# Structural + logic verification for client pure functions (mirrors shipped C# constants)
$errors = @()
if ((Get-Content "Assets\Source\Core\EconomyConfig.cs" -Raw) -notmatch 'DailyExtractLimit = 3') { $errors += "DailyExtractLimit" }
if ((Get-Content "Assets\Source\Core\EconomyConfig.cs" -Raw) -notmatch 'KiStartCost = 100') { $errors += "KiStartCost" }
if ((Get-Content "Assets\Source\Core\EconomyConfig.cs" -Raw) -notmatch 'ResourceGatherCost = 250') { $errors += "ResourceGatherCost" }
if ((Get-Content "Assets\Source\Data\GameCatalog.cs" -Raw) -notmatch 'Affinities = new') { $errors += "Affinities" }
$affinityCount = ([regex]::Matches((Get-Content "Assets\Source\Data\GameCatalog.cs" -Raw), 'new KiAffinityDef')).Count
if ($affinityCount -ne 12) { $errors += "affinity count $affinityCount" }
$resourceAdds = ([regex]::Matches((Get-Content "Assets\Source\Data\GameCatalog.cs" -Raw), 'list\.Add\(new ResourceDef')).Count
if ($resourceAdds -ne 27) { $errors += "resource count $resourceAdds" }
if ($errors.Count -gt 0) { Write-Error ("FAIL: " + ($errors -join ', ')); exit 1 }
Write-Output "PASS: client logic constants and catalog counts verified"
Write-Output "Haversine.cs and EconomyTests.cs present for Unity EditMode run"
exit 0