# Verify scene m_Script guids match Assets/Source/**/*.cs.meta guid fields
$root = if ($PSScriptRoot) { Split-Path $PSScriptRoot -Parent } else { Get-Location }
Set-Location $root
$errors = @()

function Get-MetaGuid($csRel) {
    $meta = Join-Path $root ($csRel + '.meta')
    if (-not (Test-Path $meta)) { return $null }
    if ((Get-Content $meta -Raw) -match 'guid: ([a-f0-9]{32})') { return $Matches[1] }
    return $null
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
}

if ($errors.Count -gt 0) {
    Write-Output "FAIL: $($errors -join '; ')"
    exit 1
}
Write-Output 'PASS: Bootstrap/WorldMap/Inventory scene script guids match .cs.meta files'
exit 0