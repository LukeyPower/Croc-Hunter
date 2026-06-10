using UnityEngine;

namespace CrocHunter
{
    public class BarramundiFish : MonoBehaviour
    {
        [SerializeField] private WorldScrollSettings settings;
        [SerializeField] private float arcHeight = 3f;
        [SerializeField] private float arcDuration = 2.5f;

        private float _t;
        private Vector3 _startPos;
        private bool _shot;

        void Start()
        {
            _startPos = transform.position;
            Debug.Log($"[FISH] Barramundi spawned at {_startPos}");
        }

        void Update()
        {
            if (_shot) return;

            _t += Time.deltaTime / arcDuration;
            float height = 4f * arcHeight * _t * (1f - _t);
            float zOffset = -settings.ScrollSpeed * (_t * arcDuration);
            transform.position = _startPos + new Vector3(0f, height, zOffset);

            if (_t >= 1f)
            {
                Debug.Log("[FISH] Arc complete — missed (no heal)");
                Destroy(gameObject);
            }
        }

        public void OnShot()
        {
            if (_shot) return;
            _shot = true;
            Debug.Log("[FISH] SHOT — healing player");
            GameStats.Instance?.RecordFishShot();
            PlayerHealth.Instance.Heal();
            Destroy(gameObject);
        }
    }
}
