using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 広告視聴によるコンティニューUIを制御するクラス
/// </summary>
public class ContinueAdUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image timerFillImage;
    [SerializeField] private Button adButton;

    [Header("Settings")]
    [SerializeField] private float countdownTime = 5f;
    [SerializeField] private float fadeDuration = 0.5f;

    private Action onAdRequested;
    private Action onTimeExpired;
    private Coroutine activeCoroutine;
    private bool isInteracting;

    /// <summary>
    /// UIの初期設定を行います。
    /// </summary>
    /// <param name="onAdRequested">広告視聴ボタンが押された時の処理</param>
    /// <param name="onTimeExpired">時間切れで閉じた時の処理</param>
    public void Init(Action onAdRequested, Action onTimeExpired)
    {
        this.onAdRequested = onAdRequested;
        this.onTimeExpired = onTimeExpired;

        adButton.onClick.RemoveAllListeners();
        adButton.onClick.AddListener(HandleAdButtonClick);

        // 初期状態は非表示
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// コンティニューUIを表示し、カウントダウンを開始します。
    /// </summary>
    public void Show()
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(ShowAndTimerRoutine());
    }

    private IEnumerator ShowAndTimerRoutine()
    {
        isInteracting = true;
        timerFillImage.fillAmount = 1f;

        // フェードイン
        yield return FadeCanvasGroup(1f);
        
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // カウントダウン
        float elapsed = 0f;
        while (elapsed < countdownTime)
        {
            elapsed += Time.deltaTime;
            timerFillImage.fillAmount = 1f - (elapsed / countdownTime);
            yield return null;
        }

        timerFillImage.fillAmount = 0f;
        
        // 時間切れのため、callbackを実行して閉じる
        HandleTimeout();
    }

    private void HandleAdButtonClick()
    {
        if (!isInteracting) return;
        
        // 重複防止
        isInteracting = false;
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        
        StartCoroutine(CloseRoutine(onAdRequested));
    }

    private void HandleTimeout()
    {
        if (!isInteracting) return;

        isInteracting = false;
        StartCoroutine(CloseRoutine(onTimeExpired));
    }

    private IEnumerator CloseRoutine(Action callback)
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // フェードアウト
        yield return FadeCanvasGroup(0f);

        callback?.Invoke();
        activeCoroutine = null;
    }

    private IEnumerator FadeCanvasGroup(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    #region Test Methods

    /// <summary>
    /// インスペクターのボタン等から動作確認するためのテストメソッド
    /// </summary>
    public void Test_StartUI()
    {
        Init(
            () => Debug.Log("Test: Ad Requested!"),
            () => Debug.Log("Test: Time Expired...")
        );
        Show();
    }

    #endregion
}
