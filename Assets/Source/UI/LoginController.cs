using System.Collections;
using SourceTCG.Core;
using SourceTCG.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SourceTCG.UI
{
    public class LoginController : MonoBehaviour
    {
        const string TokenKey = "source_token";
        const string EmailKey = "source_email";

        [SerializeField] SourceApiClient api;
        [SerializeField] string worldMapScene = "WorldMap";

        InputField emailField;
        InputField passwordField;
        Text statusText;
        Button loginButton;
        Button signupButton;
        bool busy;

        void Awake()
        {
            if (api == null && SourceSession.Instance != null) api = SourceSession.Instance.Api;
            if (api == null) api = FindFirstObjectByType<SourceApiClient>();
            BuildLoginUi();
        }

        void Start() => StartCoroutine(Boot());

        IEnumerator Boot()
        {
            var savedToken = PlayerPrefs.GetString(TokenKey, "");
            if (!string.IsNullOrEmpty(savedToken))
            {
                SetStatus("Restoring session...");
                api.SetToken(savedToken);
                var ok = false;
                yield return api.RefreshWallet(b => ok = b);
                if (ok)
                {
                    SceneManager.LoadScene(worldMapScene);
                    yield break;
                }
                api.SetToken(null);
            }

            var savedEmail = PlayerPrefs.GetString(EmailKey, "");
            if (!string.IsNullOrEmpty(savedEmail) && emailField != null)
                emailField.text = savedEmail;
            SetStatus("Sign up or log in to play.");
        }

        void BuildLoginUi()
        {
            RuntimeUiFactory.EnsureEventSystem();
            var canvas = RuntimeUiFactory.EnsureCanvas();

            var panel = new GameObject("LoginPanel");
            panel.transform.SetParent(canvas.transform, false);
            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(380, 420);
            var panelBg = panel.AddComponent<Image>();
            panelBg.color = new Color(0.08f, 0.1f, 0.16f, 0.94f);

            var layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 28, 28);
            layout.spacing = 14;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateLabel(panel.transform, "Source TCGMMO", 26, FontStyle.Bold);
            CreateLabel(panel.transform, "Location collection alpha", 14, FontStyle.Normal);
            emailField = RuntimeUiFactory.CreateInputField(panel.transform, "Email", "Email address", false);
            passwordField = RuntimeUiFactory.CreateInputField(panel.transform, "Password", "Password (8+ chars)", true);
            statusText = CreateLabel(panel.transform, "", 13, FontStyle.Italic);

            var buttonRow = new GameObject("Buttons");
            buttonRow.transform.SetParent(panel.transform, false);
            var rowLayout = buttonRow.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 12;
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlWidth = true;
            rowLayout.childForceExpandWidth = true;

            loginButton = CreatePanelButton(buttonRow.transform, "Log In");
            signupButton = CreatePanelButton(buttonRow.transform, "Sign Up");

            loginButton.onClick.AddListener(() => StartCoroutine(OnLogin()));
            signupButton.onClick.AddListener(() => StartCoroutine(OnSignup()));
        }

        static Text CreateLabel(Transform parent, string text, int size, FontStyle style)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(320, size + 10);
            var label = go.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = size;
            label.fontStyle = style;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.text = text;
            return label;
        }

        static Button CreatePanelButton(Transform parent, string label)
        {
            var go = new GameObject(label);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(150, 40);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.45f, 0.85f);
            var btn = go.AddComponent<Button>();
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var text = textGo.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.fontSize = 15;
            return btn;
        }

        IEnumerator OnLogin()
        {
            if (busy) yield break;
            var email = emailField.text.Trim();
            var password = passwordField.text;
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                SetStatus("Enter email and password.");
                yield break;
            }

            busy = true;
            SetButtonsEnabled(false);
            SetStatus("Logging in...");
            var ok = false;
            string err = null;
            yield return api.EmailLogin(email, password, (success, message) =>
            {
                ok = success;
                err = message;
            });
            if (!ok)
            {
                SetStatus(err ?? "Login failed.");
                SetButtonsEnabled(true);
                busy = false;
                yield break;
            }

            yield return EnterWorld();
        }

        IEnumerator OnSignup()
        {
            if (busy) yield break;
            var email = emailField.text.Trim();
            var password = passwordField.text;
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                SetStatus("Enter email and password.");
                yield break;
            }

            busy = true;
            SetButtonsEnabled(false);
            SetStatus("Creating account...");
            var ok = false;
            string err = null;
            yield return api.EmailSignup(email, password, (success, message) =>
            {
                ok = success;
                err = message;
            });
            if (!ok)
            {
                SetStatus(err ?? "Sign up failed.");
                SetButtonsEnabled(true);
                busy = false;
                yield break;
            }

            yield return EnterWorld();
        }

        IEnumerator EnterWorld()
        {
            SetStatus("Loading world...");
            var ok = false;
            yield return api.RefreshWallet(b => ok = b);
            if (!ok)
            {
                SetStatus("Connected but wallet sync failed.");
                SetButtonsEnabled(true);
                busy = false;
                yield break;
            }
            SceneManager.LoadScene(worldMapScene);
        }

        void SetStatus(string message)
        {
            if (statusText != null) statusText.text = message;
        }

        void SetButtonsEnabled(bool enabled)
        {
            if (loginButton != null) loginButton.interactable = enabled;
            if (signupButton != null) signupButton.interactable = enabled;
        }
    }
}