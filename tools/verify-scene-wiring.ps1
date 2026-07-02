# Scenes ship Camera/Light/EventSystem + controllers; visual UI wired at runtime or via AlphaSceneSetup.
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
    $sceneText = Get-Content "Assets\Scenes\$scene.unity" -Raw
    if ($sceneText -notmatch 'm_Name: Main Camera') { $errors += "$scene missing camera" }
}
if ($world -notmatch 'BuildRuntimeUi') { $errors += 'WorldMap BuildRuntimeUi' }
if ($world -notmatch 'MapPinVisualizer') { $errors += 'WorldMap pin visualizer' }
if ($world -notmatch 'KiProgressBar') { $errors += 'WorldMap ki progress bar' }
if ($world -notmatch 'pinVisualizer\?\.Refresh') { $errors += 'WorldMap pin refresh' }
if ($bootstrap -notmatch 'GuestAuth') { $errors += 'Bootstrap auth flow' }
if ($session -notmatch 'RuntimeInitializeOnLoadMethod') { $errors += 'SourceSession auto init' }

if ($errors.Count -gt 0) {
    Write-Output "FAIL: $($errors -join ', ')"
    exit 1
}
Write-Output 'PASS: alpha scenes in build; visual pin/progress UI wired in WorldMapController'
exit 0