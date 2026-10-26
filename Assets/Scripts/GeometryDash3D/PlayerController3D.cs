using System;
using UnityEngine;

namespace GeometryDash3D
{
    [RequireComponent(typeof(PlayerInputHandler))]
    public class PlayerController3D : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _forwardSpeed = 10f;
        [SerializeField] private float _gravity = -36f;
        [SerializeField] private float _jumpVelocity = 14.5f;

        [Header("Ground & Collision Detection")]
        [SerializeField] private LayerMask _groundLayer = ~0;
        [SerializeField] private LayerMask _hazardLayer = 0;
        [SerializeField] private Vector3 _boxSize = new Vector3(0.85f, 0.85f, 0.85f);
        [SerializeField] private float _groundCheckDistance = 0.12f;
        [SerializeField] private float _wallCheckDistance = 0.15f;

        [Header("Visuals & Feedback")]
        [SerializeField] private Transform _visualTransform;
        [SerializeField] private float _airRotationSpeed = 480f;
        [SerializeField] private ParticleSystem _deathParticlePrefab;
        [SerializeField] private TrailRenderer _trailRenderer;

        public event Action OnDied;
        public event Action OnJumped;
        public event Action OnBounced;

        private PlayerInputHandler _input;
        private Vector3 _velocity;
        private bool _isGrounded;
        private bool _isDead;
        private bool _isLevelCompleted;
        private float _currentVisualZRotation;

        public bool IsGrounded => _isGrounded;
        public bool IsDead => _isDead;
        public float ForwardSpeed => _forwardSpeed;
        public Vector3 Velocity => _velocity;

        private void Awake()
        {
            EnsureInput();
            if (_visualTransform == null && transform.childCount > 0)
            {
                _visualTransform = transform.GetChild(0);
            }
        }

        private void EnsureInput()
        {
            if (_input == null)
            {
                _input = GetComponent<PlayerInputHandler>();
            }
        }

        private void Start()
        {
            _currentVisualZRotation = 0f;
            EnsureZeroZPosition();
        }

        private void Update()
        {
            if (_isDead || _isLevelCompleted) return;

            CheckGround();
            CheckFrontalObstacle();
            HandleJump();
            ApplyMovement();
            HandleVisualRotation();
            EnsureZeroZPosition();
        }

        private void CheckGround()
        {
            Vector3 origin = transform.position;
            Vector3 halfExtents = new Vector3(_boxSize.x * 0.45f, 0.05f, _boxSize.z * 0.45f);
            Vector3 castDirection = Vector3.down;
            float castDist = (_boxSize.y * 0.5f) + _groundCheckDistance;

            bool hitGround = Physics.BoxCast(origin, halfExtents, castDirection, out RaycastHit hit, Quaternion.identity, castDist, _groundLayer, QueryTriggerInteraction.Ignore);

            if (hitGround && _velocity.y <= 0.01f)
            {
                if (!_isGrounded)
                {
                    // Just landed
                    _isGrounded = true;
                    _velocity.y = 0f;
                    SnapVisualRotationToNearest90();
                }
                
                // Align Y precisely to top of surface
                float targetY = hit.point.y + (_boxSize.y * 0.5f);
                if (Mathf.Abs(transform.position.y - targetY) < 0.2f)
                {
                    Vector3 pos = transform.position;
                    pos.y = targetY;
                    transform.position = pos;
                }
            }
            else
            {
                _isGrounded = false;
            }
        }

        private void CheckFrontalObstacle()
        {
            Vector3 origin = transform.position;
            Vector3 halfExtents = new Vector3(0.05f, _boxSize.y * 0.4f, _boxSize.z * 0.45f);
            float castDist = (_boxSize.x * 0.5f) + _wallCheckDistance;

            if (Physics.BoxCast(origin, halfExtents, Vector3.right, out RaycastHit hit, Quaternion.identity, castDist, _groundLayer, QueryTriggerInteraction.Ignore))
            {
                // If hit face normal opposes our forward direction, player crashed into wall
                if (hit.normal.x < -0.6f)
                {
                    Die();
                }
            }
        }

