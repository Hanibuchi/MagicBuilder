using UnityEngine;

/// <summary>
/// 通貨に関する一連の処理（データ更新、UI更新、演出再生）を統括するコントローラークラス。
/// シングルトンで、必要に応じて自動的に生成されます。
/// </summary>
public class CurrencyController : MonoBehaviour
{
    private static CurrencyController _instance;

    public static CurrencyController Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("CurrencyController");
                _instance = go.AddComponent<CurrencyController>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    [SerializeField, Tooltip("通貨追加演出開始からUIの数値が更新されるまでの待ち時間")]
    private float uiUpdateDelay = 1.5f;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 通貨を追加し、演出とUIを更新します。
    /// </summary>
    /// <param name="amount">追加する量</param>
    public void AddCurrency(int amount)
    {
        // データの更新
        CurrencyManager.Instance.AddCurrency(amount);

        // 演出の再生
        if (CoinCollectionEffect.Instance != null)
        {
            CoinCollectionEffect.Instance.PlayEffect(amount);
        }

        // UIの更新（演出に合わせて遅延させる）
        StartCoroutine(DelayedUIUpdate());
    }

    private System.Collections.IEnumerator DelayedUIUpdate()
    {
        CurrencyUI.Instance.Show();
        yield return new WaitForSecondsRealtime(uiUpdateDelay);

        if (CurrencyUI.Instance != null)
        {
            CurrencyUI.Instance.SetCurrencyText(CurrencyManager.Instance.CurrentCurrency);
        }
    }

    /// <summary>
    /// 通貨を使用し、成功した場合はUIを更新します。
    /// </summary>
    /// <param name="amount">使用する量</param>
    /// <returns>使用に成功した場合はtrue</returns>
    public bool UseCurrency(int amount)
    {
        // データの更新（消費チェック含む）
        bool success = CurrencyManager.Instance.SubtractCurrency(amount);

        if (success)
        {
            // UIの更新
            if (CurrencyUI.Instance != null)
            {
                CurrencyUI.Instance.Show();
                CurrencyUI.Instance.SetCurrencyText(CurrencyManager.Instance.CurrentCurrency);
            }
        }

        return success;
    }
}
