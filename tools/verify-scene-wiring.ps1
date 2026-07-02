# Scenes ship minimal GO + controllers; UI/buttons wired at runtime in Awake (no editor menu required)
$root = if ($PSScriptRoot) { Split-Path $PSScriptRoot -Parent } else { Get-Location }
Set-Location $root
$errors = @()
$build = Get-Content "ProjectSettings\EditorBuildSettings.asset" -Raw
$world = Get-Content "Assets\Source\UI\WorldMapController.cs" -Raw
$bootstrap = Get-Content "Assets\Source\Core\GameBootstrap.cs" -Raw
$session = Get-Content "Assets\Source\Core\SourceSession.cs" -Raw

foreach ($scene in @('Bootstrap', 'WorldMap', 'Inventory')) {
    if ($build -notmatch "Assets/Scenes/$scene\.unity") { $errors += "build missing $scene" }
    if (-not (Test-Path "Assets\Scenes\$scene.unity")) { $errors += "file missing $scene" }
}
if ($world -notmatch 'if \(hudText == null\)\s+BuildRuntimeUi') { $errors += 'WorldMap auto UI' }
if ($bootstrap -notmatch 'GuestAuth') { $errors += 'Bootstrap auth flow' }
if ($session -notmatch 'RuntimeInitializeOnLoadMethod') { $errors += 'SourceSession auto init' }

if ($errors.Count -gt 0) {
    Write-Output "FAIL: $($errors -join ', ')"
    exit 1
}
Write-Output 'PASS: alpha scenes in build; runtime UI wiring in controllers (no editor setup required)'
exit 0