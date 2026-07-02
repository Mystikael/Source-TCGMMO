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
$text = $lines -join "`n"
if ($out) { $text | Out-File -FilePath $out -Encoding utf8 }
$text