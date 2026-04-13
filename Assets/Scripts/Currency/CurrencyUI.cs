using UnityEngine;
using TMPro;

/// <summary>
/// 通貨の表示を管理するUIクラス。
/// </summary>
public class CurrencyUI : MonoBehaviour
{
    public static CurrencyUI Instance { get; private set; }

    [Header("UI要素")]
    [SerializeField] GameObject currencyFrame;
    [SerializeField, Tooltip("通貨を表示するテキスト")]
    private TextMeshProUGUI currencyText;
    [SerializeField, Tooltip("通貨の変動を表示するテキスト")]
    private TextMeshProUGUI currencyVariationText;

    [SerializeField, Tooltip("UIのアニメーター")]
    private Animator animator;

    [Header("演出設定")]
    [SerializeField, Tooltip("数値が更新されるまでにかかる時間")]
    private float updateDuration = 0.5f;
    [SerializeField, Tooltip("変動テキストのフェード時間")]
    private float fadeDuration = 0.2f;
    [SerializeField, Tooltip("変動テキストの表示時間")]
    private float displayDuration = 1.0f;

    [Header("音響設定")]
    [SerializeField, Tooltip("数値が変化するたびに再生するSE")]
    private AudioClip countSE;

    private int currentDisplayedValue;
    private Coroutine updateCoroutine;
    private Coroutine variationCoroutine;

    private void Awake()
    {
        // シングルトンの設定
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning($"Duplicate CurrencyUI found on {gameObject.name}. Destroying.");
            Destroy(gameObject);
        }
        currencyFrame.SetActive(false);
        if (currencyVariationText != null)
        {
            Color c = currencyVariationText.color;
            c.a = 0;
            currencyVariationText.color = c;
        }
    }

    private void Start()
    {
        // 初期値の設定
        if (CurrencyManager.Instance != null)
        {
            SetCurrencyText(CurrencyManager.Instance.CurrentCurrency, true);
        }
    }

    /// <summary>
    /// 通貨の表示テキストを更新します。
    /// </summary>
    /// <param name="targetAmount">最終的な金額</param>
    /// <param name="instant">演出をスキップして即座に更新するかどうか</param>
    public void SetCurrencyText(int targetAmount, bool instant = false)
    {
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
            updateCoroutine = null;
        }

        if (instant)
        {
            currentDisplayedValue = targetAmount;
            UpdateText(currentDisplayedValue);
        }
        else
        {
            updateCoroutine = StartCoroutine(UpdateCurrencyRoutine(targetAmount));
        }
    }

    /// <summary>
    /// 数値を徐々に更新するコルーチン。
    /// </summary>
    private System.Collections.IEnumerator UpdateCurrencyRoutine(int targetAmount)
    {
        int startValue = currentDisplayedValue;
        int diff = targetAmount - startValue;
        float elapsed = 0f;

        while (elapsed < updateDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / updateDuration;

            int newValue = (int)Mathf.Lerp(startValue, targetAmount, t);

            // 値が変化した時だけ更新とSE再生
            if (newValue != currentDisplayedValue)
            {
                currentDisplayedValue = newValue;
                UpdateText(currentDisplayedValue);
                PlayCountSE();
            }
            yield return null;
        }

        if (currentDisplayedValue != targetAmount)
        {
            currentDisplayedValue = targetAmount;
            UpdateText(currentDisplayedValue);
            PlayCountSE();
        }
        
        ShowVariation(diff);
        
        updateCoroutine = null;
    }

    private void ShowVariation(int amount)
    {
        if (currencyVariationText == null || amount == 0) return;

        if (variationCoroutine != null) StopCoroutine(variationCoroutine);
        variationCoroutine = StartCoroutine(VariationRoutine(amount));
    }

    private System.Collections.IEnumerator VariationRoutine(int amount)
    {
        currencyVariationText.text = amount > 0 ? $"+{amount}" : amount.ToString();
        // optionally, you could change color based on positive or negative:
        // currencyVariationText.color = amount > 0 ? Color.green : Color.red;

        Color c = currencyVariationText.color;
        
        // Fade in
        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            c.a = t / fadeDuration;
            currencyVariationText.color = c;
            yield return null;
        }
        c.a = 1;
        currencyVariationText.color = c;

        // Display 
        yield return new WaitForSecondsRealtime(displayDuration);

        // Fade out
        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            c.a = 1f - (t / fadeDuration);
            currencyVariationText.color = c;
            yield return null;
        }
        c.a = 0;
        currencyVariationText.color = c;

        variationCoroutine = null;
    }

    private void UpdateText(int value)
    {
        if (currencyText != null)
        {
            currencyText.text = value.ToString();
        }
    }

    private void PlayCountSE()
    {
        if (SoundManager.Instance != null && countSE != null)
        {
            SoundManager.Instance.PlaySE(countSE);
        }
    }

    private bool show = false;
    public bool IsShowing => show;

    /// <summary>
    /// UIを表示します。
    /// </summary>
    public void Show()
    {
        if (show) return;
        show = true;
        currencyFrame.SetActive(true);
        if (animator != null)
        {
            animator.SetTrigger("Show");
        }
    }

    /// <summary>
    /// UIを非表示にします。
    /// </summary>
    public void Hide()
    {
        if (!show) return;
        show = false;
        if (animator != null)
        {
            currencyFrame.SetActive(true);
            animator.SetTrigger("Hide");
        }
        else
        {
            currencyFrame.SetActive(false);
        }
    }

    public int test_amount = 50;
    public void Test()
    {
        SetCurrencyText(test_amount);
    }

    public void Test2()
    {
        if (test_amount > 0)
        {
            CurrencyController.Instance.AddCurrency(test_amount);
        }
        else
        {
            CurrencyController.Instance.UseCurrency(-test_amount);
        }
    }
}
