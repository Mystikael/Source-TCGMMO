# Gating structural audit: catalog ScriptableObjects, ki PNG refs, build scenes, GUID sync.
$root = Split-Path -Parent $PSScriptRoot
$out = $args[0]
$errors = @()
$lines = @()

$lines += "=== Assets/Source structure ==="
Get-ChildItem -Path "$root\Assets\Source" -Recurse -File | ForEach-Object { $lines += $_.FullName.Replace($root + '\', '') }

$lines += ""
$lines += "=== Ki affinities PNGs (expect 12) ==="
$pngs = Get-ChildItem "$root\Assets\ki_affinities\*.png"
$pngs.Name | ForEach-Object { $lines += $_ }
$lines += "Count: $($pngs.Count)"
if ($pngs.Count -ne 12) { $errors += "ki_affinities PNG count $($pngs.Count) != 12" }

function Get-PngGuid($pngName) {
    $meta = Join-Path $root "Assets\ki_affinities\$pngName.meta"
    if (-not (Test-Path $meta)) { return $null }
    if ((Get-Content $meta -Raw) -match 'guid: ([a-f0-9]{32})') { return $Matches[1] }
    return $null
}

$lines += ""
$lines += "=== Ki affinity ScriptableObjects (expect 12) ==="
$affDir = Join-Path $root "Assets\Source\Data\Catalog\Affinities"
$affAssets = @()
if (Test-Path $affDir) { $affAssets = Get-ChildItem "$affDir\*.asset" }
$lines += "Count: $($affAssets.Count)"
if ($affAssets.Count -ne 12) { $errors += "KiAffinityAsset count $($affAssets.Count) != 12" }

$expectedAffinities = @{
    'Fyr' = 'Fyr.png'; 'Watyr' = 'Watyr.png'; 'Matyr' = 'Matyr.png'; 'Ayr' = 'Ayr.png'
    'Aeth' = 'Aeth.png'; 'Lux' = 'Lux.png'; 'Vyb' = 'Vyb.png'; 'Grav' = 'Grav.png'
    'Psionic' = 'Psionic.png'; 'Omega' = 'Omega.png'; 'Veritas' = 'Veritas.png'; 'Astral' = 'Astral.png'
}
foreach ($name in $expectedAffinities.Keys) {
    $assetPath = Join-Path $affDir "$name.asset"
    if (-not (Test-Path $assetPath)) {
        $errors += "missing affinity asset $name.asset"
        continue
    }
    $pngGuid = Get-PngGuid $expectedAffinities[$name]
    $assetText = Get-Content $assetPath -Raw
    if ($null -eq $pngGuid) {
        $errors += "missing PNG meta for $($expectedAffinities[$name])"
    } elseif ($assetText -notmatch "guid: $pngGuid") {
        $errors += "$name.asset does not reference $($expectedAffinities[$name]) guid $pngGuid"
    } else {
        $lines += "$name OK -> $($expectedAffinities[$name])"
    }
}

$lines += ""
$lines += "=== Resource ScriptableObjects (expect 27) ==="
$resDir = Join-Path $root "Assets\Source\Data\Catalog\Resources"
$resAssets = @()
if (Test-Path $resDir) { $resAssets = Get-ChildItem "$resDir\*.asset" }
$lines += "Count: $($resAssets.Count)"
if ($resAssets.Count -ne 27) { $errors += "ResourceAsset count $($resAssets.Count) != 27" }

$lines += ""
$lines += "=== GameCatalog runtime constants ==="
$lines += "Affinities: $(if (Test-Path "$root\Assets\Source\Data\GameCatalog.cs") { 'GameCatalog.AffinityCount=12' } else { 'MISSING' })"
$lines += "Resources: $(if (Test-Path "$root\Assets\Source\Data\GameCatalog.cs") { 'GameCatalog.ResourceCount=27' } else { 'MISSING' })"

$lines += ""
$lines += "=== Build scenes ==="
Select-String -Path "$root\ProjectSettings\EditorBuildSettings.asset" -Pattern "path: Assets/Scenes" | ForEach-Object { $lines += $_.Line.Trim() }

$lines += ""
$lines += "=== Scene meta/build GUID sync (32 hex) ==="
$sceneMetas = @('Bootstrap','WorldMap','Inventory')
$buildText = Get-Content "$root\ProjectSettings\EditorBuildSettings.asset" -Raw
foreach ($sceneName in $sceneMetas) {
    $metaPath = "$root\Assets\Scenes\$sceneName.unity.meta"
    if (-not (Test-Path $metaPath)) { $errors += "$sceneName MISSING meta"; continue }
    $metaText = Get-Content $metaPath -Raw
    if ($metaText -notmatch 'guid: ([a-f0-9]{32})') { $errors += "$sceneName INVALID meta guid"; continue }
    $metaGuid = $Matches[1]
    if ($buildText -notmatch "path: Assets/Scenes/$sceneName\.unity[\s\S]*?guid: $metaGuid") {
        $errors += "$sceneName build/meta GUID mismatch"
    } else {
        $lines += "$sceneName OK guid=$metaGuid"
    }
}

if ($errors.Count -gt 0) {
    $lines += ""
    $lines += "=== FAILURES ==="
    $errors | ForEach-Object { $lines += $_ }
}

$text = $lines -join "`n"
if ($out) { $text | Out-File -FilePath $out -Encoding utf8 }
$text

if ($errors.Count -gt 0) {
    Write-Output "FAIL: $($errors -join '; ')"
    exit 1
}
Write-Output 'PASS: structural audit (12 affinity SO + 27 resource SO + scenes)'
exit 0