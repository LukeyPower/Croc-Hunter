using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CrocHunter
{
    public class MainMenuUI : MonoBehaviour
    {
        private Text _pulseText;
        private float _pulseTimer;

        void Start()
        {
            BuildUI();
            GameManager.Instance.OnStateChanged += OnStateChanged;
            gameObject.SetActive(GameManager.Instance.State == GameState.MainMenu);
        }

        void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= OnStateChanged;
        }

        void Update()
        {
            if (!gameObject.activeInHierarchy) return;

            // Pulse the start prompt
            if (_pulseText != null)
            {
                _pulseTimer += Time.deltaTime * 2f;
                Color c = _pulseText.color;
                c.a = Mathf.Abs(Mathf.Sin(_pulseTimer));
                _pulseText.color = c;
            }

            if (Mouse.current?.leftButton.wasPressedThisFrame == true)
            {
                Debug.Log("[UI] Main menu — start game");
                GameManager.Instance.StartGame();
            }
        }

        private void OnStateChanged(GameState state)
        {
            gameObject.SetActive(state == GameState.MainMenu);
        }

        private void BuildUI()
        {
            var rt = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            // Dark semi-transparent background
            var bg = gameObject.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.75f);
            bg.raycastTarget = false;

            // Title
            MakeText("TitleText", "CROC HUNTER", 80, Color.yellow,
                new Vector2(0.5f, 0.6f), new Vector2(900f, 100f));

            // Subtitle
            MakeText("SubText", "AUSTRALIA'S MOST DANGEROUS GAME", 28, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(700f, 40f));

            // Pulsing start prompt
            _pulseText = MakeText("StartText", "SHOOT TO START", 40, Color.green,
                new Vector2(0.5f, 0.38f), new Vector2(600f, 55f));
        }

        private Text MakeText(string goName, string content, int fontSize, Color color,
                              Vector2 anchorCenter, Vector2 size)
        {
            var go = new GameObject(goName);
            go.transform.SetParent(transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = anchorCenter;
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;
            var txt = go.AddComponent<Text>();
            txt.text = content;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false;
            return txt;
        }
    }
}
