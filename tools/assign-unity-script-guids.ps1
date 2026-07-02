# Assign cryptographically random Unity GUIDs; persisted in guid-manifest.json for scene consistency.
$root = Split-Path $PSScriptRoot -Parent
$manifestPath = Join-Path $PSScriptRoot 'guid-manifest.json'

$scriptPaths = @(
    'Assets\Source\Core\GameBootstrap.cs',
    'Assets\Source\Core\EconomyConfig.cs',
    'Assets\Source\Core\Haversine.cs',
    'Assets\Source\Core\HexResolver.cs',
    'Assets\Source\Core\SourceSession.cs',
    'Assets\Source\Data\GameCatalog.cs',
    'Assets\Source\Debug\GpsSimulator.cs',
    'Assets\Source\Networking\SourceApiClient.cs',
    'Assets\Source\UI\WorldMapController.cs',
    'Assets\Source\UI\WorldMapHudFormatter.cs',
    'Assets\Source\UI\MapPinVisualizer.cs',
    'Assets\Source\UI\KiProgressBar.cs',
    'Assets\Source\UI\InventoryController.cs',
    'Assets\Source\UI\RuntimeUiFactory.cs',
    'Assets\Source\UI\OnboardingCopy.cs',
    'Assets\Source\Editor\AlphaSceneSetup.cs',
    'Assets\Source\Tests\EconomyTests.cs',
    'Assets\Source\Tests\HexResolverTests.cs',
    'Assets\Source\Tests\HudFormatterTests.cs'
)

$manifest = @{}
if (Test-Path $manifestPath) {
    $manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json -AsHashtable
}

foreach ($rel in $scriptPaths) {
    if (-not $manifest.ContainsKey($rel)) {
        $manifest[$rel] = [guid]::NewGuid().ToString('N')
    }
}

$manifest | ConvertTo-Json | Set-Content $manifestPath -Encoding utf8

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

foreach ($rel in $scriptPaths) {
    $metaPath = Join-Path $root ($rel + '.meta')
    $content = $template -f $manifest[$rel]
    Set-Content -Path $metaPath -Value $content.TrimEnd() -Encoding utf8
    Add-Content -Path $metaPath -Value ""
}

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

if (-not (Test-Path (Join-Path $root 'Assets\Plugins\H3.meta'))) {
    $folderGuid = [guid]::NewGuid().ToString('N')
    @"
fileFormatVersion: 2
guid: $folderGuid
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"@ | Set-Content (Join-Path $root 'Assets\Plugins\H3.meta') -Encoding utf8
}

$h3Guid = if ($manifest.ContainsKey('Assets\Plugins\H3\pocketken.H3.dll')) { $manifest['Assets\Plugins\H3\pocketken.H3.dll'] } else { [guid]::NewGuid().ToString('N') }
$ntsGuid = if ($manifest.ContainsKey('Assets\Plugins\H3\NetTopologySuite.dll')) { $manifest['Assets\Plugins\H3\NetTopologySuite.dll'] } else { [guid]::NewGuid().ToString('N') }
$manifest['Assets\Plugins\H3\pocketken.H3.dll'] = $h3Guid
$manifest['Assets\Plugins\H3\NetTopologySuite.dll'] = $ntsGuid
$manifest | ConvertTo-Json | Set-Content $manifestPath -Encoding utf8

Set-Content (Join-Path $root 'Assets\Plugins\H3\pocketken.H3.dll.meta') ($pluginMeta -f $h3Guid) -Encoding utf8
Set-Content (Join-Path $root 'Assets\Plugins\H3\NetTopologySuite.dll.meta') ($pluginMeta -f $ntsGuid) -Encoding utf8

Write-Output "Wrote $($scriptPaths.Count) script metas with random GUIDs -> $manifestPath"