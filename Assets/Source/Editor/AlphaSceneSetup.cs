#if UNITY_EDITOR
using System.Linq;
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
        public static void SetupAll()
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
            EditorBuildSettings.scenes = scenes.Select(s => new EditorBuildSettingsScene(s, true)).ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("Alpha scenes created and added to build settings.");
        }

        static void CreateBootstrapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var bootstrap = new GameObject("Bootstrap");
            bootstrap.AddComponent<GameBootstrap>();
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Bootstrap.unity");
        }

        static void CreateWorldMapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EnsureEventSystem();
            var canvas = CreateCanvas();
            var hud = CreateText(canvas.transform, "HUD", new Vector2(0, 0), new Vector2(1, 0.55f));
            var log = CreateText(canvas.transform, "Log", new Vector2(0, 0.55f), new Vector2(1, 1));
            log.fontSize = 14;
            var extract = CreateButton(canvas.transform, "Extract", new Vector2(0.05f, 0.02f));
            var gather = CreateButton(canvas.transform, "Gather ReSource (250)", new Vector2(0.35f, 0.02f));
            var ki = CreateButton(canvas.transform, "Start Ki (100)", new Vector2(0.7f, 0.02f));
            var inv = CreateButton(canvas.transform, "Inventory", new Vector2(0.05f, 0.1f));
            var map = new GameObject("WorldMapController");
            var ctrl = map.AddComponent<WorldMapController>();
            var so = new SerializedObject(ctrl);
            so.FindProperty("hudText").objectReferenceValue = hud;
            so.FindProperty("logText").objectReferenceValue = log;
            so.FindProperty("extractButton").objectReferenceValue = extract;
            so.FindProperty("inventoryButton").objectReferenceValue = inv;
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
            if (Object.FindFirstObjectByType<EventSystem>() == null)
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