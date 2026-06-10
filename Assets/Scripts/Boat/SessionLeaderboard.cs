using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CrocHunter
{
    public class SessionLeaderboard : MonoBehaviour
    {
        public static SessionLeaderboard Instance { get; private set; }

        private readonly List<ScoreEntry> _entries = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        public void AddEntry(ScoreEntry entry)
        {
            _entries.Add(entry);
            _entries.Sort((a, b) => b.Score.CompareTo(a.Score));
            Debug.Log($"[LEADERBOARD] Entry added: {entry.Name} — {entry.Score}pts");
        }

        public IReadOnlyList<ScoreEntry> TopFive => _entries.Take(5).ToList().AsReadOnly();
    }
}
