using UnityEngine;
using UnityEngine.UI;
using System;

public class StageSkipUI : MonoBehaviour
{
    [SerializeField] private Button skipButton;
    [SerializeField] private GameObject confirmationUIPrefab;
    
    [Header("Settings")]
    [SerializeField, Tooltip("スキップが回復するまでの時間（時間単位）")]
    private int skipRecoveryHours = 3;

    [Space(10)]
    [SerializeField] private AudioClip openConfirmSE;
    [SerializeField] private AudioClip executeSkipSE;

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
        // エンドレスモードの場合はボタン自体を非表示にする
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(!IsEndlessMode());
        }
    }

    private bool IsEndlessMode()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentStageConfig != null)
        {
            return GameManager.Instance.CurrentStageConfig.clearCondition == StageClearCondition.Endless;
        }
        return false;
    }

    private bool CanSkip()
    {
        // エンドレスモードの場合はスキップ不可
        if (IsEndlessMode()) return false;

        string lastSkipDateStr = PlayerPrefs.GetString(LastSkipDateKey, "");
        if (string.IsNullOrEmpty(lastSkipDateStr))
        {
            return true;
        }

        if (DateTime.TryParse(lastSkipDateStr, out DateTime lastSkipDate))
        {
            return DateTime.Now >= lastSkipDate.AddHours(skipRecoveryHours);
        }

        return true; // パースに失敗した場合は念のため許可
    }

    private string GetAlreadyUsedMessageWithTime()
    {
        string baseMessage = "スキップ回復まで";
        string lastSkipDateStr = PlayerPrefs.GetString(LastSkipDateKey, "");
        if (string.IsNullOrEmpty(lastSkipDateStr) || !DateTime.TryParse(lastSkipDateStr, out DateTime lastSkipDate))
        {
            return baseMessage;
        }

        DateTime now = DateTime.Now;
        DateTime recoveryTime = lastSkipDate.AddHours(skipRecoveryHours);
        TimeSpan remainingTime = recoveryTime - now;

        if (remainingTime.TotalSeconds <= 0)
        {
            return $"{baseMessage}\nあと 0秒";
        }
        
        string timeText = "";
        if (remainingTime.Hours > 0)
        {
            timeText += $"{remainingTime.Hours}時間";
        }
        if (remainingTime.Minutes > 0 || remainingTime.Hours > 0)
        {
            timeText += $"{remainingTime.Minutes}分";
        }
        timeText += $"{remainingTime.Seconds}秒";
        
        return $"{baseMessage}\nあと {timeText}";
    }

    private void Update()
    {
        if (currentConfirmationInstance != null && !CanSkip())
        {
            ConfirmationUI confirmationUI = currentConfirmationInstance.GetComponent<ConfirmationUI>();
            if (confirmationUI != null)
            {
                confirmationUI.UpdateText(GetAlreadyUsedMessageWithTime());
            }
        }
    }

    private void RecordSkipUsage()
    {
        PlayerPrefs.SetString(LastSkipDateKey, DateTime.Now.ToString("o"));
        PlayerPrefs.Save();
        UpdateSkipButtonState();
    }

    private void OnSkipButtonClicked()
    {
        if (openConfirmSE != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySE(openConfirmSE);
        }

        if (currentConfirmationInstance != null) return;

        if (!CanSkip())
        {
            ShowInfoMessage(GetAlreadyUsedMessageWithTime());
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
            string message = $"このステージをスキップしますか？\n（{skipRecoveryHours}時間に1回使用可能）";
            confirmationUI.Initialize(
                message,
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

        if (executeSkipSE != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySE(executeSkipSE);
        }

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

    [ContextMenu("Debug: Set Recovery to 1 Minute")]
    public void DebugSetRecoveryToOneMinute()
    {
        // 1分後に回復するように、LastSkipDateを逆算して設定する
        DateTime targetRecoveryTime = DateTime.Now.AddMinutes(1);
        DateTime fakeLastSkipDate = targetRecoveryTime.AddHours(-skipRecoveryHours);

        PlayerPrefs.SetString(LastSkipDateKey, fakeLastSkipDate.ToString("o"));
        PlayerPrefs.Save();
        
        if (Application.isPlaying)
        {
            UpdateSkipButtonState();
        }
        
        Debug.Log("デバッグ：スキップ回復までの残り時間を1分に設定しました。");
    }
}
