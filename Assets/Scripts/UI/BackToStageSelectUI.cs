using UnityEngine;
using UnityEngine.UI;

public class BackToStageSelectUI : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject confirmationUIPrefab;
    [SerializeField, TextArea] private string confirmMessage = "ステージ選択画面に戻りますか？\n（現在の進行状況は破棄されます）";

    private GameObject currentConfirmationInstance;

    private void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
    }

    private void OnBackButtonClicked()
    {
        // すでに確認画面が出ている場合は何もしない
        if (currentConfirmationInstance != null) return;

        if (confirmationUIPrefab == null)
        {
            Debug.LogWarning("ConfirmationUI Prefab is not set! Returning immediately.");
            ExecuteBackToSelect();
            return;
        }

        currentConfirmationInstance = Instantiate(confirmationUIPrefab);
        ConfirmationUI confirmationUI = currentConfirmationInstance.GetComponent<ConfirmationUI>();

        if (confirmationUI != null)
        {
            confirmationUI.Initialize(
                confirmMessage,
                onYes: () =>
                {
                    ExecuteBackToSelect();
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

    private bool isReturning = false;
    private void ExecuteBackToSelect()
    {
        if (isReturning) return;
        isReturning = true;

        if (TimeStopManager.Instance != null)
        {
            TimeStopManager.Instance.ResetAllRequests();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadStageSelectScene();
        }
        else
        {
            Debug.LogError("GameManager is missing. Cannot return to stage select.");
        }
    }
}
