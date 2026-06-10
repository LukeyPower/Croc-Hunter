using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CrocHunter
{
    public class GameOverUI : MonoBehaviour
    {
        private Text _statsText;

        void Start()
        {
            BuildUI();
            GameManager.Instance.OnStateChanged += OnStateChanged;
            gameObject.SetActive(GameManager.Instance.State == GameState.GameOver);
        }

        void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= OnStateChanged;
        }

        void Update()
        {
            if (!gameObject.activeInHierarchy) return;
            if (GameManager.Instance?.State != GameState.GameOver) return;

            if (Mouse.current?.leftButton.wasPressedThisFrame == true)
            {
                Debug.Log("[UI] Game over — going to name entry");
                GameManager.Instance.GoToNameEntry();
            }
        }

        private void OnStateChanged(GameState state)
        {
            gameObject.SetActive(state == GameState.GameOver);
            if (state == GameState.GameOver)
                RefreshStats();
        }

        private void RefreshStats()
        {
            if (_statsText == null || GameStats.Instance == null) return;
            var s = GameStats.Instance;
            int mins = (int)(s.TimeSurvived / 60f);
            int secs = (int)(s.TimeSurvived % 60f);
            _statsText.text =
                $"SCORE          {s.Score}\n\n" +
                $"Time Survived  {mins:00}:{secs:00}\n" +
                $"Crocs Killed   {s.CrocKills}\n" +
                $"Headshots      {s.Headshots}\n" +
                $"Fish Shot      {s.FishShot}\n" +
                $"Shots Fired    {s.ShotsFired}\n" +
                $"Accuracy       {s.Accuracy:0.0}%";
        }

        private void BuildUI()
        {
            var rt = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var bg = gameObject.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.85f);
            bg.raycastTarget = false;

            MakeText("GameOverTitle", "GAME OVER", 72, Color.red,
                new Vector2(0.5f, 0.78f), new Vector2(700f, 90f));

            // Stats block — centred, monospace-ish alignment via spaces
            var statsGO = new GameObject("StatsText");
            statsGO.transform.SetParent(transform, false);
            var srt = statsGO.AddComponent<RectTransform>();
            srt.anchorMin = srt.anchorMax = srt.pivot = new Vector2(0.5f, 0.52f);
            srt.sizeDelta = new Vector2(460f, 260f);
            srt.anchoredPosition = Vector2.zero;
            _statsText = statsGO.AddComponent<Text>();
            _statsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _statsText.fontSize = 26;
            _statsText.color = Color.white;
            _statsText.alignment = TextAnchor.UpperLeft;
            _statsText.raycastTarget = false;

            MakeText("ContinueText", "SHOOT TO ENTER INITIALS", 32, Color.green,
                new Vector2(0.5f, 0.18f), new Vector2(600f, 45f));
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
