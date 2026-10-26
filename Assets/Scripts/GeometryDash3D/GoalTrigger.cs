using UnityEngine;

namespace GeometryDash3D
{
    public class GoalTrigger : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _completionParticles;

        private bool _triggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered) return;

            PlayerController3D player = other.GetComponent<PlayerController3D>();
            if (player == null)
            {
                player = other.GetComponentInParent<PlayerController3D>();
            }

            if (player != null && !player.IsDead)
            {
                _triggered = true;
                player.CompleteLevel();

                if (_completionParticles != null)
                {
                    _completionParticles.Play();
                }

                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.OnLevelCompleted();
                }
            }
        }

        public void ResetTrigger()
        {
            _triggered = false;
        }
    }
}