        private void HandleJump()
        {
            if (_isGrounded)
            {
                if (_input.HasBufferedJump || _input.IsJumpHeld)
                {
                    ExecuteJump();
                }
            }
        }

        private void ExecuteJump()
        {
            _velocity.y = _jumpVelocity;
            _isGrounded = false;
            EnsureInput();
            _input?.ConsumeJumpBuffer();
            OnJumped?.Invoke();
        }

        public void Bounce(float bounceVelocity)
        {
            if (_isDead || _isLevelCompleted) return;

            _velocity.y = bounceVelocity;
            _isGrounded = false;
            EnsureInput();
            _input?.ConsumeJumpBuffer();
            OnBounced?.Invoke();
        }

        private void ApplyMovement()
        {
            // Apply gravity when in air
            if (!_isGrounded)
            {
                _velocity.y += _gravity * Time.deltaTime;
                if (_velocity.y < -35f)
                {
                    _velocity.y = -35f; // Terminal vertical velocity
                }
            }

            _velocity.x = _forwardSpeed;
            _velocity.z = 0f;

            transform.position += _velocity * Time.deltaTime;

            // Kill if fallen out of bounds (abyss pit)
            if (transform.position.y < -10f)
            {
                Die();
            }
        }

        private void HandleVisualRotation()
        {
            if (_visualTransform == null) return;

            if (!_isGrounded)
            {
                // Rotate clockwise around Z axis in flight
                _currentVisualZRotation -= _airRotationSpeed * Time.deltaTime;
                _currentVisualZRotation %= 360f;
                _visualTransform.localRotation = Quaternion.Euler(0f, 0f, _currentVisualZRotation);
            }
        }

        private void SnapVisualRotationToNearest90()
        {
            if (_visualTransform == null) return;

            float snapped = Mathf.Round(_currentVisualZRotation / 90f) * 90f;
            _currentVisualZRotation = snapped % 360f;
            _visualTransform.localRotation = Quaternion.Euler(0f, 0f, _currentVisualZRotation);
        }

        private void EnsureZeroZPosition()
        {
            Vector3 pos = transform.position;
            if (pos.z != 0f)
            {
                pos.z = 0f;
                transform.position = pos;
            }
        }

        public void Die()
        {
            if (_isDead || _isLevelCompleted) return;

            _isDead = true;
            _velocity = Vector3.zero;

            if (_visualTransform != null)
            {
                _visualTransform.gameObject.SetActive(false);
            }

            if (_deathParticlePrefab != null)
            {
                Instantiate(_deathParticlePrefab, transform.position, Quaternion.identity);
            }

            if (_trailRenderer != null)
            {
                _trailRenderer.Clear();
                _trailRenderer.enabled = false;
            }

            OnDied?.Invoke();
        }

        public void Respawn(Vector3 spawnPosition)
        {
            _isDead = false;
            _isLevelCompleted = false;
            _velocity = Vector3.zero;
            _currentVisualZRotation = 0f;

            transform.position = spawnPosition;
            EnsureZeroZPosition();

            if (_visualTransform != null)
            {
                _visualTransform.localRotation = Quaternion.identity;
                _visualTransform.gameObject.SetActive(true);
            }

            if (_trailRenderer != null)
            {
                _trailRenderer.Clear();
                _trailRenderer.enabled = true;
            }
        }

        public void CompleteLevel()
        {
            if (_isDead) return;
            _isLevelCompleted = true;
            _velocity = Vector3.zero;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Vector3 groundCenter = transform.position + Vector3.down * ((_boxSize.y * 0.5f) + (_groundCheckDistance * 0.5f));
            Gizmos.DrawWireCube(groundCenter, new Vector3(_boxSize.x * 0.9f, _groundCheckDistance, _boxSize.z * 0.9f));

            Gizmos.color = Color.red;
            Vector3 wallCenter = transform.position + Vector3.right * ((_boxSize.x * 0.5f) + (_wallCheckDistance * 0.5f));
            Gizmos.DrawWireCube(wallCenter, new Vector3(_wallCheckDistance, _boxSize.y * 0.8f, _boxSize.z * 0.9f));
        }
    }
}
