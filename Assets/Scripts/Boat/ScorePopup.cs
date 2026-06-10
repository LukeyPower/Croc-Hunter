using UnityEngine;
using UnityEngine.UI;

namespace CrocHunter
{
    public class ScorePopup : MonoBehaviour
    {
        private Text _text;
        private Vector2 _startPos;
        private float _elapsed;

        private const float Duration = 1f;
        private const float Rise = 80f;

        public static void Spawn(Canvas canvas, Vector2 screenPos, string label, Color color)
        {
            var go = new GameObject("ScorePopup");
            go.transform.SetParent(canvas.transform, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(220f, 70f);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                screenPos, null, out Vector2 localPos);
            rt.anchoredPosition = localPos;

            var txt = go.AddComponent<Text>();
            txt.text = label;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 36;
            txt.fontStyle = FontStyle.Bold;
            txt.color = color;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false;

            var popup = go.AddComponent<ScorePopup>();
            popup._text = txt;
            popup._startPos = localPos;
        }

        void Update()
        {
            _elapsed += Time.deltaTime;
            float t = _elapsed / Duration;

            var rt = GetComponent<RectTransform>();
            rt.anchoredPosition = _startPos + Vector2.up * (Rise * t);

            Color c = _text.color;
            c.a = Mathf.Lerp(1f, 0f, t);
            _text.color = c;

            if (_elapsed >= Duration)
                Destroy(gameObject);
        }
    }
}
