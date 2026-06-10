using System;
using UnityEngine;

namespace CrocHunter
{
    public class GameStats : MonoBehaviour
    {
        public static GameStats Instance { get; private set; }

        public int   ShotsFired   { get; private set; }
        public int   ShotsHit     { get; private set; }
        public int   CrocKills    { get; private set; }
        public int   Headshots    { get; private set; }
        public int   FishShot     { get; private set; }
        public float TimeSurvived { get; private set; }

        // Score formula: croc kill 100pts + headshot bonus 50pts + fish 25pts + 5pts/sec survived
        public int   Score    => CrocKills * 100 + Headshots * 50 + FishShot * 25 + (int)(TimeSurvived * 5);
        public float Accuracy => ShotsFired > 0 ? ShotsHit / (float)ShotsFired * 100f : 0f;

        public event Action OnStatsChanged;
        public event Action<string, Vector3> OnScoreEvent;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        public void Reset()
        {
            ShotsFired = ShotsHit = CrocKills = Headshots = FishShot = 0;
            TimeSurvived = 0f;
            Debug.Log("[STATS] Reset");
        }

        public void AddTime(float dt)         => TimeSurvived += dt;
        public void RecordShotFired()         => ShotsFired++;
        public void RecordShotHit()           => ShotsHit++;

        public void RecordFishShot()
        {
            FishShot++;
            ShotsHit++;
            OnStatsChanged?.Invoke();
        }

        public void RecordCrocKill(bool head)
        {
            CrocKills++;
            if (head) Headshots++;
            OnStatsChanged?.Invoke();
        }

        public void FireScoreEvent(string label, Vector3 worldPos)
            => OnScoreEvent?.Invoke(label, worldPos);
    }
}
