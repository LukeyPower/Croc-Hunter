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

        // Called by GameManager when transitioning to Playing state
        void OnEnable()
        {
            _crocTimer = crocInterval - 2f; // first croc after 2s
            _fishTimer = fishInterval - 3f;
        }

        void Update()
        {
            _crocTimer += Time.deltaTime;
            _fishTimer += Time.deltaTime;

            if (_crocTimer >= crocInterval) { _crocTimer = 0f; SpawnCroc(); }
            if (_fishTimer >= fishInterval) { _fishTimer = 0f; SpawnFish(); }
        }

        private void SpawnCroc()
        {
            if (crocPrefab == null) return;
            float x = Random.Range(-riverHalfWidth, riverHalfWidth);
            Instantiate(crocPrefab, new Vector3(x, crocSpawnY, spawnZ), Quaternion.identity);
        }

        private void SpawnFish()
        {
            if (fishPrefab == null) return;
            float x = Random.Range(-riverHalfWidth, riverHalfWidth);
            Instantiate(fishPrefab, new Vector3(x, fishSpawnY, spawnZ), Quaternion.identity);
        }
    }
}
