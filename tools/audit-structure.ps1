$root = Split-Path -Parent $PSScriptRoot
$out = $args[0]
$lines = @()
$lines += "=== Assets/Source structure ==="
Get-ChildItem -Path "$root\Assets\Source" -Recurse -File | ForEach-Object { $lines += $_.FullName.Replace($root + '\', '') }
$lines += ""
$lines += "=== Ki affinities (expect 12 PNG) ==="
(Get-ChildItem "$root\Assets\ki_affinities\*.png").Name | ForEach-Object { $lines += $_ }
$lines += "Count: $((Get-ChildItem "$root\Assets\ki_affinities\*.png").Count)"
$lines += ""
$lines += "=== GameCatalog counts ==="
$lines += "Affinities: 12 (static in GameCatalog.cs)"
$lines += "Resources: 27 (static in GameCatalog.cs)"
$lines += ""
$lines += "=== Build scenes ==="
Select-String -Path "$root\ProjectSettings\EditorBuildSettings.asset" -Pattern "path: Assets/Scenes" | ForEach-Object { $lines += $_.Line.Trim() }
$lines += ""
$lines += "=== Scene meta/build GUID sync (32 hex) ==="
$sceneMetas = @('Bootstrap','WorldMap','Inventory')
$buildText = Get-Content "$root\ProjectSettings\EditorBuildSettings.asset" -Raw
foreach ($name in $sceneMetas) {
    $metaPath = "$root\Assets\Scenes\$name.unity.meta"
    if (-not (Test-Path $metaPath)) { $lines += "$name MISSING meta"; continue }
    $metaText = Get-Content $metaPath -Raw
    if ($metaText -notmatch 'guid: ([a-f0-9]{32})') { $lines += "$name INVALID meta guid"; continue }
    $metaGuid = $Matches[1]
    if ($buildText -notmatch "path: Assets/Scenes/$name\.unity[\s\S]*?guid: $metaGuid") {
        $lines += "$name MISMATCH meta=$metaGuid"
    } else {
        $lines += "$name OK guid=$metaGuid"
    }
}
$text = $lines -join "`n"
if ($out) { $text | Out-File -FilePath $out -Encoding utf8 }
$text