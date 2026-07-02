using System.Collections;
using System.Text;
using SourceTCG.Core;
using SourceTCG.Data;
using SourceTCG.Debugging;
using SourceTCG.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SourceTCG.UI
{
    public class WorldMapController : MonoBehaviour
    {
        [SerializeField] SourceApiClient api;
        [SerializeField] GpsSimulator gps;
        [SerializeField] Text hudText;
        [SerializeField] Text logText;
        [SerializeField] Button extractButton;
        [SerializeField] Button inventoryButton;
        [SerializeField] float refreshInterval = 3f;

        KiSessionResponse activeKi;
        readonly StringBuilder log = new();

        void Awake()
        {
            if (api == null && SourceSession.Instance != null) api = SourceSession.Instance.Api;
            if (gps == null && SourceSession.Instance != null) gps = SourceSession.Instance.Gps;
            if (api == null) api = FindFirstObjectByType<SourceApiClient>();
            if (gps == null) gps = FindFirstObjectByType<GpsSimulator>();
            extractButton?.onClick.AddListener(OnExtract);
            inventoryButton?.onClick.AddListener(() => SceneManager.LoadScene("Inventory"));
        }

        void Start() => StartCoroutine(MapLoop());

        IEnumerator MapLoop()
        {
            while (true)
            {
                var lat = gps.Latitude;
                var lng = gps.Longitude;
                yield return api.RefreshWallet();
                yield return api.RefreshHex(lat, lng);
                yield return api.RefreshNearby(lat, lng);
                RefreshHud(lat, lng);
                yield return new WaitForSeconds(refreshInterval);
            }
        }

        void RefreshHud(double lat, double lng)
        {
            var w = api.Wallet;
            var hex = api.CurrentHex;
            var sb = new StringBuilder();
            sb.AppendLine($"Source TCGMMO — World Map");
            sb.AppendLine($"GPS: {lat:F4}, {lng:F4}");
            sb.AppendLine($"Hex: {hex?.h3Index ?? "?"} ({hex?.zoneClass})");
            sb.AppendLine($"Source Points: {w?.sourcePoints ?? 0}");
            sb.AppendLine($"Extracts left: {w?.extractionsRemaining ?? 0}/{EconomyConfig.DailyExtractLimit}");
            if (api.Nearby?.spawns != null)
            {
                sb.AppendLine("Nearby:");
                foreach (var s in api.Nearby.spawns)
                {
                    var inRange = Haversine.IsWithinRadiusMeters(lat, lng, s.lat, s.lng, EconomyConfig.CollectionRadiusM);
                    sb.AppendLine($"  [{s.spawnType}] {s.subtypeId} {(inRange ? "IN RANGE" : $"{s.distanceM:F0}m")}");
                }
            }
            if (hudText != null) hudText.text = sb.ToString();
            if (extractButton != null)
                extractButton.interactable = hex != null && hex.canExtract;
        }

        void OnExtract()
        {
            StartCoroutine(DoExtract());
        }

        IEnumerator DoExtract()
        {
            ExtractResponse result = null;
            yield return api.Extract(gps.Latitude, gps.Longitude, r => result = r);
            if (result != null)
                AppendLog($"Extracted +{result.pointsAwarded} Source Points!");
            else
                AppendLog("Extract failed (daily limit?)");
            yield return api.RefreshWallet();
            RefreshHud(gps.Latitude, gps.Longitude);
        }

        public void OnGatherNearestResource()
        {
            StartCoroutine(DoGather());
        }

        IEnumerator DoGather()
        {
            if (api.Wallet != null && api.Wallet.sourcePoints < EconomyConfig.ResourceGatherCost)
            {
                AppendLog($"Need {EconomyConfig.ResourceGatherCost} Source Points to gather.");
                yield break;
            }
            SpawnDto target = null;
            foreach (var s in api.Nearby?.spawns ?? System.Array.Empty<SpawnDto>())
            {
                if (s.spawnType == "resource" && Haversine.IsWithinRadiusMeters(gps.Latitude, gps.Longitude, s.lat, s.lng, EconomyConfig.CollectionRadiusM))
                {
                    target = s;
                    break;
                }
            }
            if (target == null) { AppendLog("No ReSource in range (40m)."); yield break; }
            GatherResponse r = null;
            yield return api.Gather(target.id, gps.Latitude, gps.Longitude, x => r = x);
            if (r != null)
                AppendLog($"Gather {r.outcomeTier}! {r.quantity}x {r.subtypeId} ({r.multiplier}x)");
            else
                AppendLog("Gather failed (depleted or out of range).");
            yield return api.RefreshWallet();
            yield return api.RefreshNearby(gps.Latitude, gps.Longitude);
        }

        public void OnStartKi()
        {
            StartCoroutine(DoKi());
        }

        IEnumerator DoKi()
        {
            if (api.Wallet != null && api.Wallet.sourcePoints < EconomyConfig.KiStartCost)
            {
                AppendLog($"Need {EconomyConfig.KiStartCost} Source Points for Ki.");
                yield break;
            }
            SpawnDto target = null;
            foreach (var s in api.Nearby?.spawns ?? System.Array.Empty<SpawnDto>())
            {
                if (s.spawnType == "ki" && Haversine.IsWithinRadiusMeters(gps.Latitude, gps.Longitude, s.lat, s.lng, EconomyConfig.CollectionRadiusM))
                {
                    target = s;
                    break;
                }
            }
            if (target == null) { AppendLog("No Ki node in range."); yield break; }
            KiSessionResponse start = null;
            yield return api.StartKi(target.id, gps.Latitude, gps.Longitude, r => start = r);
            if (start == null) { AppendLog("Ki start failed."); yield break; }
            activeKi = start;
            AppendLog($"Ki session started: {start.affinityId} ({start.requiredSeconds}s)");
            StartCoroutine(KiPingLoop(start.sessionId));
        }

        IEnumerator KiPingLoop(string sessionId)
        {
            while (activeKi != null && activeKi.state != "completed")
            {
                yield return new WaitForSeconds(1f);
                KiSessionResponse ping = null;
                yield return api.PingKi(sessionId, gps.Latitude, gps.Longitude, r => ping = r);
                if (ping == null) break;
                activeKi = ping;
                if (ping.state == "completed")
                {
                    AppendLog($"Ki complete! +{ping.kiAwarded} {GameCatalog.Affinities[0].Name} tier Ki");
                    yield return api.RefreshWallet();
                }
            }
        }

        void AppendLog(string msg)
        {
            log.Insert(0, msg + "\n");
            if (logText != null) logText.text = log.ToString();
            Debug.Log(msg);
        }
    }
}