using System;
using System.Collections;
using UnityEngine;

namespace GeometryDash3D
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private PlayerController3D _player;
        [SerializeField] private CameraController3D _cameraController;
        [SerializeField] private GoalTrigger _goalTrigger;
        [SerializeField] private Transform _spawnPoint;

        [Header("Timing")]
        [SerializeField] private float _respawnDelay = 0.45f;

        public event Action<int> OnAttemptChanged;
        public event Action OnLevelFinished;
        public event Action OnLevelReset;

        private Vector3 _spawnPosition;
        private float _startX;
        private float _goalX;
        private int _attemptCount = 1;
        private bool _isLevelCompleted = false;

        public int AttemptCount => _attemptCount;
        public bool IsLevelCompleted => _isLevelCompleted;
        public PlayerController3D Player => _player;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (_player == null)
            {
                _player = UnityEngine.Object.FindFirstObjectByType<PlayerController3D>();
            }

            if (_cameraController == null)
            {
                _cameraController = UnityEngine.Object.FindFirstObjectByType<CameraController3D>();
            }

            if (_goalTrigger == null)
            {
                _goalTrigger = UnityEngine.Object.FindFirstObjectByType<GoalTrigger>();
            }
        }

        private void Start()
        {
            if (_spawnPoint != null)
            {
                _spawnPosition = _spawnPoint.position;
            }
            else if (_player != null)
            {
                _spawnPosition = _player.transform.position;
            }

            _startX = _spawnPosition.x;

            if (_goalTrigger != null)
            {
                _goalX = _goalTrigger.transform.position.x;
            }
            else
            {
                _goalX = _startX + 100f; // Default level length
            }

            if (_player != null)
            {
                _player.OnDied += HandlePlayerDeath;
            }

            OnAttemptChanged?.Invoke(_attemptCount);
        }

        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.OnDied -= HandlePlayerDeath;
            }
        }

        public void ConfigureCourse(PlayerController3D player, CameraController3D cameraCtrl, GoalTrigger goal, Vector3 spawnPos)
        {
            _player = player;
            _cameraController = cameraCtrl;
            _goalTrigger = goal;
            _spawnPosition = spawnPos;
            _startX = spawnPos.x;

            if (_goalTrigger != null)
            {
                _goalX = _goalTrigger.transform.position.x;
            }

            if (_player != null)
            {
                _player.OnDied -= HandlePlayerDeath;
                _player.OnDied += HandlePlayerDeath;
            }
        }

        private void HandlePlayerDeath()
        {
            if (_isLevelCompleted) return;

            if (_cameraController != null)
            {
                _cameraController.TriggerShake(0.2f, 0.35f);
            }

            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(_respawnDelay);

            _attemptCount++;
            OnAttemptChanged?.Invoke(_attemptCount);

            if (_goalTrigger != null)
            {
                _goalTrigger.ResetTrigger();
            }

            if (_player != null)
            {
                _player.Respawn(_spawnPosition);
            }

            if (_cameraController != null)
            {
                _cameraController.SnapToTarget();
            }

            OnLevelReset?.Invoke();
        }

        public void OnLevelCompleted()
        {
            if (_isLevelCompleted) return;

            _isLevelCompleted = true;
            OnLevelFinished?.Invoke();
        }

        public void RestartFromBeginning()
        {
            _isLevelCompleted = false;
            _attemptCount = 1;
            OnAttemptChanged?.Invoke(_attemptCount);

            if (_goalTrigger != null)
            {
                _goalTrigger.ResetTrigger();
            }

            if (_player != null)
            {
                _player.Respawn(_spawnPosition);
            }

            if (_cameraController != null)
            {
                _cameraController.SnapToTarget();
            }

            OnLevelReset?.Invoke();
        }

        public float GetProgress01()
        {
            if (_player == null) return 0f;

            float distance = _goalX - _startX;
            if (distance <= 0.01f) return 1f;

            float currentDistance = _player.transform.position.x - _startX;
            return Mathf.Clamp01(currentDistance / distance);
        }
    }
}
