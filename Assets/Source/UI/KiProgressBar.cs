using SourceTCG.Data;
using UnityEngine;
using UnityEngine.UI;

namespace SourceTCG.UI
{
    /// <summary>Visual Ki dwell progress bar for active extraction sessions.</summary>
    public class KiProgressBar : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] Image fillImage;
        [SerializeField] Text labelText;

        public void Bind(GameObject barRoot, Image fill, Text label)
        {
            root = barRoot;
            fillImage = fill;
            labelText = label;
        }

        public void Refresh(int elapsedSeconds, int requiredSeconds, string state, string affinityId)
        {
            if (root == null) return;
            var active = state != "completed" && requiredSeconds > 0;
            root.SetActive(active);
            if (!active) return;

            var pct = Mathf.Clamp01(requiredSeconds > 0 ? (float)elapsedSeconds / requiredSeconds : 0f);
            if (fillImage != null) fillImage.fillAmount = pct;
            if (labelText != null)
            {
                var name = GameCatalog.GetAffinityName(affinityId);
                labelText.text = $"Ki: {name} {elapsedSeconds}/{requiredSeconds}s ({Mathf.RoundToInt(pct * 100)}%) — {state}";
            }
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }
    }
}