using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CrocHunter
{
    public class HealthUI : MonoBehaviour
    {
        [SerializeField] private Image[] chunkImages;
        [SerializeField] private Image damageFlashImage;
        [SerializeField] private float flashDuration = 0.3f;
        [SerializeField] private RectTransform crosshairRect;
        [SerializeField] private AimController aimController;
        [SerializeField] private Canvas canvas;

        private Coroutine _flashCoroutine;

        void Start()
        {
            PlayerHealth.Instance.OnHealthChanged += RefreshChunks;
            PlayerHealth.Instance.OnDamaged += PlayFlash;
            RefreshChunks();
        }

        void OnDestroy()
        {
            if (PlayerHealth.Instance == null) return;
            PlayerHealth.Instance.OnHealthChanged -= RefreshChunks;
            PlayerHealth.Instance.OnDamaged -= PlayFlash;
        }

        void LateUpdate()
        {
            if (aimController == null || crosshairRect == null || canvas == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                aimController.ScreenPosition,
                null,
                out Vector2 localPoint
            );
            crosshairRect.localPosition = localPoint;
        }

        private void RefreshChunks()
        {
            int current = PlayerHealth.Instance.CurrentChunks;
            for (int i = 0; i < chunkImages.Length; i++)
                chunkImages[i].color = i < current ? Color.green : Color.gray;
        }

        private void PlayFlash()
        {
            if (_flashCoroutine != null)
                StopCoroutine(_flashCoroutine);
            _flashCoroutine = StartCoroutine(FlashCoroutine());
        }

        private IEnumerator FlashCoroutine()
        {
            Color c = damageFlashImage.color;
            c.a = 1f;
            damageFlashImage.color = c;

            float elapsed = 0f;
            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Lerp(1f, 0f, elapsed / flashDuration);
                damageFlashImage.color = c;
                yield return null;
            }

            c.a = 0f;
            damageFlashImage.color = c;
        }
    }
}
