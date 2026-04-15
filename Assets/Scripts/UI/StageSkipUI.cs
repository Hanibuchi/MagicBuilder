using UnityEngine;
using UnityEngine.UI;
using System;

public class StageSkipUI : MonoBehaviour
{
    [SerializeField] private Button skipButton;
    [SerializeField] private GameObject confirmationUIPrefab;
    [SerializeField, TextArea] private string skipMessage = "このステージをスキップしてクリアしますか？\n（1日1回のみ使用可能）";
    [SerializeField, TextArea] private string alreadyUsedMessage = "本日のスキップ機能はすでに使用済みです。明日またご利用ください。";

    private const string LastSkipDateKey = "LastSkipDate";
    private GameObject currentConfirmationInstance;

    private void Start()
    {
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(OnSkipButtonClicked);
            UpdateSkipButtonState();
        }
    }

    private void UpdateSkipButtonState()
    {
        // スキップできない場合はボタンのGameObject自体を非表示にする
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(CanSkip());
        }
    }

    private bool CanSkip()
    {
        // エンドレスモードの場合はスキップ不可
        if (GameManager.Instance != null && GameManager.Instance.CurrentStageConfig != null)
        {
            if (GameManager.Instance.CurrentStageConfig.clearCondition == StageClearCondition.Endless)
            {
                return false;
            }
        }

        string lastSkipDateStr = PlayerPrefs.GetString(LastSkipDateKey, "");
        if (string.IsNullOrEmpty(lastSkipDateStr))
        {
            return true;
        }

        if (DateTime.TryParse(lastSkipDateStr, out DateTime lastSkipDate))
        {
            return lastSkipDate.Date < DateTime.Now.Date;
        }

        return true; // パースに失敗した場合は念のため許可
    }

    private void RecordSkipUsage()
    {
        PlayerPrefs.SetString(LastSkipDateKey, DateTime.Now.ToString("o"));
        PlayerPrefs.Save();
        UpdateSkipButtonState();
    }

    private void OnSkipButtonClicked()
    {
        if (currentConfirmationInstance != null) return;

        if (!CanSkip())
        {
            ShowInfoMessage(alreadyUsedMessage);
            return;
        }

        if (confirmationUIPrefab == null)
        {
            Debug.LogWarning("ConfirmationUI Prefab is not set! Skipping immediately.");
            ExecuteSkip();
            return;
        }

        currentConfirmationInstance = Instantiate(confirmationUIPrefab);
        ConfirmationUI confirmationUI = currentConfirmationInstance.GetComponent<ConfirmationUI>();

        if (confirmationUI != null)
        {
            confirmationUI.Initialize(
                skipMessage,
                onYes: () =>
                {
                    ExecuteSkip();
                },
                onNo: () =>
                {
                    // キャンセル処理
                },
                onClosed: () =>
                {
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

    private void ShowInfoMessage(string message)
    {
        if (confirmationUIPrefab == null) return;

        currentConfirmationInstance = Instantiate(confirmationUIPrefab);
        ConfirmationUI confirmationUI = currentConfirmationInstance.GetComponent<ConfirmationUI>();

        if (confirmationUI != null)
        {
            // 確認ダイアログを情報表示用として代用（「はい」「いいえ」どちらを押しても閉じるだけ）
            confirmationUI.Initialize(
                message,
                onYes: () => {},
                onNo: () => {},
                onClosed: () =>
                {
                    currentConfirmationInstance = null;
                }
            );
        }
    }

    private bool isSkipping = false;
    private void ExecuteSkip()
    {
        if (isSkipping) return;
        isSkipping = true;

        RecordSkipUsage();

        if (StageManager.Instance != null)
        {
            Debug.Log("ステージをスキップします。");
            StageManager.Instance.HandleStageClear();
        }
        else
        {
            Debug.LogError("StageManager instance is missing.");
        }
    }

    [ContextMenu("Debug: Reset Skip State")]
    public void DebugResetSkipState()
    {
        PlayerPrefs.DeleteKey(LastSkipDateKey);
        PlayerPrefs.Save();
        
        // 実行中の場合はボタンの状態も更新
        if (Application.isPlaying)
        {
            UpdateSkipButtonState();
        }
        
        Debug.Log("デバッグ：スキップの使用状態をリセットしました（再びスキップ可能になりました）。");
    }
}
