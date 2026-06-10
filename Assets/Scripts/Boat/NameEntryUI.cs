using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CrocHunter
{
    // Keyboard layout: 4 rows of 7 (A-Z + backspace)
    // Player aims crosshair at a letter and left-clicks to select it.
    // After 3 letters the name is submitted automatically.
    public class NameEntryUI : MonoBehaviour
    {
        [SerializeField] private AimController aimController;

        private const int   Cols       = 7;
        private const float BtnSize    = 70f;
        private const float BtnGap     = 8f;
        private const char  Backspace  = '\b';

        private readonly List<char>         _selected    = new();
        private readonly List<LetterButton> _allButtons  = new();
        private LetterButton                _highlighted;
        private Text[]                      _slotTexts   = new Text[3];

        void Start()
        {
            if (aimController == null)
                aimController = FindFirstObjectByType<AimController>();

            BuildUI();
            GameManager.Instance.OnStateChanged += OnStateChanged;
            gameObject.SetActive(GameManager.Instance.State == GameState.NameEntry);
        }

        void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= OnStateChanged;
        }

        void Update()
        {
            if (!gameObject.activeInHierarchy) return;
            UpdateHighlight();

            if (Mouse.current?.leftButton.wasPressedThisFrame == true)
                SelectUnderCursor();
        }

        private void OnStateChanged(GameState state)
        {
            gameObject.SetActive(state == GameState.NameEntry);
            if (state == GameState.NameEntry)
            {
                _selected.Clear();
                RefreshSlots();
            }
        }

        private void UpdateHighlight()
        {
            if (_highlighted != null) { _highlighted.SetHighlighted(false); _highlighted = null; }

            var btn = GetButtonUnderCursor();
            if (btn != null) { btn.SetHighlighted(true); _highlighted = btn; }
        }

        private void SelectUnderCursor()
        {
            var btn = GetButtonUnderCursor();
            if (btn == null) return;

            if (btn.Letter == Backspace)
            {
                if (_selected.Count > 0) _selected.RemoveAt(_selected.Count - 1);
                Debug.Log("[NAME] Backspace");
            }
            else if (_selected.Count < 3)
            {
                _selected.Add(btn.Letter);
                Debug.Log($"[NAME] Letter '{btn.Letter}' selected — {new string(_selected.ToArray())}");
            }

            RefreshSlots();

            if (_selected.Count == 3)
            {
                string name = new string(_selected.ToArray());
                Debug.Log($"[NAME] Submitted: {name}");
                GameManager.Instance.SubmitNameEntry(name);
            }
        }

        private LetterButton GetButtonUnderCursor()
        {
            var ped = new PointerEventData(EventSystem.current);
            ped.position = aimController != null
                ? aimController.ScreenPosition
                : (Mouse.current?.position.ReadValue() ?? Vector2.zero);

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(ped, results);

            foreach (var r in results)
            {
                var btn = r.gameObject.GetComponent<LetterButton>();
                if (btn != null) return btn;
            }
            return null;
        }

        private void RefreshSlots()
        {
            for (int i = 0; i < 3; i++)
            {
                if (_slotTexts[i] != null)
                    _slotTexts[i].text = i < _selected.Count ? _selected[i].ToString() : "_";
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

            MakeText("InstructionText", "ENTER YOUR INITIALS", 38, Color.yellow,
                new Vector2(0.5f, 0.82f), new Vector2(600f, 50f));

            // 3 letter slots
            float slotSpacing = 90f;
            float slotStartX  = -(slotSpacing);
            for (int i = 0; i < 3; i++)
            {
                var slotGO = new GameObject($"Slot{i}");
                slotGO.transform.SetParent(transform, false);
                var slotRT = slotGO.AddComponent<RectTransform>();
                slotRT.anchorMin = slotRT.anchorMax = slotRT.pivot = new Vector2(0.5f, 0.68f);
                slotRT.sizeDelta = new Vector2(70f, 80f);
                slotRT.anchoredPosition = new Vector2(slotStartX + i * slotSpacing, 0f);

                // Slot background border
                var slotBg = slotGO.AddComponent<Image>();
                slotBg.color = new Color(0.2f, 0.2f, 0.3f, 1f);
                slotBg.raycastTarget = false;

                var txtGO = new GameObject("Letter");
                txtGO.transform.SetParent(slotGO.transform, false);
                var txtRT = txtGO.AddComponent<RectTransform>();
                txtRT.anchorMin = Vector2.zero;
                txtRT.anchorMax = Vector2.one;
                txtRT.offsetMin = txtRT.offsetMax = Vector2.zero;
                var txt = txtGO.AddComponent<Text>();
                txt.text = "_";
                txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txt.fontSize = 56;
                txt.color = Color.white;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.raycastTarget = false;
                _slotTexts[i] = txt;
            }

            MakeText("ShootPrompt", "AIM AND SHOOT A LETTER", 26, Color.gray,
                new Vector2(0.5f, 0.57f), new Vector2(500f, 35f));

            // Letter grid — centred on screen, rows of 7
            // Letters: A-Z (26) + backspace = 27, fits in 4 rows of 7 (28 cells, last cell empty)
            char[] letters = new char[27];
            for (int i = 0; i < 26; i++) letters[i] = (char)('A' + i);
            letters[26] = Backspace;

            float gridW = Cols * BtnSize + (Cols - 1) * BtnGap;
            int rows = Mathf.CeilToInt(letters.Length / (float)Cols);
            float gridH = rows * BtnSize + (rows - 1) * BtnGap;

            var gridGO = new GameObject("LetterGrid");
            gridGO.transform.SetParent(transform, false);
            var gridRT = gridGO.AddComponent<RectTransform>();
            gridRT.anchorMin = gridRT.anchorMax = gridRT.pivot = new Vector2(0.5f, 0.35f);
            gridRT.sizeDelta = new Vector2(gridW, gridH);
            gridRT.anchoredPosition = Vector2.zero;

            for (int i = 0; i < letters.Length; i++)
            {
                int col = i % Cols;
                int row = i / Cols;

                float x = col * (BtnSize + BtnGap) - gridW * 0.5f + BtnSize * 0.5f;
                float y = -row * (BtnSize + BtnGap) + gridH * 0.5f - BtnSize * 0.5f;

                var btnGO = new GameObject(letters[i] == Backspace ? "BackspaceBtn" : $"Btn_{letters[i]}");
                btnGO.transform.SetParent(gridGO.transform, false);
                var btnRT = btnGO.AddComponent<RectTransform>();
                btnRT.anchorMin = btnRT.anchorMax = btnRT.pivot = new Vector2(0.5f, 0.5f);
                btnRT.sizeDelta = new Vector2(BtnSize, BtnSize);
                btnRT.anchoredPosition = new Vector2(x, y);

                var img = btnGO.AddComponent<Image>();
                img.color = new Color(0.15f, 0.15f, 0.25f, 1f);
                img.raycastTarget = true;

                var lbl = new GameObject("Label");
                lbl.transform.SetParent(btnGO.transform, false);
                var lblRT = lbl.AddComponent<RectTransform>();
                lblRT.anchorMin = Vector2.zero;
                lblRT.anchorMax = Vector2.one;
                lblRT.offsetMin = lblRT.offsetMax = Vector2.zero;
                var lblTxt = lbl.AddComponent<Text>();
                lblTxt.text = letters[i] == Backspace ? "←" : letters[i].ToString();
                lblTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                lblTxt.fontSize = 36;
                lblTxt.color = letters[i] == Backspace ? Color.red : Color.white;
                lblTxt.alignment = TextAnchor.MiddleCenter;
                lblTxt.raycastTarget = false;

                var letterBtn = btnGO.AddComponent<LetterButton>();
                letterBtn.Init(letters[i]);
                _allButtons.Add(letterBtn);
            }
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
