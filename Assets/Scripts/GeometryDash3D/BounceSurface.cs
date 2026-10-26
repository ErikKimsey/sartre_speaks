using System.Collections;
using UnityEngine;

namespace GeometryDash3D
{
    public class BounceSurface : MonoBehaviour
    {
        [Header("Bounce Settings")]
        [SerializeField] private float _bounceVelocity = 19f;
        [SerializeField] private ParticleSystem _bounceParticles;

        private Vector3 _originalScale;
        private Coroutine _squashCoroutine;

        private void Awake()
        {
            _originalScale = transform.localScale;
        }

        private void OnTriggerEnter(Collider other)
        {
            TriggerBounce(other.gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            TriggerBounce(collision.gameObject);
        }

        private void TriggerBounce(GameObject target)
        {
            PlayerController3D player = target.GetComponent<PlayerController3D>();
            if (player == null)
            {
                player = target.GetComponentInParent<PlayerController3D>();
            }

            if (player != null && !player.IsDead)
            {
                player.Bounce(_bounceVelocity);

                if (_bounceParticles != null)
                {
                    _bounceParticles.Play();
                }

                if (_squashCoroutine != null)
                {
                    StopCoroutine(_squashCoroutine);
                }
                _squashCoroutine = StartCoroutine(AnimateSquashAndStretch());
            }
        }

        private IEnumerator AnimateSquashAndStretch()
        {
            Vector3 squashed = new Vector3(_originalScale.x * 1.3f, _originalScale.y * 0.4f, _originalScale.z * 1.3f);
            float duration = 0.08f;
            float elapsed = 0f;

            // Compress
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(_originalScale, squashed, elapsed / duration);
                yield return null;
            }

            // Rebound back
            elapsed = 0f;
            float reboundDuration = 0.16f;
            while (elapsed < reboundDuration)
            {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(squashed, _originalScale, elapsed / reboundDuration);
                yield return null;
            }

            transform.localScale = _originalScale;
            _squashCoroutine = null;
        }
    }
}
