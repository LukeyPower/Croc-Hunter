using System;
using System.Collections;
using Blamcon.Lightguns;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CrocHunter
{
    public class GameShooter : MonoBehaviour
    {
        [SerializeField] private AimController aimController;
        [SerializeField] private float maxRayDistance = 100f;
        [SerializeField] private LayerMask shootableLayer;
        [SerializeField] private int maxAmmo = 2;
        [SerializeField] private float reloadTime = 1f;

        public int CurrentAmmo { get; private set; }
        public int MaxAmmo => maxAmmo;
        public bool IsReloading { get; private set; }

        public event Action OnAmmoChanged;

        void Awake()
        {
            CurrentAmmo = maxAmmo;
            Debug.Log($"[SHOOT] GameShooter ready — {maxAmmo} shells, reload time {reloadTime}s. AimController: {(aimController != null ? "OK" : "MISSING — assign in Inspector")}");
        }

        void Update()
        {
            if (Mouse.current == null) return;
            // Only process shots during active gameplay
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Debug.Log("[INPUT] Left click detected");
                FireShot(null);
            }

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                Debug.Log("[INPUT] Right click detected");
                TryReload();
            }
        }

        // Called by PlayerInput Fire event (lightgun support)
        public void OnFire(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            // Skip if it came from a mouse — Update() already handles it to avoid double-firing
            if (context.control.device is Mouse) return;
            Debug.Log("[INPUT] OnFire callback (non-mouse device)");
            FireShot(context.control.device);
        }

        // Called by PlayerInput Reload event
        public void OnReload(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            if (context.control.device is Mouse) return;
            Debug.Log("[INPUT] OnReload callback (non-mouse device)");
            TryReload();
        }

        private void FireShot(InputDevice device)
        {
            if (IsReloading)
            {
                Debug.Log("[SHOOT] Blocked — reloading");
                return;
            }
            if (CurrentAmmo <= 0)
            {
                Debug.Log("[SHOOT] Blocked — out of ammo");
                return;
            }

            Vector2 screenPos = aimController.ScreenPosition;
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y, 0f));

            Debug.Log($"[SHOOT] Firing ray from screen pos {screenPos}");

            if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, shootableLayer))
            {
                var crocHitbox = hit.collider.GetComponent<CrocHitbox>();
                var fish       = hit.collider.GetComponent<BarramundiFish>();

                if (crocHitbox != null)
                {
                    string shotType = crocHitbox.IsHead ? "HEAD" : "BODY";
                    Debug.Log($"[SHOOT] HIT — {shotType} shot on '{hit.collider.transform.root.name}'");
                    GameStats.Instance?.RecordShotHit();
                    crocHitbox.TakeHit();
                }
                else if (fish != null)
                {
                    Debug.Log($"[SHOOT] HIT — Barramundi '{hit.collider.name}'");
                    fish.OnShot(); // fish calls GameStats.RecordFishShot() internally
                }
                else
                {
                    Debug.Log($"[SHOOT] HIT — '{hit.collider.name}' (not a croc/fish)");
                }
            }
            else
            {
                Debug.Log($"[SHOOT] MISS — nothing hit at {screenPos}");
            }

            GameStats.Instance?.RecordShotFired();
            CurrentAmmo--;
            Debug.Log($"[SHOOT] Ammo: {CurrentAmmo}/{maxAmmo}");
            OnAmmoChanged?.Invoke();

            if (device != null) TriggerRecoil(device);

            if (CurrentAmmo <= 0)
                StartCoroutine(AutoReloadCoroutine());
        }

        private void TryReload()
        {
            if (IsReloading)
            {
                Debug.Log("[RELOAD] Already reloading");
                return;
            }
            if (CurrentAmmo >= maxAmmo)
            {
                Debug.Log("[RELOAD] Already full");
                return;
            }
            Debug.Log("[RELOAD] Starting reload");
            StartCoroutine(ReloadCoroutine());
        }

        private IEnumerator AutoReloadCoroutine()
        {
            yield return new WaitForSeconds(0.4f);
            if (!IsReloading)
            {
                Debug.Log("[RELOAD] Auto-reload starting");
                StartCoroutine(ReloadCoroutine());
            }
        }

        private IEnumerator ReloadCoroutine()
        {
            IsReloading = true;
            OnAmmoChanged?.Invoke();
            Debug.Log($"[RELOAD] Reloading ({reloadTime}s)...");
            yield return new WaitForSeconds(reloadTime);
            CurrentAmmo = maxAmmo;
            IsReloading = false;
            OnAmmoChanged?.Invoke();
            Debug.Log($"[RELOAD] Complete — ammo {CurrentAmmo}/{maxAmmo}");
        }

        private void TriggerRecoil(InputDevice device)
        {
            if (device is BlamconLightgunHID)
                StartCoroutine(RecoilCoroutine());
        }

        private IEnumerator RecoilCoroutine()
        {
            yield return null;
            BlamconLightgunHID dev = InputSystem.GetDevice<BlamconLightgunHID>();
            if (dev != null)
                dev.ActivateRecoil(1);
        }
    }
}
