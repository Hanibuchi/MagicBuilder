using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 広告非表示（課金）購入UIを制御するクラス
/// </summary>
public class RemoveAdsPurchaseUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Button purchaseButton;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    private Action onPurchaseRequested;
    private Coroutine activeCoroutine;

    /// <summary>
    /// UIの初期設定を行います。
    /// </summary>
    /// <param name="price">表示する価格（例: "200"）</param>
    /// <param name="onPurchase">購入ボタンが押された時の処理</param>
    public void Init(string price, Action onPurchase)
    {
        this.onPurchaseRequested = onPurchase;
        
        // 価格表示を「¥価格」の形式に設定
        if (priceText != null)
        {
            priceText.text = $"¥{price}";
        }

        purchaseButton.onClick.RemoveAllListeners();
        purchaseButton.onClick.AddListener(() =>
        {
            onPurchaseRequested?.Invoke();
            Hide();
        });

        // 初期状態は非表示
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// UIを表示します。
    /// </summary>
    public void Show()
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(FadeRoutine(1f, true));
    }

    /// <summary>
    /// UIを非表示にします。
    /// </summary>
    public void Hide()
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(FadeRoutine(0f, false));
    }

    private IEnumerator FadeRoutine(float targetAlpha, bool isInteractive)
    {
        if (!isInteractive)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        if (isInteractive)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        activeCoroutine = null;
    }

    #region Test Methods

    /// <summary>
    /// インスペクターから動作確認するためのテストメソッド
    /// </summary>
    public void Test_ShowUI()
    {
        Init("200", () => Debug.Log("Test: Purchase Requested!"));
        Show();
    }

    #endregion
}
