using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CrocHunter
{
    public class LeaderboardUI : MonoBehaviour
    {
        private GameObject _entriesRoot;

        void Start()
        {
            BuildUI();
            GameManager.Instance.OnStateChanged += OnStateChanged;
            gameObject.SetActive(GameManager.Instance.State == GameState.Leaderboard);
        }

        void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= OnStateChanged;
        }

        void Update()
        {
            if (!gameObject.activeInHierarchy) return;
            if (GameManager.Instance?.State != GameState.Leaderboard) return;

            if (Mouse.current?.leftButton.wasPressedThisFrame == true)
            {
                Debug.Log("[UI] Leaderboard — back to main menu");
                GameManager.Instance.GoToMainMenu();
            }
        }

        private void OnStateChanged(GameState state)
        {
            gameObject.SetActive(state == GameState.Leaderboard);
            if (state == GameState.Leaderboard)
                RefreshEntries();
        }

        private void RefreshEntries()
        {
            if (_entriesRoot == null) return;
            foreach (Transform child in _entriesRoot.transform)
                Destroy(child.gameObject);

            var topFive = SessionLeaderboard.Instance.TopFive;
            float rowHeight = 50f;

            for (int i = 0; i < topFive.Count; i++)
            {
                var e = topFive[i];
                int mins = (int)(e.TimeSurvived / 60f);
                int secs = (int)(e.TimeSurvived % 60f);
                string line = $"{i + 1}.  {e.Name}    {e.Score,6}pts    " +
                              $"{e.CrocKills}K / {e.Headshots}HS    " +
                              $"{mins:00}:{secs:00}    {e.Accuracy:0}%";

                var rowGO = new GameObject($"Entry{i}");
                rowGO.transform.SetParent(_entriesRoot.transform, false);
                var rowRT = rowGO.AddComponent<RectTransform>();
                rowRT.anchorMin = new Vector2(0f, 1f);
                rowRT.anchorMax = new Vector2(1f, 1f);
                rowRT.pivot     = new Vector2(0.5f, 1f);
                rowRT.sizeDelta = new Vector2(0f, rowHeight);
                rowRT.anchoredPosition = new Vector2(0f, -i * rowHeight);

                var rowTxt = rowGO.AddComponent<Text>();
                rowTxt.text = line;
                rowTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                rowTxt.fontSize = 28;
                rowTxt.color = i == 0 ? Color.yellow : Color.white;
                rowTxt.alignment = TextAnchor.MiddleLeft;
                rowTxt.raycastTarget = false;
            }

            if (topFive.Count == 0)
            {
                var emptyGO = new GameObject("Empty");
                emptyGO.transform.SetParent(_entriesRoot.transform, false);
                var emptyRT = emptyGO.AddComponent<RectTransform>();
                emptyRT.anchorMin = emptyRT.anchorMax = emptyRT.pivot = new Vector2(0.5f, 0.5f);
                emptyRT.sizeDelta = new Vector2(400f, 50f);
                var emptyTxt = emptyGO.AddComponent<Text>();
                emptyTxt.text = "No scores yet";
                emptyTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                emptyTxt.fontSize = 30;
                emptyTxt.color = Color.gray;
                emptyTxt.alignment = TextAnchor.MiddleCenter;
                emptyTxt.raycastTarget = false;
            }
        }

        private void BuildUI()
        {
            var rt = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var bg = gameObject.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.88f);
            bg.raycastTarget = false;

            MakeText("Title", "HIGH SCORES", 60, Color.yellow,
                new Vector2(0.5f, 0.86f), new Vector2(600f, 75f));

            // Entries container
            _entriesRoot = new GameObject("Entries");
            _entriesRoot.transform.SetParent(transform, false);
            var eRT = _entriesRoot.AddComponent<RectTransform>();
            eRT.anchorMin = new Vector2(0.05f, 0.25f);
            eRT.anchorMax = new Vector2(0.95f, 0.76f);
            eRT.offsetMin = eRT.offsetMax = Vector2.zero;

            MakeText("ContinueText", "SHOOT TO PLAY AGAIN", 32, Color.green,
                new Vector2(0.5f, 0.1f), new Vector2(600f, 45f));
        }

        private void MakeText(string goName, string content, int fontSize, Color color,
                               Vector2 anchorCenter, Vector2 size)
        {
            var go = new GameObject(goName);
            go.transform.SetParent(transform, false);
            var rt2 = go.AddComponent<RectTransform>();
            rt2.anchorMin = rt2.anchorMax = rt2.pivot = anchorCenter;
            rt2.sizeDelta = size;
            rt2.anchoredPosition = Vector2.zero;
            var txt = go.AddComponent<Text>();
            txt.text = content;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false;
        }
    }
}
