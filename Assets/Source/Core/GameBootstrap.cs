using System.Collections;
using SourceTCG.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SourceTCG.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] SourceApiClient apiClient;
        [SerializeField] string worldMapScene = "WorldMap";

        IEnumerator Start()
        {
            if (apiClient == null && SourceSession.Instance != null)
                apiClient = SourceSession.Instance.Api;
            if (apiClient == null)
                apiClient = FindFirstObjectByType<SourceApiClient>();

            var ok = false;
            yield return apiClient.GuestAuth(b => ok = b);
            if (!ok)
            {
                Debug.LogError("Guest auth failed");
                yield break;
            }
            yield return apiClient.RefreshWallet();
            SceneManager.LoadScene(worldMapScene);
        }
    }
}