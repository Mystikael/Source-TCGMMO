# Verify scene script guids, essentials, and serialized visual components.
$root = if ($PSScriptRoot) { Split-Path $PSScriptRoot -Parent } else { Get-Location }
Set-Location $root
$errors = @()

function Get-MetaGuid($csRel) {
    $meta = Join-Path $root ($csRel + '.meta')
    if (-not (Test-Path $meta)) { return $null }
    if ((Get-Content $meta -Raw) -match 'guid: ([a-f0-9]{32})') { return $Matches[1] }
    return $null
}

$manifestPath = Join-Path $PSScriptRoot 'guid-manifest.json'
if (Test-Path $manifestPath) {
    $manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
    foreach ($prop in $manifest.PSObject.Properties) {
        $guid = $prop.Value
        if ($guid -match '^[a-f0-9]{8}[a-f0-9]{4}[a-f0-9]{4}[a-f0-9]{4}[a-f0-9]{12}$' -and $guid -match '(0123|abcd|dead|beef|ffff)') {
            $errors += "synthetic-looking guid in manifest: $($prop.Name)"
        }
    }
}

$requiredScripts = @(
    'Assets\Source\Core\HexResolver.cs',
    'Assets\Source\UI\WorldMapHudFormatter.cs',
    'Assets\Source\UI\MapPinVisualizer.cs',
    'Assets\Source\UI\KiProgressBar.cs',
    'Assets\Source\Tests\EditMode\HexResolverEditModeTests.cs',
    'Assets\Source\Tests\EditMode\SceneWiringEditModeTests.cs'
)
foreach ($script in $requiredScripts) {
    if (-not (Test-Path (Join-Path $root ($script + '.meta'))) -and $script -notmatch 'EditMode') {
        $errors += "missing meta: $script"
    }
}

$sceneBindings = @{
    'Assets\Scenes\Bootstrap.unity'  = 'Assets\Source\Core\GameBootstrap.cs'
    'Assets\Scenes\WorldMap.unity'   = 'Assets\Source\UI\WorldMapController.cs'
    'Assets\Scenes\Inventory.unity'  = 'Assets\Source\UI\InventoryController.cs'
}
foreach ($scene in $sceneBindings.Keys) {
    $script = $sceneBindings[$scene]
    $expected = Get-MetaGuid ($script -replace '/', '\')
    $sceneText = Get-Content $scene -Raw
    if ($null -eq $expected) { $errors += "missing meta for $script"; continue }
    if ($sceneText -notmatch "guid: $expected") {
        $errors += "$scene does not reference guid $expected from $script"
    }
    foreach ($essential in @('Main Camera', 'Directional Light', 'EventSystem')) {
        if ($sceneText -notmatch "m_Name: $essential") { $errors += "$scene missing $essential" }
    }
}

$worldMap = Get-Content 'Assets\Scenes\WorldMap.unity' -Raw
if ($worldMap -notmatch 'MapPinVisualizer') { $errors += 'WorldMap missing serialized MapPinVisualizer' }
if ($worldMap -notmatch 'KiProgressBar') { $errors += 'WorldMap missing serialized KiProgressBar' }
if ($worldMap -notmatch 'pinPanel:') { $errors += 'WorldMap missing serialized pinPanel ref' }
if ($worldMap -notmatch 'm_Name: PinPanel') { $errors += 'WorldMap missing PinPanel GameObject' }
if ($worldMap -notmatch 'm_Name: Canvas') { $errors += 'WorldMap missing Canvas' }

if ($errors.Count -gt 0) {
    Write-Output "FAIL: $($errors -join '; ')"
    exit 1
}
Write-Output 'PASS: scene guids, essentials, serialized MapPinVisualizer/KiProgressBar/PinPanel/Canvas'
exit 0