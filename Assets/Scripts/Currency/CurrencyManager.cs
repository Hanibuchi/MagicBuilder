using UnityEngine;

namespace MagicBuilder.Currency
{
    /// <summary>
    /// 通貨（コイン）を管理するクラス。
    /// シングルトンで、必要に応じて自動的に生成されます。
    /// </summary>
    public class CurrencyManager : MonoBehaviour
    {
        private static CurrencyManager _instance;

        public static CurrencyManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    // なければ新しく作成
                    GameObject go = new GameObject("CurrencyManager");
                    _instance = go.AddComponent<CurrencyManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private const string CURRENCY_SAVE_KEY = "Player_Currency_Amount";

        [SerializeField, ReadOnly]
        private int currentCurrency = 0;

        /// <summary>
        /// 現在の所持額を取得します。
        /// </summary>
        public int CurrentCurrency => currentCurrency;

        private void Awake()
        {
            if (_instance = null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
            if (_instance == this)
                LoadCurrency();
        }

        /// <summary>
        /// 通貨を追加します。
        /// </summary>
        /// <param name="amount">追加する量</param>
        public void AddCurrency(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("CurrencyManager: Use SubtractCurrency to remove funds.");
                return;
            }

            currentCurrency += amount;
            SaveCurrency();
        }

        /// <summary>
        /// 通貨を消費します。
        /// </summary>
        /// <param name="amount">消費する量</param>
        /// <returns>消費に成功した（残高が足りていた）場合はtrue</returns>
        public bool SubtractCurrency(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("CurrencyManager: Amount must be positive.");
                return false;
            }

            if (currentCurrency >= amount)
            {
                currentCurrency -= amount;
                SaveCurrency();
                return true;
            }

            Debug.Log("CurrencyManager: Not enough currency.");
            return false;
        }

        /// <summary>
        /// 所持額を保存します。
        /// </summary>
        private void SaveCurrency()
        {
            PlayerPrefs.SetInt(CURRENCY_SAVE_KEY, currentCurrency);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 所持額をロードします。
        /// </summary>
        private void LoadCurrency()
        {
            currentCurrency = PlayerPrefs.GetInt(CURRENCY_SAVE_KEY, 0);
        }
    }

    // インスペクタで値を表示するための属性（必要に応じて）
    public class ReadOnlyAttribute : PropertyAttribute { }
}
