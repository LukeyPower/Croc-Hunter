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

        [Header("Camera Shake")]
        [SerializeField] private float shakeIntensity = 0.15f;
        [SerializeField] private float shakeDuration = 0.25f;

        private Coroutine _flashCoroutine;
        private Coroutine _shakeCoroutine;
        private Coroutine _crosshairCoroutine;
        private GameShooter _shooter;
        private Vector3 _cameraHomePos;

        private Text _scoreText;
        private Text _killText;

        void Start()
        {
            PlayerHealth.Instance.OnHealthChanged += RefreshChunks;
            PlayerHealth.Instance.OnDamaged += PlayFlash;
            RefreshChunks();

            if (Camera.main != null)
                _cameraHomePos = Camera.main.transform.localPosition;

            GameManager.Instance.OnStateChanged += OnStateChanged;

            if (GameStats.Instance != null)
                GameStats.Instance.OnScoreEvent += SpawnScorePopup;

            _shooter = FindFirstObjectByType<GameShooter>();
            if (_shooter != null)
                _shooter.OnHit += PulseCrosshair;

            CreateHUD();
            OnStateChanged(GameManager.Instance.State);
        }

        void OnDestroy()
        {
            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.OnHealthChanged -= RefreshChunks;
                PlayerHealth.Instance.OnDamaged -= PlayFlash;
            }
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= OnStateChanged;
            if (GameStats.Instance != null)
                GameStats.Instance.OnScoreEvent -= SpawnScorePopup;
            if (_shooter != null)
                _shooter.OnHit -= PulseCrosshair;
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

            if (GameManager.Instance?.State == GameState.Playing)
                RefreshHUD();
        }

        // ── HUD ──────────────────────────────────────────────────────────────

        private void CreateHUD()
        {
            if (canvas == null) return;
            var parent = canvas.GetComponent<RectTransform>();
            _scoreText = MakeHUDText("ScoreHUD", parent, new Vector2(-20f, -20f), 28);
            _killText  = MakeHUDText("KillHUD",  parent, new Vector2(-20f, -58f), 24);
        }

        private Text MakeHUDText(string goName, RectTransform parent, Vector2 offset, int fontSize)
        {
            var go = new GameObject(goName);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.one; // top-right
            rt.sizeDelta = new Vector2(260f, 36f);
            rt.anchoredPosition = offset;
            var txt = go.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = fontSize;
            txt.color = Color.white;
            txt.alignment = TextAnchor.UpperRight;
            txt.raycastTarget = false;
            return txt;
        }

        private void OnStateChanged(GameState state)
        {
            bool playing = state == GameState.Playing;
            if (_scoreText != null) _scoreText.gameObject.SetActive(playing);
            if (_killText  != null) _killText.gameObject.SetActive(playing);
        }

        private void RefreshHUD()
        {
            if (GameStats.Instance == null) return;
            if (_scoreText != null) _scoreText.text = $"SCORE  {GameStats.Instance.Score:N0}";
            if (_killText  != null) _killText.text  = $"{GameStats.Instance.CrocKills}K / {GameStats.Instance.Headshots}HS";
        }

        // ── Score popups ──────────────────────────────────────────────────────

        private void SpawnScorePopup(string label, Vector3 worldPos)
        {
            if (canvas == null || Camera.main == null) return;
            Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            Color color = label.Contains("HEADSHOT") ? Color.yellow : Color.white;
            ScorePopup.Spawn(canvas, screenPos, label, color);
        }

        // ── Health chunks ─────────────────────────────────────────────────────

        private void RefreshChunks()
        {
            int current = PlayerHealth.Instance.CurrentChunks;
            for (int i = 0; i < chunkImages.Length; i++)
                chunkImages[i].color = i < current ? Color.green : Color.gray;
        }

        // ── Damage flash + camera shake ───────────────────────────────────────

        private void PlayFlash()
        {
            if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
            _flashCoroutine = StartCoroutine(FlashCoroutine());

            if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = StartCoroutine(ShakeCoroutine());
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

        private IEnumerator ShakeCoroutine()
        {
            var cam = Camera.main;
            if (cam == null) yield break;

            cam.transform.localPosition = _cameraHomePos;
            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float strength = shakeIntensity * (1f - elapsed / shakeDuration);
                cam.transform.localPosition = _cameraHomePos + new Vector3(
                    Random.Range(-strength, strength),
                    Random.Range(-strength, strength),
                    0f
                );
                yield return null;
            }

            cam.transform.localPosition = _cameraHomePos;
        }

        // ── Crosshair hit feedback ────────────────────────────────────────────

        private void PulseCrosshair()
        {
            if (crosshairRect == null) return;
            if (_crosshairCoroutine != null) StopCoroutine(_crosshairCoroutine);
            _crosshairCoroutine = StartCoroutine(CrosshairPulseCoroutine());
        }

        private IEnumerator CrosshairPulseCoroutine()
        {
            float t = 0f;
            const float expandTime = 0.05f;
            const float contractTime = 0.10f;

            while (t < expandTime)
            {
                t += Time.deltaTime;
                crosshairRect.localScale = Vector3.one * Mathf.Lerp(1f, 1.6f, t / expandTime);
                yield return null;
            }

            t = 0f;
            while (t < contractTime)
            {
                t += Time.deltaTime;
                crosshairRect.localScale = Vector3.one * Mathf.Lerp(1.6f, 1f, t / contractTime);
                yield return null;
            }

            crosshairRect.localScale = Vector3.one;
        }
    }
}
