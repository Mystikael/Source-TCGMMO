using UnityEngine;

namespace SourceTCG.Data
{
    [CreateAssetMenu(fileName = "Resource", menuName = "Source TCG/Resource")]
    public class ResourceAsset : ScriptableObject
    {
        public string resourceId;
        public int tier;
        public string category;
        public string displayName;
    }
}