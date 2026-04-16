using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 汎用的な（広告非表示や各種アイテムなどの）購入UIを制御するクラス
/// </summary>
public class PurchaseMessageUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button purchaseButton;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    private Action onPurchaseRequested;
    private Coroutine activeCoroutine;
    private Coroutine typewriterCoroutine;
    private string fullDescription;

    /// <summary>
    /// UIの初期設定を行います。
    /// </summary>
    /// <param name="price">表示する価格（例: "200"）</param>
    /// <param name="description">購入物の説明テキスト</param>
    /// <param name="onPurchase">購入ボタンが押された時の処理</param>
    public void Init(string price, string description, Action onPurchase)
    {
        this.onPurchaseRequested = onPurchase;
        this.fullDescription = description;

        // 価格表示をそのまま使用（すでに通貨記号が含まれている想定）
        if (priceText != null)
        {
            priceText.text = price;
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

        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
        typewriterCoroutine = StartCoroutine(TypewriterRoutine());
    }

    private IEnumerator TypewriterRoutine()
    {
        if (descriptionText == null) yield break;

        descriptionText.text = fullDescription;
        descriptionText.maxVisibleCharacters = 0;
        descriptionText.ForceMeshUpdate();

        int totalVisibleCharacters = descriptionText.textInfo.characterCount;
        for (int i = 0; i <= totalVisibleCharacters; i++)
        {
            descriptionText.maxVisibleCharacters = i;
            yield return null;
        }
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
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        if (isInteractive)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            Destroy(gameObject);
        }

        activeCoroutine = null;
    }

    #region Test Methods

    [ContextMenu("Test/Show Purchase UI")]
    /// <summary>
    /// インスペクターから動作確認するためのテストメソッド
    /// </summary>
    public void Test_ShowUI()
    {
        Init("¥200", "広告を非表示にしますか？\n<size=20>※広告報酬は今まで通り得られます。", () => Debug.Log("Test: Purchase Requested!"));
        Show();
    }

    #endregion
}
