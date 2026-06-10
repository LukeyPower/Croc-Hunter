using UnityEngine;

namespace CrocHunter
{
    public class SpawnManager : MonoBehaviour
    {
        [SerializeField] private WorldScrollSettings settings;
        [SerializeField] private GameObject crocPrefab;
        [SerializeField] private GameObject fishPrefab;
        [SerializeField] private float crocInterval   = 4f;
        [SerializeField] private float fishInterval   = 7f;
        [SerializeField] private float spawnZ         = 20f;
        [SerializeField] private float riverHalfWidth = 3f;
        [SerializeField] private float crocSpawnY     = -2f;
        [SerializeField] private float fishSpawnY     = 0f;

        private float _crocTimer;
        private float _fishTimer;
        private float _elapsedTime;

        // Called by GameManager when transitioning to Playing state
        void OnEnable()
        {
            _crocTimer = crocInterval - 2f; // first croc after 2s
            _fishTimer = fishInterval - 3f;
            _elapsedTime = 0f;
        }

        void Update()
        {
            _elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsedTime / 120f);
            float effectiveCrocInterval = Mathf.Lerp(crocInterval, 2f, t);
            float effectiveWarning      = Mathf.Lerp(2.5f, 1.5f, t);

            _crocTimer += Time.deltaTime;
            _fishTimer += Time.deltaTime;

            if (_crocTimer >= effectiveCrocInterval) { _crocTimer = 0f; SpawnCroc(effectiveWarning); }
            if (_fishTimer >= fishInterval)           { _fishTimer = 0f; SpawnFish(); }
        }

        private void SpawnCroc(float warningDuration)
        {
            if (crocPrefab == null) return;
            float x = Random.Range(-riverHalfWidth, riverHalfWidth);
            var go = Instantiate(crocPrefab, new Vector3(x, crocSpawnY, spawnZ), Quaternion.identity);
            go.GetComponent<CrocController>()?.SetWarningDuration(warningDuration);
        }

        private void SpawnFish()
        {
            if (fishPrefab == null) return;
            float x = Random.Range(-riverHalfWidth, riverHalfWidth);
            Instantiate(fishPrefab, new Vector3(x, fishSpawnY, spawnZ), Quaternion.identity);
        }
    }
}
