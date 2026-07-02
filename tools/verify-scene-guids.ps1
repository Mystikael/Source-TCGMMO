# Verify scene script guids, scene essentials, and all Source .cs.meta files exist.
$root = if ($PSScriptRoot) { Split-Path $PSScriptRoot -Parent } else { Get-Location }
Set-Location $root
$errors = @()

function Get-MetaGuid($csRel) {
    $meta = Join-Path $root ($csRel + '.meta')
    if (-not (Test-Path $meta)) { return $null }
    if ((Get-Content $meta -Raw) -match 'guid: ([a-f0-9]{32})') { return $Matches[1] }
    return $null
}

$requiredScripts = @(
    'Assets\Source\Core\GameBootstrap.cs',
    'Assets\Source\Core\HexResolver.cs',
    'Assets\Source\UI\WorldMapController.cs',
    'Assets\Source\UI\WorldMapHudFormatter.cs',
    'Assets\Source\UI\MapPinVisualizer.cs',
    'Assets\Source\UI\KiProgressBar.cs',
    'Assets\Source\UI\InventoryController.cs',
    'Assets\Source\Tests\HexResolverTests.cs',
    'Assets\Source\Tests\HudFormatterTests.cs'
)
foreach ($script in $requiredScripts) {
    if (-not (Test-Path (Join-Path $root ($script + '.meta')))) {
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
        if ($sceneText -notmatch "m_Name: $essential") {
            $errors += "$scene missing $essential"
        }
    }
    if ($sceneText -match 'm_SceneGUID: 00000000000000000000000000000000') {
        $errors += "$scene has zeroed m_SceneGUID"
    }
}

if ($errors.Count -gt 0) {
    Write-Output "FAIL: $($errors -join '; ')"
    exit 1
}
Write-Output 'PASS: scene script guids, scene essentials (Camera/Light/EventSystem), Source .cs.meta files'
exit 0