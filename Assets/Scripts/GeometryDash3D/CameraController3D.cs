using UnityEngine;

namespace GeometryDash3D
{
    public class CameraController3D : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _target;

        [Header("Offsets")]
        [SerializeField] private Vector3 _offset = new Vector3(4f, 2.5f, -12f);
        [SerializeField] private Vector3 _cameraRotation = new Vector3(8f, 0f, 0f);

        [Header("Follow Dynamics")]
        [SerializeField] private float _verticalSmoothTime = 0.15f;
        [SerializeField] private float _minCameraY = 1.5f;

        private float _currentVelocityY;
        private float _currentCameraY;
        private float _shakeDuration;
        private float _shakeMagnitude;

        public Transform Target
        {
            get => _target;
            set => _target = value;
        }

        private void Start()
        {
            transform.rotation = Quaternion.Euler(_cameraRotation);
            if (_target != null)
            {
                SnapToTarget();
            }
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            // Follow X directly with offset
            float targetX = _target.position.x + _offset.x;

            // Smoothly damp Y
            float desiredY = Mathf.Max(_minCameraY, _target.position.y + _offset.y);
            _currentCameraY = Mathf.SmoothDamp(_currentCameraY, desiredY, ref _currentVelocityY, _verticalSmoothTime);

            Vector3 newPosition = new Vector3(targetX, _currentCameraY, _offset.z);

            // Apply camera shake if active
            if (_shakeDuration > 0f)
            {
                Vector3 shakeOffset = Random.insideUnitSphere * _shakeMagnitude;
                shakeOffset.z = 0f;
                newPosition += shakeOffset;
                _shakeDuration -= Time.deltaTime;
            }

            transform.position = newPosition;
        }

        public void SnapToTarget()
        {
            if (_target == null) return;

            _currentVelocityY = 0f;
            _currentCameraY = Mathf.Max(_minCameraY, _target.position.y + _offset.y);
            transform.position = new Vector3(_target.position.x + _offset.x, _currentCameraY, _offset.z);
        }

        public void TriggerShake(float duration = 0.18f, float magnitude = 0.25f)
        {
            _shakeDuration = duration;
            _shakeMagnitude = magnitude;
        }
    }
}
