using NUnit.Framework;
using SourceTCG.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SourceTCG.Tests.EditMode
{
    public class SceneWiringEditModeTests
    {
        const string WorldMapPath = "Assets/Scenes/WorldMap.unity";

        [Test]
        public void WorldMapScene_LoadsWithSerializedVisualComponents()
        {
            var scene = EditorSceneManager.OpenScene(WorldMapPath, OpenSceneMode.Additive);
            try
            {
                var ctrl = Object.FindFirstObjectByType<WorldMapController>();
                var pins = Object.FindFirstObjectByType<MapPinVisualizer>();
                var progress = Object.FindFirstObjectByType<KiProgressBar>();
                Assert.IsNotNull(ctrl, "WorldMapController missing");
                Assert.IsNotNull(pins, "MapPinVisualizer missing");
                Assert.IsNotNull(progress, "KiProgressBar missing");

                var so = new SerializedObject(ctrl);
                Assert.AreEqual(pins, so.FindProperty("pinVisualizer").objectReferenceValue);
                Assert.AreEqual(progress, so.FindProperty("kiProgressBar").objectReferenceValue);
                Assert.IsNotNull(so.FindProperty("hudText").objectReferenceValue);
                Assert.IsNotNull(so.FindProperty("extractButton").objectReferenceValue);
                Assert.IsNotNull(Object.FindFirstObjectByType<EventSystem>());
                Assert.IsNotNull(GameObject.Find("Canvas"));
                Assert.IsNotNull(GameObject.Find("PinPanel"));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void AlphaScenes_MetaGuidMatchesBuildSettings()
        {
            var paths = new[]
            {
                "Assets/Scenes/Bootstrap.unity",
                "Assets/Scenes/WorldMap.unity",
                "Assets/Scenes/Inventory.unity",
            };
            foreach (var path in paths)
            {
                var guid = AssetDatabase.AssetPathToGUID(path);
                Assert.AreEqual(32, guid.Length, $"GUID length for {path}");
                Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(guid, "^[a-f0-9]{32}$"),
                    $"GUID format for {path}: {guid}");

                var build = System.Array.Find(EditorBuildSettings.scenes, s => s.path == path);
                Assert.IsTrue(build.enabled, $"Build entry disabled: {path}");
                Assert.AreEqual(guid, build.guid.ToString(), $"Build GUID mismatch for {path}");
            }
        }
    }
}