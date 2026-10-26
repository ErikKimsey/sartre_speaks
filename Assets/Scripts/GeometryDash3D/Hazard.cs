using UnityEngine;

namespace GeometryDash3D
{
    public class Hazard : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            HandlePlayerHit(other.gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            HandlePlayerHit(collision.gameObject);
        }

        private void HandlePlayerHit(GameObject target)
        {
            PlayerController3D player = target.GetComponent<PlayerController3D>();
            if (player == null)
            {
                player = target.GetComponentInParent<PlayerController3D>();
            }

            if (player != null && !player.IsDead)
            {
                player.Die();
            }
        }
    }
}
