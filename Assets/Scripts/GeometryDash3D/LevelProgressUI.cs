using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GeometryDash3D
{
    public class LevelProgressUI : MonoBehaviour
    {
        [Header("Progress Elements")]
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private TextMeshProUGUI _attemptText;

        [Header("Victory Screen")]
        [SerializeField] private GameObject _victoryPanel;
        [SerializeField] private TextMeshProUGUI _victoryAttemptsText;
        [SerializeField] private Button _restartButton;

        private void Start()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnAttemptChanged += UpdateAttemptText;
                LevelManager.Instance.OnLevelFinished += HandleLevelFinished;
                LevelManager.Instance.OnLevelReset += HandleLevelReset;

                UpdateAttemptText(LevelManager.Instance.AttemptCount);
            }

            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(OnRestartClicked);
            }

            if (_victoryPanel != null)
            {
                _victoryPanel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnAttemptChanged -= UpdateAttemptText;
                LevelManager.Instance.OnLevelFinished -= HandleLevelFinished;
                LevelManager.Instance.OnLevelReset -= HandleLevelReset;
            }

            if (_restartButton != null)
            {
                _restartButton.onClick.RemoveListener(OnRestartClicked);
            }
        }

        private void Update()
        {
            if (LevelManager.Instance == null) return;

            float progress = LevelManager.Instance.GetProgress01();

            if (_progressBar != null)
            {
                _progressBar.value = progress;
            }

            if (_progressText != null)
            {
                int percent = Mathf.FloorToInt(progress * 100f);
                _progressText.text = $"{percent}%";
            }
        }

        private void UpdateAttemptText(int attempts)
        {
            if (_attemptText != null)
            {
                _attemptText.text = $"ATTEMPT {attempts}";
            }
        }

        private void HandleLevelFinished()
        {
            if (_victoryPanel != null)
            {
                _victoryPanel.SetActive(true);
            }

            if (_victoryAttemptsText != null && LevelManager.Instance != null)
            {
                _victoryAttemptsText.text = $"COMPLETED IN {LevelManager.Instance.AttemptCount} ATTEMPTS!";
            }
        }

        private void HandleLevelReset()
        {
            if (_victoryPanel != null)
            {
                _victoryPanel.SetActive(false);
            }
        }

        private void OnRestartClicked()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RestartFromBeginning();
            }
        }
    }
}
