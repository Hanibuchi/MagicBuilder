using UnityEngine;
using UnityEngine.UI;

public class RetryUI : MonoBehaviour
{
    [SerializeField] private Button retryButton;
    [SerializeField] private GameObject confirmationUIPrefab;
    [SerializeField, TextArea] private string retryMessage = "本当にリトライしますか？";

    private GameObject currentConfirmationInstance;

    private void Start()
    {
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryButtonClicked);
        }
    }

    private void OnRetryButtonClicked()
    {
        // すでに確認画面が出ている場合は何もしない
        if (currentConfirmationInstance != null) return;

        if (confirmationUIPrefab == null)
        {
            Debug.LogWarning("ConfirmationUI Prefab is not set! Retrying immediately.");
            ExecuteRetry();
            return;
        }

        currentConfirmationInstance = Instantiate(confirmationUIPrefab);
        ConfirmationUI confirmationUI = currentConfirmationInstance.GetComponent<ConfirmationUI>();

        if (confirmationUI != null)
        {
            confirmationUI.Initialize(
                retryMessage,
                onYes: () =>
                {
                    ExecuteRetry();
                },
                onNo: () =>
                {
                    // Noの場合の追加処理が必要ならここに記述
                },
                onClosed: () =>
                {
                    // 完全に閉じた（フェードアウト後）に参照をクリアして再度ボタンを押せるようにする
                    currentConfirmationInstance = null;
                }
            );
        }
        else
        {
            Debug.LogError("ConfirmationUI component not found on the instantiated prefab.");
            currentConfirmationInstance = null;
        }
    }

    private bool isRetrying = false;
    private void ExecuteRetry()
    {
        if (isRetrying) return;
        isRetrying = true;

        if (GameManager.Instance != null && GameManager.Instance.CurrentStageConfig != null)
        {
            TimeStopManager.Instance.ResetAllRequests();
            GameManager.Instance.OnStageStart(GameManager.Instance.CurrentStageConfig);
        }
        else
        {
            Debug.LogError("GameManager or CurrentStageConfig is missing. Re-loading current scene as fallback.");
            TimeStopManager.Instance.ResetAllRequests();
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}
