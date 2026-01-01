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

    [SerializeField, Tooltip("UIのアニメーター")]
    private Animator animator;

    [Header("演出設定")]
    [SerializeField, Tooltip("数値が更新されるまでにかかる時間")]
    private float updateDuration = 0.5f;

    [Header("音響設定")]
    [SerializeField, Tooltip("数値が変化するたびに再生するSE")]
    private AudioClip countSE;

    private int currentDisplayedValue;
    private Coroutine updateCoroutine;

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
        updateCoroutine = null;
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

    bool show = false;

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
        currencyFrame.SetActive(true);
        if (animator != null)
        {
            animator.SetTrigger("Hide");
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
