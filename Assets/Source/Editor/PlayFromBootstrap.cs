#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SourceTCG.Editor
{
    /// <summary>Forces Play Mode to start from Bootstrap so SampleScene/AR template is never used by accident.</summary>
    [InitializeOnLoad]
    public static class PlayFromBootstrap
    {
        const string BootstrapPath = "Assets/Scenes/Bootstrap.unity";
        const string PrefKey = "SourceTCG.PlayFromBootstrap";

        static SceneAsset bootstrapScene;

        static PlayFromBootstrap()
        {
            bootstrapScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootstrapPath);
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            ApplyPlayStartScene();
        }

        static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
                ApplyPlayStartScene();
        }

        static void ApplyPlayStartScene()
        {
            if (!EditorPrefs.GetBool(PrefKey, true) || bootstrapScene == null)
            {
                EditorSceneManager.playModeStartScene = null;
                return;
            }

            EditorSceneManager.playModeStartScene = bootstrapScene;
        }

        [MenuItem("Source TCG/Open Bootstrap Scene")]
        public static void OpenBootstrapScene()
        {
            if (!System.IO.File.Exists(BootstrapPath))
            {
                Debug.LogError($"Bootstrap scene missing at {BootstrapPath}. Run Source TCG → Setup Alpha Scenes.");
                return;
            }

            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene(BootstrapPath, OpenSceneMode.Single);
            Debug.Log("Opened Bootstrap — press Play to see the login screen.");
        }

        [MenuItem("Source TCG/Play From Bootstrap", false, 1)]
        static void TogglePlayFromBootstrap()
        {
            var enabled = !EditorPrefs.GetBool(PrefKey, true);
            EditorPrefs.SetBool(PrefKey, enabled);
            ApplyPlayStartScene();
            Debug.Log(enabled
                ? "Play will always start from Bootstrap."
                : "Play will use whichever scene is currently open.");
        }

        [MenuItem("Source TCG/Play From Bootstrap", true)]
        static bool TogglePlayFromBootstrapValidate()
        {
            Menu.SetChecked("Source TCG/Play From Bootstrap", EditorPrefs.GetBool(PrefKey, true));
            return true;
        }
    }
}
#endif