using System;
using UnityEngine;

namespace CrocHunter
{
    public class PlayerHealth : MonoBehaviour
    {
        public static PlayerHealth Instance { get; private set; }

        [SerializeField] private int maxChunks = 3;

        public int CurrentChunks { get; private set; }

        public event Action OnHealthChanged;
        public event Action OnDamaged;
        public event Action OnDeath;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            CurrentChunks = maxChunks;
        }

        public void TakeDamage()
        {
            if (CurrentChunks <= 0) return;
            CurrentChunks--;
            Debug.Log($"[HEALTH] Damaged — {CurrentChunks}/{maxChunks} chunks remaining");
            OnHealthChanged?.Invoke();
            OnDamaged?.Invoke();
            if (CurrentChunks <= 0)
            {
                Debug.Log("[HEALTH] DEAD");
                OnDeath?.Invoke();
            }
        }

        public void ResetHealth()
        {
            CurrentChunks = maxChunks;
            Debug.Log($"[HEALTH] Reset to {maxChunks}/{maxChunks}");
            OnHealthChanged?.Invoke();
        }

        public void Heal()
        {
            if (CurrentChunks >= maxChunks)
            {
                Debug.Log("[HEALTH] Heal ignored — already at full health");
                return;
            }
            CurrentChunks++;
            Debug.Log($"[HEALTH] Healed — {CurrentChunks}/{maxChunks} chunks");
            OnHealthChanged?.Invoke();
        }
    }
}
