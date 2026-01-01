using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// 呪文の購入機能を備えた詳細説明パネル。
/// </summary>
public class SpellPurchaseUI : SpellDescriptionUI
{
    public static new SpellPurchaseUI Instance { get; private set; }

    [Header("購入UI要素")]
    [SerializeField, Tooltip("購入ボタン")]
    private Button purchaseButton;

    [SerializeField, Tooltip("購入ボタンの画像コンポーネント")]
    private Image purchaseButtonImage;

    [SerializeField, Tooltip("購入可能な時のスプライト")]
    private Sprite canBuySprite;

    [SerializeField, Tooltip("購入不可能な時のスプライト")]
    private Sprite cannotBuySprite;

    [SerializeField, Tooltip("金額と保有数を表示するテキスト (例: 100で購入(6))")]
    private TextMeshProUGUI costAndOwnedText;

    [SerializeField, Tooltip("コスト表示の横にあるコインの画像")]
    private Image costCoinImage;

    [Header("音響設定")]
    [SerializeField, Tooltip("購入成功時に再生するSE")]
    private AudioClip purchaseSE;

    protected override void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (purchaseButton != null)
        {
            purchaseButton.onClick.AddListener(OnPurchaseButtonClicked);
        }
    }

    public override void StartShowAnimation(SpellBase spell)
    {
        base.StartShowAnimation(spell);
        UpdatePurchaseUI();
    }

    /// <summary>
    /// 購入UIの表示内容（テキストとボタンの有効状態）を更新します。
    /// </summary>
    private void UpdatePurchaseUI()
    {
        if (currentlyDisplayedSpell == null || costAndOwnedText == null) return;

        // SpellHoldInfoManagerから保有数を取得
        SpellType type = SpellDatabase.Instance.GetSpellType(currentlyDisplayedSpell);
        int owned = SpellHoldInfoManager.Instance.GetSpellCount(type);

        int cost = GetCurrentCost(currentlyDisplayedSpell, owned);

        // テキストの更新: "金額で購入(保有数)"
        costAndOwnedText.text = $"{cost}で購入({owned})";

        bool canAfford = CurrencyManager.Instance.CurrentCurrency >= cost;

        // 購入可能かどうかの判定（所持金チェック）
        if (purchaseButton != null)
        {
            purchaseButton.interactable = canAfford;
        }

        // スプライトの切り替え
        if (purchaseButtonImage != null)
        {
            purchaseButtonImage.sprite = canAfford ? canBuySprite : cannotBuySprite;
        }

        // コイン画像の明度変更
        if (costCoinImage != null)
        {
            costCoinImage.color = canAfford ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
        }
    }

    /// <summary>
    /// 現在の保有数に基づいたコストを取得します。
    /// </summary>
    private int GetCurrentCost(SpellBase spell, int ownedCount)
    {
        if (spell.purchaseCosts == null || spell.purchaseCosts.Length == 0) return 0;

        // 保有数が配列の範囲外なら最後の要素を返す
        int index = Mathf.Clamp(ownedCount, 0, spell.purchaseCosts.Length - 1);
        return spell.purchaseCosts[index];
    }

    /// <summary>
    /// 購入ボタンがクリックされた時の処理。
    /// </summary>
    public void OnPurchaseButtonClicked()
    {
        if (currentlyDisplayedSpell == null) return;

        SpellType type = SpellDatabase.Instance.GetSpellType(currentlyDisplayedSpell);
        int owned = SpellHoldInfoManager.Instance.GetSpellCount(type);
        int cost = GetCurrentCost(currentlyDisplayedSpell, owned);

        if (CurrencyManager.Instance.SubtractCurrency(cost))
        {
            // 保有数を増やす
            SpellHoldInfoManager.Instance.IncreaseSpellCount(type);

            // SE再生
            if (SoundManager.Instance != null && purchaseSE != null)
            {
                SoundManager.Instance.PlaySE(purchaseSE);
            }

            // UI更新
            UpdatePurchaseUI();

            Debug.Log($"{currentlyDisplayedSpell.spellName} を購入しました。現在の保有数: {SpellHoldInfoManager.Instance.GetSpellCount(type)}");
        }
        else
        {
            // お金が足りない
            Debug.Log("通貨が不足しています。");
        }
    }
}