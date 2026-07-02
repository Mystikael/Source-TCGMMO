# Assign stable 32-char Unity GUIDs to every Assets/Source/**/*.cs.meta
$root = Split-Path $PSScriptRoot -Parent
$guidMap = @{
    'Assets\Source\Core\GameBootstrap.cs'         = 'f4a8c2e1b3d5476980a1b2c3d4e5f601'
    'Assets\Source\Core\EconomyConfig.cs'         = '1a2b3c4d5e6f7890abcdef1234567890'
    'Assets\Source\Core\Haversine.cs'             = '2b3c4d5e6f789012abcdef1234567891'
    'Assets\Source\Core\HexResolver.cs'           = 'a1b2c3d4e5f6789012345678abcdef01'
    'Assets\Source\Core\SourceSession.cs'         = '3c4d5e6f7890123abcdef1234567892'
    'Assets\Source\Data\GameCatalog.cs'           = '4d5e6f78901234abcdef1234567893'
    'Assets\Source\Debug\GpsSimulator.cs'           = '5e6f789012345abcdef1234567894'
    'Assets\Source\Networking\SourceApiClient.cs'   = '6f7890123456abcdef1234567895'
    'Assets\Source\UI\WorldMapController.cs'        = 'a7b3c9d2e4f5487190a2b3c4d5e6f702'
    'Assets\Source\UI\WorldMapHudFormatter.cs'    = 'b2c3d4e5f6789012345678abcdef0123'
    'Assets\Source\UI\MapPinVisualizer.cs'        = 'c3d4e5f6789012345678abcdef01234'
    'Assets\Source\UI\KiProgressBar.cs'           = 'd4e5f6789012345678abcdef0123456'
    'Assets\Source\UI\InventoryController.cs'     = 'c8d4e0f3a5b6498201b3c4d5e6f70803'
    'Assets\Source\UI\RuntimeUiFactory.cs'          = '78901234567abcdef1234567896ab'
    'Assets\Source\UI\OnboardingCopy.cs'            = '89012345678abcdef1234567897ab'
    'Assets\Source\Editor\AlphaSceneSetup.cs'       = '90123456789abcdef1234567898ab'
    'Assets\Source\Tests\EconomyTests.cs'           = '0123456789abcdef123456789abcd'
    'Assets\Source\Tests\HexResolverTests.cs'       = '123456789abcdef0123456789abcde'
    'Assets\Source\Tests\HudFormatterTests.cs'     = '23456789abcdef0123456789abcdef'
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
    Set-Content -Path $metaPath -Value $content.TrimEnd() -Encoding utf8
    Add-Content -Path $metaPath -Value ""
}
# Plugin DLL metas
$pluginMeta = @'
fileFormatVersion: 2
guid: {0}
PluginImporter:
  externalObjects: {{}}
  serializedVersion: 2
  iconMap: {{}}
  executionOrder: {{}}
  defineConstraints: []
  isPreloaded: 0
  isOverridable: 0
  isExplicitlyReferenced: 1
  validateReferences: 1
  platformData:
  - first:
      Any: 
    second:
      enabled: 1
      settings: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
'@
Set-Content (Join-Path $root 'Assets\Plugins\H3\pocketken.H3.dll.meta') ($pluginMeta -f 'e5f6789012345678abcdef01234567') -Encoding utf8
Set-Content (Join-Path $root 'Assets\Plugins\H3\NetTopologySuite.dll.meta') ($pluginMeta -f 'f6789012345678abcdef012345678') -Encoding utf8
if (-not (Test-Path (Join-Path $root 'Assets\Plugins\H3.meta'))) {
    @'
fileFormatVersion: 2
guid: 6789012345678abcdef0123456789a
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
'@ | Set-Content (Join-Path $root 'Assets\Plugins\H3.meta') -Encoding utf8
}
Write-Output "Wrote $($guidMap.Count) script .meta files + H3 plugin metas"