using System.Collections;
using UnityEngine;

namespace CrocHunter
{
    public class CrocController : MonoBehaviour
    {
        [SerializeField] private WorldScrollSettings settings;
        [SerializeField] private float emergeTime = 1.5f;
        [SerializeField] private float warningTime = 2.5f;
        [SerializeField] private float lungeSpeed = 10f;
        [SerializeField] private float hiddenY = -2f;
        [SerializeField] private float emergedY = 0f;

        private enum State { Hidden, Emerging, Warning, Lunging, Dead }
        private State _state = State.Hidden;
        private float _warningTimer;
        private int _bodyHits = 2;

        void Start()
        {
            Vector3 pos = transform.position;
            pos.y = hiddenY;
            transform.position = pos;
            StartCoroutine(EmergeSequence());
        }

        void Update()
        {
            if (_state == State.Dead) return;

            transform.position += Vector3.back * settings.ScrollSpeed * Time.deltaTime;

            if (_state == State.Warning)
            {
                _warningTimer -= Time.deltaTime;
                if (_warningTimer <= 0f)
                {
                    Debug.Log("[CROC] Warning timer expired — LUNGING at player");
                    _state = State.Lunging;
                }
            }
            else if (_state == State.Lunging)
            {
                Vector3 target = Camera.main.transform.position;
                transform.position = Vector3.MoveTowards(transform.position, target, lungeSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, target) < 2f)
                {
                    Debug.Log("[CROC] Reached player — dealing damage");
                    PlayerHealth.Instance.TakeDamage();
                    Destroy(gameObject);
                }
            }
        }

        private IEnumerator EmergeSequence()
        {
            float lead = Random.Range(0f, 2f);
            yield return new WaitForSeconds(lead);

            Debug.Log("[CROC] Emerging from water");
            _state = State.Emerging;
            float startY = hiddenY;
            float elapsed = 0f;

            while (elapsed < emergeTime)
            {
                elapsed += Time.deltaTime;
                Vector3 pos = transform.position;
                pos.y = Mathf.Lerp(startY, emergedY, elapsed / emergeTime);
                transform.position = pos;
                yield return null;
            }

            Vector3 final = transform.position;
            final.y = emergedY;
            transform.position = final;

            _warningTimer = warningTime;
            _state = State.Warning;
            Debug.Log($"[CROC] WARNING state — {warningTime}s to shoot it");
        }

        public void HitBody()
        {
            if (_state != State.Warning && _state != State.Emerging) return;
            _bodyHits--;
            Debug.Log($"[CROC] BODY HIT — {_bodyHits} hit(s) remaining");
            StartCoroutine(HitFeedback());
            if (_bodyHits <= 0)
                Die(false);
        }

        public void HitHead()
        {
            if (_state != State.Warning && _state != State.Emerging) return;
            Debug.Log("[CROC] HEAD SHOT — instant kill");
            StartCoroutine(HitFeedback());
            Die(true);
        }

        private void Die(bool headshot)
        {
            Debug.Log($"[CROC] Killed (headshot={headshot})");
            _state = State.Dead;
            GameStats.Instance?.RecordCrocKill(headshot);
            Destroy(gameObject, 0.15f);
        }

        private IEnumerator HitFeedback()
        {
            var renderers = GetComponentsInChildren<MeshRenderer>();
            var origColors = new Color[renderers.Length];
            Vector3 origScale = transform.localScale;

            for (int i = 0; i < renderers.Length; i++)
            {
                origColors[i] = renderers[i].material.color;
                renderers[i].material.color = Color.white;
            }
            transform.localScale = origScale * 1.35f;

            yield return new WaitForSeconds(0.12f);

            if (this == null) yield break;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    renderers[i].material.color = origColors[i];
            }
            transform.localScale = origScale;
        }
    }
}
