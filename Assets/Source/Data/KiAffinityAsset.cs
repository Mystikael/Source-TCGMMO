using UnityEngine;

namespace SourceTCG.Data
{
    [CreateAssetMenu(fileName = "KiAffinity", menuName = "Source TCG/Ki Affinity")]
    public class KiAffinityAsset : ScriptableObject
    {
        public string affinityId;
        public string displayName;
        public Sprite icon;
    }
}