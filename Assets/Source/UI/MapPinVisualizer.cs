using System.Collections.Generic;
using SourceTCG.Core;
using SourceTCG.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace SourceTCG.UI
{
    /// <summary>Visual Ki/ReSource pin markers on the world map HUD.</summary>
    public class MapPinVisualizer : MonoBehaviour
    {
        [SerializeField] RectTransform pinPanel;
        readonly List<GameObject> pinInstances = new();

        public void BindPanel(RectTransform panel) => pinPanel = panel;

        public void Refresh(SpawnDto[] spawns, double playerLat, double playerLng)
        {
            if (pinPanel == null) return;
            ClearPins();
            if (spawns == null) return;

            foreach (var s in spawns)
            {
                var pin = CreatePinMarker(s, playerLat, playerLng);
                pinInstances.Add(pin);
            }
        }

        void ClearPins()
        {
            foreach (var go in pinInstances)
            {
                if (go != null) Destroy(go);
            }
            pinInstances.Clear();
        }

        GameObject CreatePinMarker(SpawnDto spawn, double playerLat, double playerLng)
        {
            var go = new GameObject($"Pin_{spawn.subtypeId}");
            go.transform.SetParent(pinPanel, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(72, 56);

            var icon = new GameObject("Icon");
            icon.transform.SetParent(go.transform, false);
            var iconRect = icon.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.65f);
            iconRect.anchorMax = new Vector2(0.5f, 0.65f);
            iconRect.sizeDelta = new Vector2(28, 28);
            var img = icon.AddComponent<Image>();
            var inRange = Haversine.IsWithinRadiusMeters(
                playerLat, playerLng, spawn.lat, spawn.lng, EconomyConfig.CollectionRadiusM);
            img.color = spawn.spawnType == "ki"
                ? (inRange ? new Color(0.2f, 0.7f, 1f) : new Color(0.15f, 0.35f, 0.55f))
                : (inRange ? new Color(0.3f, 0.9f, 0.4f) : new Color(0.2f, 0.45f, 0.25f));

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(1, 0.45f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var label = labelGo.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 11;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            var prefix = spawn.spawnType == "ki" ? "Ki" : "Re";
            label.text = inRange ? $"{prefix}\nIN RANGE" : $"{prefix}\n{spawn.distanceM:F0}m";

            return go;
        }
    }
}