# Writes alpha scenes with Main Camera, Directional Light, EventSystem, and scene GUIDs.
$root = Split-Path $PSScriptRoot -Parent
Set-Location $root

$sceneGuids = @{
    'Bootstrap' = '7a4e2f9c8b1d4630e5a69738291c4d01'
    'WorldMap'  = '8b5f3a0d9c2e5741f6b7a8493025e12'
    'Inventory' = '9c6a4b1e0d3f6852a7c8b9504136f23'
}

$scriptGuids = @{
    GameBootstrap       = 'f4a8c2e1b3d5476980a1b2c3d4e5f601'
    WorldMapController  = 'a7b3c9d2e4f5487190a2b3c4d5e6f702'
    InventoryController = 'c8d4e0f3a5b6498201b3c4d5e6f70803'
}

function Write-SceneMeta($name, $guid) {
    $path = Join-Path $root "Assets\Scenes\$name.unity.meta"
    @"
fileFormatVersion: 2
guid: $guid
DefaultImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"@ | Set-Content $path -Encoding utf8
}

function Get-SceneHeader($sceneGuid) {
    @"
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!29 &1
OcclusionCullingSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_OcclusionBakeSettings:
    smallestOccluder: 5
    smallestHole: 0.25
    backfaceThreshold: 100
  m_SceneGUID: $sceneGuid
  m_OcclusionCullingData: {fileID: 0}
--- !u!104 &2
RenderSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 10
  m_Fog: 0
  m_AmbientSkyColor: {r: 0.212, g: 0.227, b: 0.259, a: 1}
  m_AmbientIntensity: 1
  m_AmbientMode: 0
  m_SkyboxMaterial: {fileID: 10304, guid: 0000000000000000f000000000000000, type: 0}
--- !u!157 &3
LightmapSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 13
  m_GISettings:
    serializedVersion: 2
    m_BounceScale: 1
    m_IndirectOutputScale: 1
    m_AlbedoBoost: 1
    m_EnvironmentLightingMode: 0
    m_EnableBakedLightmaps: 1
    m_EnableRealtimeLightmaps: 0
  m_LightmapEditorSettings:
    serializedVersion: 12
    m_Resolution: 2
    m_BakeResolution: 40
    m_AtlasSize: 1024
  m_LightingDataAsset: {fileID: 0}
  m_LightingSettings: {fileID: 0}
--- !u!196 &4
NavMeshSettings:
  serializedVersion: 2
  m_ObjectHideFlags: 0
  m_NavMeshData: {fileID: 0}
"@
}

function Get-DefaultEnvironmentYaml() {
    @"
--- !u!1 &900100
GameObject:
  m_ObjectHideFlags: 0
  serializedVersion: 6
  m_Component:
  - component: {fileID: 900101}
  - component: {fileID: 900102}
  - component: {fileID: 900103}
  m_Layer: 0
  m_Name: Main Camera
  m_TagString: MainCamera
  m_IsActive: 1
--- !u!4 &900101
Transform:
  m_GameObject: {fileID: 900100}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 1, z: -10}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_Father: {fileID: 0}
--- !u!20 &900102
Camera:
  m_GameObject: {fileID: 900100}
  m_Enabled: 1
  serializedVersion: 2
  m_ClearFlags: 1
  m_BackGroundColor: {r: 0.19215687, g: 0.3019608, b: 0.4745098, a: 0}
  m_projectionMatrixMode: 1
  m_FOVAxisMode: 0
  m_SensorSize: {x: 36, y: 24}
  m_LensShift: {x: 0, y: 0}
  m_FocalLength: 50
  m_NormalizedViewPortRect:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1
    height: 1
  near clip plane: 0.3
  far clip plane: 1000
  field of view: 60
  orthographic: 0
  m_Depth: -1
--- !u!81 &900103
AudioListener:
  m_GameObject: {fileID: 900100}
  m_Enabled: 1
--- !u!1 &900200
GameObject:
  m_ObjectHideFlags: 0
  serializedVersion: 6
  m_Component:
  - component: {fileID: 900201}
  - component: {fileID: 900202}
  m_Layer: 0
  m_Name: Directional Light
  m_IsActive: 1
