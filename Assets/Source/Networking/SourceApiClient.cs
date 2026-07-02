using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace SourceTCG.Networking
{
    [Serializable]
    public class AuthResponse { public string token; public string playerId; public string email; }

    [Serializable]
    public class EmailAuthRequest { public string email; public string password; }

    [Serializable]
    public class ApiErrorResponse { public string error; }

    [Serializable]
    public class WalletResponse
    {
        public int sourcePoints;
        public int extractionsRemaining;
        public int extractionsToday;
    }

    [Serializable]
    public class SpawnDto
    {
        public string id;
        public double lat;
        public double lng;
        public string spawnType;
        public string subtypeId;
        public int tier;
        public float distanceM;
    }

    [Serializable]
    public class SpawnsResponse
    {
        public SpawnDto[] spawns;
    }

    [Serializable]
    public class ExtractResponse
    {
        public int pointsAwarded;
        public int sourcePoints;
        public int extractionsRemaining;
    }

    [Serializable]
    public class GatherResponse
    {
        public string outcomeTier;
        public int multiplier;
        public int quantity;
        public string subtypeId;
        public int sourcePoints;
    }

    [Serializable]
    public class KiSessionResponse
    {
        public string sessionId;
        public string affinityId;
        public int requiredSeconds;
        public int elapsedSeconds;
        public float areaModifier;
        public int sourcePoints;
        public string state;
        public bool inRange;
        public int kiAwarded;
    }

    [Serializable]
    public class HexResponse
    {
        public string h3Index;
        public string zoneClass;
        public float areaModifier;
        public bool canExtract;
        public int extractionsRemaining;
    }

    [Serializable]
    public class InventoryItemDto
    {
        public string item_type;
        public string subtype_id;
        public int tier;
        public int quantity;
    }

    [Serializable]
    public class InventoryResponse
    {
        public InventoryItemDto[] items;
    }

    public class SourceApiClient : MonoBehaviour
    {
        [SerializeField] string baseUrl = "http://127.0.0.1:3847";

        public string Token { get; private set; }
        public WalletResponse Wallet { get; private set; }
        public HexResponse CurrentHex { get; private set; }
        public SpawnsResponse Nearby { get; private set; }

        public void SetBaseUrl(string url) => baseUrl = url;

        public IEnumerator GuestAuth(Action<bool> done)
        {
            yield return PostJson("/auth/guest", null, null, (ok, json) =>
            {
                if (!ok) { done?.Invoke(false); return; }
                var r = JsonUtility.FromJson<AuthResponse>(json);
                Token = r.token;
                done?.Invoke(true);
            });
        }

        public IEnumerator EmailSignup(string email, string password, Action<bool, string> done)
        {
            var body = JsonUtility.ToJson(new EmailAuthRequest { email = email, password = password });
            yield return PostJson("/auth/signup", body, null, (ok, json) =>
            {
                if (!ok) { done?.Invoke(false, ParseApiError(json)); return; }
                ApplyAuth(JsonUtility.FromJson<AuthResponse>(json));
                done?.Invoke(true, null);
            });
        }

        public IEnumerator EmailLogin(string email, string password, Action<bool, string> done)
        {
            var body = JsonUtility.ToJson(new EmailAuthRequest { email = email, password = password });
            yield return PostJson("/auth/login", body, null, (ok, json) =>
            {
                if (!ok) { done?.Invoke(false, ParseApiError(json)); return; }
                ApplyAuth(JsonUtility.FromJson<AuthResponse>(json));
                done?.Invoke(true, null);
            });
        }

        public void SetToken(string token) => Token = token;

        void ApplyAuth(AuthResponse response)
        {
            Token = response.token;
            if (!string.IsNullOrEmpty(response.email))
                PlayerPrefs.SetString("source_email", response.email);
            PlayerPrefs.SetString("source_token", response.token);
            PlayerPrefs.Save();
        }

        static string ParseApiError(string json)
        {
            if (string.IsNullOrEmpty(json)) return "Request failed";
            try
            {
                var err = JsonUtility.FromJson<ApiErrorResponse>(json);
                if (!string.IsNullOrEmpty(err.error)) return FormatApiError(err.error);
            }
            catch { /* fall through */ }
            return "Request failed";
        }

        static string FormatApiError(string code) => code switch
        {
            "invalid_email" => "Enter a valid email address.",
            "weak_password" => "Password must be at least 8 characters.",
            "email_taken" => "That email is already registered.",
            "invalid_credentials" => "Incorrect email or password.",
            _ => "Something went wrong. Try again.",
        };

        public IEnumerator RefreshWallet(Action<bool> done = null)
        {
            yield return Get("/player/wallet", (ok, json) =>
            {
                if (ok) Wallet = JsonUtility.FromJson<WalletResponse>(json);
                done?.Invoke(ok);
            });
        }

        public IEnumerator RefreshHex(double lat, double lng, Action<bool> done = null)
        {
            yield return Get($"/hex/current?lat={lat}&lng={lng}", (ok, json) =>
            {
                if (ok) CurrentHex = JsonUtility.FromJson<HexResponse>(json);
                done?.Invoke(ok);
            });
        }

        public IEnumerator RefreshNearby(double lat, double lng, Action<bool> done = null)
        {
            yield return Get($"/spawns/nearby?lat={lat}&lng={lng}", (ok, json) =>
            {
                if (ok) Nearby = JsonUtility.FromJson<SpawnsResponse>(json);
                done?.Invoke(ok);
            });
        }

        public IEnumerator Extract(double lat, double lng, Action<ExtractResponse> done)
        {
            var body = $"{{\"lat\":{lat},\"lng\":{lng}}}";
            yield return PostJson("/source/extract", body, (ok, json) =>
            {
                if (!ok) { done?.Invoke(null); return; }
                var r = JsonUtility.FromJson<ExtractResponse>(json);
                if (Wallet != null) { Wallet.sourcePoints = r.sourcePoints; Wallet.extractionsRemaining = r.extractionsRemaining; }
                done?.Invoke(r);
            });
        }

        public IEnumerator Gather(string spawnId, double lat, double lng, Action<GatherResponse> done)
        {
            var body = $"{{\"spawnId\":\"{spawnId}\",\"lat\":{lat},\"lng\":{lng}}}";
            yield return PostJson("/resources/gather", body, (ok, json) =>
            {
                if (!ok) { done?.Invoke(null); return; }
                done?.Invoke(JsonUtility.FromJson<GatherResponse>(json));
            });
        }

        public IEnumerator StartKi(string spawnId, double lat, double lng, Action<KiSessionResponse> done)
        {
            var body = $"{{\"spawnId\":\"{spawnId}\",\"lat\":{lat},\"lng\":{lng},\"targetKiTier\":1}}";
            yield return PostJson("/ki/sessions", body, (ok, json) =>
            {
                if (!ok) { done?.Invoke(null); return; }
                done?.Invoke(JsonUtility.FromJson<KiSessionResponse>(json));
            });
        }

        public IEnumerator PingKi(string sessionId, double lat, double lng, Action<KiSessionResponse> done)
        {
            var body = $"{{\"lat\":{lat},\"lng\":{lng}}}";
            yield return PostJson($"/ki/sessions/{sessionId}/ping", body, (ok, json) =>
            {
                if (!ok) { done?.Invoke(null); return; }
                done?.Invoke(JsonUtility.FromJson<KiSessionResponse>(json));
            });
        }

        IEnumerator Get(string path, Action<bool, string> done)
        {
            using var req = UnityWebRequest.Get(baseUrl + path);
            req.SetRequestHeader("Authorization", "Bearer " + Token);
            yield return req.SendWebRequest();
            done(req.result == UnityWebRequest.Result.Success, req.downloadHandler.text);
        }

        public IEnumerator GetInventory(Action<InventoryResponse> done)
        {
            yield return Get("/player/inventory", (ok, json) =>
            {
                if (!ok) { done?.Invoke(null); return; }
                done?.Invoke(JsonUtility.FromJson<InventoryResponse>(json));
            });
        }

        IEnumerator PostJson(string path, string body, Action<bool, string> done)
        {
            yield return PostJson(path, body, Token, done);
        }

        IEnumerator PostJson(string path, string body, string token, Action<bool, string> done)
        {
            using var req = new UnityWebRequest(baseUrl + path, "POST");
            var raw = Encoding.UTF8.GetBytes(body ?? "{}");
            req.uploadHandler = new UploadHandlerRaw(raw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(token))
                req.SetRequestHeader("Authorization", "Bearer " + token);
            yield return req.SendWebRequest();
            done(req.result == UnityWebRequest.Result.Success, req.downloadHandler.text);
        }
    }
}