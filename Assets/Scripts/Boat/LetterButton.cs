using UnityEngine;
using UnityEngine.UI;

namespace CrocHunter
{
    public class LetterButton : MonoBehaviour
    {
        public char Letter { get; private set; }

        private Image _bg;
        private Color _normalColor  = new Color(0.15f, 0.15f, 0.25f, 1f);
        private Color _hoverColor   = new Color(0.35f, 0.55f, 0.90f, 1f);

        public void Init(char letter)
        {
            Letter = letter;
            _bg = GetComponent<Image>();
            if (_bg) _bg.color = _normalColor;
        }

        public void SetHighlighted(bool on)
        {
            if (_bg) _bg.color = on ? _hoverColor : _normalColor;
        }
    }
}
