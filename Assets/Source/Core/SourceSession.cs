using SourceTCG.Debugging;
using SourceTCG.Networking;
using UnityEngine;

namespace SourceTCG.Core
{
    public class SourceSession : MonoBehaviour
    {
        public static SourceSession Instance { get; private set; }

        public SourceApiClient Api { get; private set; }
        public GpsSimulator Gps { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void EnsureSession()
        {
            if (Instance != null) return;
            var go = new GameObject("SourceSession");
            DontDestroyOnLoad(go);
            go.AddComponent<SourceSession>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Api = gameObject.AddComponent<SourceApiClient>();
            Gps = gameObject.AddComponent<GpsSimulator>();
        }
    }
}