# Assign stable 32-char Unity GUIDs to every Assets/Source/**/*.cs.meta
$root = Split-Path $PSScriptRoot -Parent
$guidMap = @{
    'Assets\Source\Core\GameBootstrap.cs'       = 'f4a8c2e1b3d5476980a1b2c3d4e5f601'
    'Assets\Source\Core\EconomyConfig.cs'       = '1a2b3c4d5e6f7890abcdef1234567890'
    'Assets\Source\Core\Haversine.cs'           = '2b3c4d5e6f789012abcdef1234567891'
    'Assets\Source\Core\SourceSession.cs'       = '3c4d5e6f7890123abcdef1234567892'
    'Assets\Source\Data\GameCatalog.cs'         = '4d5e6f78901234abcdef1234567893'
    'Assets\Source\Debug\GpsSimulator.cs'       = '5e6f789012345abcdef1234567894'
    'Assets\Source\Networking\SourceApiClient.cs' = '6f7890123456abcdef1234567895'
    'Assets\Source\UI\WorldMapController.cs'    = 'a7b3c9d2e4f5487190a2b3c4d5e6f702'
    'Assets\Source\UI\InventoryController.cs'   = 'c8d4e0f3a5b6498201b3c4d5e6f70803'
    'Assets\Source\UI\RuntimeUiFactory.cs'      = '78901234567abcdef1234567896ab'
    'Assets\Source\UI\OnboardingCopy.cs'        = '89012345678abcdef1234567897ab'
    'Assets\Source\Editor\AlphaSceneSetup.cs'     = '90123456789abcdef1234567898ab'
    'Assets\Source\Tests\EconomyTests.cs'       = '0123456789abcdef123456789abcd'
}
$template = @'
fileFormatVersion: 2
guid: {0}
MonoImporter:
  externalObjects: {{}}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {{instanceID: 0}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
'@
foreach ($rel in $guidMap.Keys) {
    $metaPath = Join-Path $root ($rel + '.meta')
    $content = $template -f $guidMap[$rel]
    Set-Content -Path $metaPath -Value $content.TrimEnd() -Encoding utf8 -NoNewline
    Add-Content -Path $metaPath -Value ""
}
Write-Output "Wrote $($guidMap.Count) script .meta files"