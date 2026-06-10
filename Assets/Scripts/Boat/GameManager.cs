using System;
using UnityEngine;

namespace CrocHunter
{
    public enum GameState { MainMenu, Playing, GameOver, NameEntry, Leaderboard }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private SpawnManager spawnManager;
        [SerializeField] private PlayerHealth playerHealth;

        public GameState State { get; private set; } = GameState.MainMenu;
        public event Action<GameState> OnStateChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (spawnManager == null) spawnManager = FindFirstObjectByType<SpawnManager>();
            if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        void Start()
        {
            playerHealth.OnDeath += HandlePlayerDied;
            SetState(GameState.MainMenu);
        }

        void Update()
        {
            if (State == GameState.Playing)
                GameStats.Instance.AddTime(Time.deltaTime);
        }

        public void StartGame()
        {
            foreach (var c in FindObjectsByType<CrocController>(FindObjectsSortMode.None))
                Destroy(c.gameObject);
            foreach (var f in FindObjectsByType<BarramundiFish>(FindObjectsSortMode.None))
                Destroy(f.gameObject);

            playerHealth.ResetHealth();
            GameStats.Instance.Reset();
            SetState(GameState.Playing);
        }

        public void GoToNameEntry()  => SetState(GameState.NameEntry);
        public void GoToMainMenu()   => SetState(GameState.MainMenu);

        public void SubmitNameEntry(string name)
        {
            var entry = new ScoreEntry
            {
                Name         = string.IsNullOrEmpty(name) ? "???" : name,
                Score        = GameStats.Instance.Score,
                CrocKills    = GameStats.Instance.CrocKills,
                Headshots    = GameStats.Instance.Headshots,
                FishShot     = GameStats.Instance.FishShot,
                TimeSurvived = GameStats.Instance.TimeSurvived,
                Accuracy     = GameStats.Instance.Accuracy
            };
            SessionLeaderboard.Instance.AddEntry(entry);
            SetState(GameState.Leaderboard);
        }

        private void HandlePlayerDied()
        {
            Debug.Log($"[GAME] Game over. Score: {GameStats.Instance.Score}");
            SetState(GameState.GameOver);
        }

        private void SetState(GameState next)
        {
            State = next;
            if (spawnManager != null)
                spawnManager.enabled = (next == GameState.Playing);
            Debug.Log($"[GAME] → {next}");
            OnStateChanged?.Invoke(next);
        }
    }
}
