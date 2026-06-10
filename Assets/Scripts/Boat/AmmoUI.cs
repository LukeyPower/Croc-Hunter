using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CrocHunter
{
    public class AmmoUI : MonoBehaviour
    {
        [SerializeField] private GameShooter shooter;
        [SerializeField] private Image[] shellImages;
        [SerializeField] private Text reloadText;
        [SerializeField] private Color loadedColor  = new Color(0.90f, 0.60f, 0.10f);
        [SerializeField] private Color emptyColor   = new Color(0.25f, 0.25f, 0.25f);

        private Coroutine _reloadFlash;

        void Start()
        {
            shooter.OnAmmoChanged += Refresh;
            Refresh();
        }

        void OnDestroy()
        {
            if (shooter != null)
                shooter.OnAmmoChanged -= Refresh;
        }

        private void Refresh()
        {
            for (int i = 0; i < shellImages.Length; i++)
                shellImages[i].color = i < shooter.CurrentAmmo ? loadedColor : emptyColor;

            if (reloadText != null)
                reloadText.gameObject.SetActive(shooter.IsReloading);

            if (shooter.IsReloading && _reloadFlash == null)
                _reloadFlash = StartCoroutine(FlashShells());
            else if (!shooter.IsReloading && _reloadFlash != null)
            {
                StopCoroutine(_reloadFlash);
                _reloadFlash = null;
            }
        }

        private IEnumerator FlashShells()
        {
            while (shooter.IsReloading)
            {
                float t = Mathf.PingPong(Time.time * 3f, 1f);
                Color flash = Color.Lerp(emptyColor, loadedColor, t);
                foreach (var img in shellImages)
                    img.color = flash;
                yield return null;
            }
            _reloadFlash = null;
            Refresh();
        }
    }
}
