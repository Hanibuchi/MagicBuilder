using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// 持ち込み呪文の最大容量を拡張するためのUIクラス。
/// SpellPurchaseUIと似た構成ですが、特定の呪文ではなく「容量拡張」という概念を扱います。
/// </summary>
public class CapacityPurchaseUI : MonoBehaviour, IPointerClickHandler
{
    public static CapacityPurchaseUI Instance { get; private set; }

    [Header("UI要素への参照")]
    [SerializeField, Tooltip("詳細説明パネル全体のルートGameObject")]
    private GameObject detailPanelRoot;

    [SerializeField, Tooltip("詳細説明パネルのアニメーターコンポーネント")]
    private Animator panelAnimator;

    [Header("購入UI要素")]
    [SerializeField, Tooltip("購入ボタン")]
    private Button purchaseButton;

    [SerializeField, Tooltip("購入ボタンの画像コンポーネント")]
    private Image purchaseButtonImage;

    [SerializeField, Tooltip("購入可能な時のスプライト")]
    private Sprite canBuySprite;

    [SerializeField, Tooltip("購入不可能な時のスプライト")]
    private Sprite cannotBuySprite;

    [SerializeField, Tooltip("金額と現在の容量を表示するテキスト (例: 500で購入(1))")]
    private TextMeshProUGUI costAndCapacityText;

    [SerializeField, Tooltip("コスト表示の横にあるコインの画像")]
    private Image costCoinImage;

    [Header("音響設定")]
    [SerializeField, Tooltip("購入成功時に再生するSE")]
    private AudioClip purchaseSE;

    private bool isHiding = false;
    private bool isShow = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        // 初期状態では非表示
        detailPanelRoot.SetActive(false);
    }

    private void Start()
    {
        if (purchaseButton != null)
        {
            purchaseButton.onClick.AddListener(OnPurchaseButtonClicked);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isHiding) return;
        StartHideAnimation();
    }

    /// <summary>
    /// パネルを表示します。
    /// </summary>
    public void StartShowAnimation()
    {
        UpdateUI();
        isHiding = false;
        detailPanelRoot.SetActive(true);
        if (panelAnimator != null)
        {
            panelAnimator.SetTrigger("Show");
        }
    }

    /// <summary>
    /// アニメーションから呼ばれるメソッド。
    /// </summary>
    public void ShowDexcription()
    {
        if (isShow) return;
        isShow = true;
        TimeStopManager.Instance.RequestTimeStop(this, 0f);
    }

    private void StartHideAnimation()
    {
        if (isHiding || !detailPanelRoot.activeSelf) return;
        if (!isShow) return;
        isShow = false;
        TimeStopManager.Instance.ReleaseTimeStop(this);

        isHiding = true;
        if (panelAnimator != null)
        {
            panelAnimator.SetTrigger("Hide");
        }
        else
        {
            HideDescription();
        }
    }

    /// <summary>
    /// パネルを非表示にします。アニメーションから呼ばれる。
    /// </summary>
    public void HideDescription()
    {
        detailPanelRoot.SetActive(false);
        isHiding = false;
    }

    /// <summary>
    /// UIの表示内容を更新します。
    /// </summary>
    public void UpdateUI()
    {
        if (costAndCapacityText == null) return;

        // EquippedSpellManagerから現在の容量と拡張コストを取得
        int currentCapacity = EquippedSpellManager.Instance.GetMaxCapacity();
        int cost = EquippedSpellManager.Instance.GetCapacityUpgradeCost();

        // テキストの更新: "金額で購入(現在の容量)"
        costAndCapacityText.text = $"{cost}で購入({currentCapacity})";

        // 所持金チェック
        bool canAfford = CurrencyManager.Instance.CurrentCurrency >= cost;

        // 購入可能かどうかの判定
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
    /// 購入ボタンがクリックされた時の処理。
    /// </summary>
    public void OnPurchaseButtonClicked()
    {
        int cost = EquippedSpellManager.Instance.GetCapacityUpgradeCost();

        // 通貨を消費して容量を増やす
        if (CurrencyController.Instance.UseCurrency(cost))
        {
            // 容量を増やす
            EquippedSpellManager.Instance.IncreaseCapacity(1);

            // SE再生
            if (SoundManager.Instance != null && purchaseSE != null)
            {
                SoundManager.Instance.PlaySE(purchaseSE);
            }

            // UI更新
            UpdateUI();

            Debug.Log($"持ち込み容量を拡張しました。現在の最大容量: {EquippedSpellManager.Instance.GetMaxCapacity()}");
        }
        else
        {
            // お金が足りない
            Debug.Log("通貨が不足しています。");
        }
    }
}
