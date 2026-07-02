#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SourceTCG.Core;
using SourceTCG.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SourceTCG.Editor
{
    public static class AlphaSceneSetup
    {
        [MenuItem("Source TCG/Setup Alpha Scenes")]
        public static void SetupAll() => BatchSetup();

        /// <summary>Callable from Unity -batchmode -executeMethod SourceTCG.Editor.AlphaSceneSetup.BatchSetup</summary>
        public static void BatchSetup()
        {
            CreateBootstrapScene();
            CreateWorldMapScene();
            CreateInventoryScene();
            var scenes = new[]
            {
                "Assets/Scenes/Bootstrap.unity",
                "Assets/Scenes/WorldMap.unity",
                "Assets/Scenes/Inventory.unity",
            };
            SyncBuildSettings(scenes);
            AssetDatabase.SaveAssets();
            Debug.Log("Alpha scenes created and added to build settings.");
        }

        [MenuItem("Source TCG/Verify Alpha Scenes")]
        public static void VerifyMenu() => VerifyAlphaScenes();

        /// <summary>Callable from Unity -batchmode -executeMethod SourceTCG.Editor.AlphaSceneSetup.VerifyAlphaScenes</summary>
        public static void VerifyAlphaScenes()
        {
            try
            {
                var scenePaths = new[]
                {
                    "Assets/Scenes/Bootstrap.unity",
                    "Assets/Scenes/WorldMap.unity",
                    "Assets/Scenes/Inventory.unity",
                };
                foreach (var path in scenePaths)
                    AssertSceneBuildWiring(path);

                AssertWorldMapRuntimeWiring();
                Debug.Log("VerifyAlphaScenes PASS");
            }
            catch (Exception ex)
            {
                Debug.LogError($"VerifyAlphaScenes FAIL: {ex.Message}");
                EditorApplication.Exit(1);
            }
        }

        static void SyncBuildSettings(string[] scenePaths)
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            EditorBuildSettings.scenes = scenePaths
                .Select(path => new EditorBuildSettingsScene(path, true))
                .ToArray();

            var buildSettingsPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../ProjectSettings/EditorBuildSettings.asset"));
            var existing = File.ReadAllText(buildSettingsPath);
            var configIndex = existing.IndexOf("  m_configObjects:", StringComparison.Ordinal);
            var configBlock = configIndex >= 0 ? existing.Substring(configIndex).TrimEnd() : "  m_configObjects: {}";

            var sb = new StringBuilder();
            sb.AppendLine("%YAML 1.1");
            sb.AppendLine("%TAG !u! tag:unity3d.com,2011:");
            sb.AppendLine("--- !u!1045 &1");
            sb.AppendLine("EditorBuildSettings:");
            sb.AppendLine("  m_ObjectHideFlags: 0");
            sb.AppendLine("  serializedVersion: 2");
            sb.AppendLine("  m_Scenes:");
            foreach (var path in scenePaths)
            {
                var guid = AssetDatabase.AssetPathToGUID(path);
                if (guid.Length != 32 || !Regex.IsMatch(guid, "^[a-f0-9]{32}$"))
                    throw new InvalidOperationException($"Cannot sync build settings; invalid GUID for {path}: '{guid}'");
                sb.AppendLine("  - enabled: 1");
                sb.AppendLine($"    path: {path}");
                sb.AppendLine($"    guid: {guid}");
            }
            sb.AppendLine(configBlock);
            File.WriteAllText(buildSettingsPath, sb.ToString(), new UTF8Encoding(false));
            AssetDatabase.Refresh();
        }

        static void AssertSceneBuildWiring(string path)
        {
            var guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid) || guid.Length != 32 || !Regex.IsMatch(guid, "^[a-f0-9]{32}$"))
                throw new InvalidOperationException($"Invalid AssetDatabase GUID for {path}: '{guid}'");

            var buildEntry = EditorBuildSettings.scenes.FirstOrDefault(s => s.path == path);
            if (buildEntry.path != path || !buildEntry.enabled)
                throw new InvalidOperationException($"Build settings missing or disabled scene: {path}");
            if (buildEntry.guid.ToString() != guid)
                throw new InvalidOperationException(
                    $"Build GUID mismatch for {path}: build={buildEntry.guid} meta={guid}");
        }

        static void AssertWorldMapRuntimeWiring()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/WorldMap.unity", OpenSceneMode.Single);
            try
            {
                var ctrl = UnityEngine.Object.FindFirstObjectByType<WorldMapController>();
                var pins = UnityEngine.Object.FindFirstObjectByType<MapPinVisualizer>();
                var progress = UnityEngine.Object.FindFirstObjectByType<KiProgressBar>();
                if (ctrl == null || pins == null || progress == null)
                    throw new InvalidOperationException("WorldMap missing WorldMapController, MapPinVisualizer, or KiProgressBar");

                var so = new SerializedObject(ctrl);
                if (so.FindProperty("pinVisualizer").objectReferenceValue != pins)
                    throw new InvalidOperationException("WorldMapController.pinVisualizer not wired to MapPinVisualizer");
                if (so.FindProperty("kiProgressBar").objectReferenceValue != progress)
                    throw new InvalidOperationException("WorldMapController.kiProgressBar not wired to KiProgressBar");
                if (so.FindProperty("hudText").objectReferenceValue == null)
                    throw new InvalidOperationException("WorldMapController.hudText is not assigned");
                if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
                    throw new InvalidOperationException("WorldMap missing EventSystem");
                if (GameObject.Find("Canvas") == null || GameObject.Find("PinPanel") == null)
                    throw new InvalidOperationException("WorldMap missing Canvas or PinPanel");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        static void CreateBootstrapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EnsureEventSystem();
            var bootstrap = new GameObject("Bootstrap");
            bootstrap.AddComponent<GameBootstrap>();
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Bootstrap.unity");
        }

        static void CreateWorldMapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EnsureEventSystem();
            var canvas = CreateCanvas();
            var hud = CreateText(canvas.transform, "HUD", new Vector2(0, 0.23f), new Vector2(1, 0.55f));
            var log = CreateText(canvas.transform, "Log", new Vector2(0, 0.55f), new Vector2(1, 1));
            log.fontSize = 14;
            var pinPanel = RuntimeUiFactory.CreatePinPanel(canvas.transform);
            RuntimeUiFactory.CreateKiProgressBar(canvas.transform, out var kiRoot, out var kiFill, out var kiLabel);
            var extract = CreateButton(canvas.transform, "Extract", new Vector2(0.05f, 0.02f));
            var gather = CreateButton(canvas.transform, "Gather ReSource (250)", new Vector2(0.35f, 0.02f));
            var ki = CreateButton(canvas.transform, "Start Ki (100)", new Vector2(0.7f, 0.02f));
            var inv = CreateButton(canvas.transform, "Inventory", new Vector2(0.05f, 0.1f));
            var map = new GameObject("WorldMapController");
            var ctrl = map.AddComponent<WorldMapController>();
            var pins = map.AddComponent<MapPinVisualizer>();
            var progress = map.AddComponent<KiProgressBar>();
            pins.BindPanel(pinPanel);
            progress.Bind(kiRoot, kiFill, kiLabel);
            var so = new SerializedObject(ctrl);
            so.FindProperty("hudText").objectReferenceValue = hud;
            so.FindProperty("logText").objectReferenceValue = log;
            so.FindProperty("extractButton").objectReferenceValue = extract;
            so.FindProperty("gatherButton").objectReferenceValue = gather;
            so.FindProperty("kiButton").objectReferenceValue = ki;
            so.FindProperty("inventoryButton").objectReferenceValue = inv;
            so.FindProperty("pinVisualizer").objectReferenceValue = pins;
            so.FindProperty("kiProgressBar").objectReferenceValue = progress;
            so.ApplyModifiedPropertiesWithoutUndo();
            gather.onClick.AddListener(ctrl.OnGatherNearestResource);
            ki.onClick.AddListener(ctrl.OnStartKi);
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/WorldMap.unity");
        }

        static void CreateInventoryScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EnsureEventSystem();
            var canvas = CreateCanvas();
            var text = CreateText(canvas.transform, "InventoryText", new Vector2(0, 0), new Vector2(1, 1));
            var back = CreateButton(canvas.transform, "Back", new Vector2(0.05f, 0.02f));
            var inv = new GameObject("InventoryController");
            var ctrl = inv.AddComponent<InventoryController>();
            var so = new SerializedObject(ctrl);
            so.FindProperty("inventoryText").objectReferenceValue = text;
            so.FindProperty("backButton").objectReferenceValue = back;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Inventory.unity");
        }

        static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
        }

        static GameObject CreateCanvas()
        {
            var go = new GameObject("Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        static Text CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperLeft;
            return text;
        }

        static Button CreateButton(Transform parent, string label, Vector2 anchor)
        {
            var go = new GameObject(label);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0, 0);
            rect.sizeDelta = new Vector2(200, 40);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.4f, 0.8f);
            var btn = go.AddComponent<Button>();
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var text = textGo.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            return btn;
        }
    }
}
#endif