--- !u!4 &900201
Transform:
  m_GameObject: {fileID: 900200}
  serializedVersion: 2
  m_LocalRotation: {x: 0.40821788, y: -0.23456968, z: 0.10938163, w: 0.8754261}
  m_LocalPosition: {x: 0, y: 3, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_Father: {fileID: 0}
--- !u!108 &900202
Light:
  m_GameObject: {fileID: 900200}
  m_Enabled: 1
  serializedVersion: 10
  m_Type: 1
  m_Color: {r: 1, g: 0.95686275, b: 0.8392157, a: 1}
  m_Intensity: 1
--- !u!1 &900300
GameObject:
  m_ObjectHideFlags: 0
  serializedVersion: 6
  m_Component:
  - component: {fileID: 900301}
  - component: {fileID: 900302}
  - component: {fileID: 900303}
  m_Layer: 0
  m_Name: EventSystem
  m_IsActive: 1
--- !u!4 &900301
Transform:
  m_GameObject: {fileID: 900300}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_Father: {fileID: 0}
--- !u!114 &900302
MonoBehaviour:
  m_GameObject: {fileID: 900300}
  m_Enabled: 1
  m_Script: {fileID: 11500000, guid: 76c392e42b5098c458856cdf6ecaaaa1, type: 3}
--- !u!114 &900303
MonoBehaviour:
  m_GameObject: {fileID: 900300}
  m_Enabled: 1
  m_Script: {fileID: 11500000, guid: 4f231c4fb786f3946a6b90b886c48677, type: 3}
"@
}

$bootstrap = Get-SceneHeader $sceneGuids.Bootstrap
$bootstrap += Get-DefaultEnvironmentYaml
$bootstrap += @"

--- !u!1 &100000
GameObject:
  m_ObjectHideFlags: 0
  serializedVersion: 6
  m_Component:
  - component: {fileID: 100001}
  - component: {fileID: 100002}
  m_Layer: 0
  m_Name: Bootstrap
  m_IsActive: 1
--- !u!4 &100001
Transform:
  m_GameObject: {fileID: 100000}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_Father: {fileID: 0}
--- !u!114 &100002
MonoBehaviour:
  m_GameObject: {fileID: 100000}
  m_Enabled: 1
  m_Script: {fileID: 11500000, guid: $($scriptGuids.GameBootstrap), type: 3}
  worldMapScene: WorldMap
"@
Set-Content "Assets\Scenes\Bootstrap.unity" $bootstrap.TrimEnd() -Encoding utf8

$worldMap = Get-SceneHeader $sceneGuids.WorldMap
$worldMap += Get-DefaultEnvironmentYaml
$worldMap += @"

--- !u!1 &200000
GameObject:
  m_ObjectHideFlags: 0
  serializedVersion: 6
  m_Component:
  - component: {fileID: 200001}
  - component: {fileID: 200002}
  m_Layer: 0
  m_Name: WorldMapController
  m_IsActive: 1
--- !u!4 &200001
Transform:
  m_GameObject: {fileID: 200000}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_Father: {fileID: 0}
--- !u!114 &200002
MonoBehaviour:
  m_GameObject: {fileID: 200000}
  m_Enabled: 1
  m_Script: {fileID: 11500000, guid: $($scriptGuids.WorldMapController), type: 3}
  refreshInterval: 3
"@
Set-Content "Assets\Scenes\WorldMap.unity" $worldMap.TrimEnd() -Encoding utf8

$inventory = Get-SceneHeader $sceneGuids.Inventory
$inventory += Get-DefaultEnvironmentYaml
$inventory += @"

--- !u!1 &300000
GameObject:
  m_ObjectHideFlags: 0
  serializedVersion: 6
  m_Component:
  - component: {fileID: 300001}
  - component: {fileID: 300002}
  m_Layer: 0
  m_Name: InventoryController
  m_IsActive: 1
--- !u!4 &300001
Transform:
  m_GameObject: {fileID: 300000}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_Father: {fileID: 0}
--- !u!114 &300002
MonoBehaviour:
  m_GameObject: {fileID: 300000}
  m_Enabled: 1
  m_Script: {fileID: 11500000, guid: $($scriptGuids.InventoryController), type: 3}
"@
Set-Content "Assets\Scenes\Inventory.unity" $inventory.TrimEnd() -Encoding utf8

foreach ($name in $sceneGuids.Keys) {
    Write-SceneMeta $name $sceneGuids[$name]
}

$build = Get-Content "ProjectSettings\EditorBuildSettings.asset" -Raw
$build = $build -replace 'guid: b1c2d3e4f5a60718293a4b5c6d7e8f01', "guid: $($sceneGuids.Bootstrap)"
$build = $build -replace 'guid: c2d3e4f5a6b70718293a4b5c6d7e8f02', "guid: $($sceneGuids.WorldMap)"
$build = $build -replace 'guid: d3e4f5a6b7c80718293a4b5c6d7e8f03', "guid: $($sceneGuids.Inventory)"
Set-Content "ProjectSettings\EditorBuildSettings.asset" $build.TrimEnd() -Encoding utf8

Write-Output "Baked Bootstrap, WorldMap, Inventory with Camera/Light/EventSystem and scene GUIDs"