using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshProUGUIを使うために必要

public class SettingsUI : MonoBehaviour
{
    // --- BGM設定要素 ---
    [Header("BGM Settings")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private TextMeshProUGUI bgmPercentageText;

    // --- SE設定要素 ---
    [Header("SE Settings")]
    [SerializeField] private Slider seSlider;
    [SerializeField] private TextMeshProUGUI sePercentageText;

    // --- UI/アニメーション要素 ---
    [Header("Panel Animation & SFX")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Animator panelAnimator;
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button xButton; // X（旧Twitter）を開くボタン
    [SerializeField] private Button buyCoffeeButton; // 作者にコーヒーをおごるボタン
    [SerializeField] private Button clearDataButton; // データリセットボタン
    [SerializeField] private string authorXUrl = "https://x.com/your_account_name"; // 作者のXのURL
    [SerializeField] private AudioClip openSFX;
    [SerializeField] private AudioClip closeSFX;
    [SerializeField] private AudioClip volumeChangeSFX;
    [SerializeField] private AudioClip clearDataConfirmSFX; // データ削除時のSE
    [SerializeField] private AudioClip clearDataCancelSFX; // データ削除キャンセル時のSE
    [SerializeField] private PurchaseMessageUI purchaseMessageUIPrefab; // 購入UIのプレハブ
    [SerializeField] private ConfirmationUI confirmationUIPrefab; // 確認UIのプレハブ

    private PurchaseMessageUI buyCoffeePurchaseUI;

    private bool isPanelOpen = false;

    // Animatorのトリガー名
    private const string OPEN_TRIGGER = "Open";
    private const string CLOSE_TRIGGER = "Close";

    void Start()
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogError("SoundManagerのインスタンスが見つかりません。");
            return;
        }

        // --- 初期設定時に、SoundManagerから初期音量を取得してスライダーを初期化 ---
        InitializeSlider(bgmSlider, bgmPercentageText, bgmSlider.maxValue, "BGM");
        InitializeSlider(seSlider, sePercentageText, seSlider.maxValue, "SE");

        // スライダーの値変更イベントにメソッドを登録
        bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        seSlider.onValueChanged.AddListener(OnSEVolumeChanged);

        // ボタンのクリックイベントにメソッドを登録
        openButton.onClick.AddListener(OnOpenButtonClicked);
        closeButton.onClick.AddListener(Close);
        
        if (xButton != null)
        {
            xButton.onClick.AddListener(OnXButtonClicked);
        }

        if (buyCoffeeButton != null)
        {
            buyCoffeeButton.onClick.AddListener(OnBuyCoffeeButtonClicked);
        }

        if (clearDataButton != null)
        {
            clearDataButton.onClick.AddListener(OnClearDataButtonClicked);
        }

        // 初期状態では設定パネルを非表示にしておく
        settingsPanel.SetActive(false);

        if (IAPManager.Instance != null)
        {
            IAPManager.Instance.OnCoffeePurchased += ShowThankYouUI;
        }
    }

    private void OnDestroy()
    {
        if (IAPManager.Instance != null)
        {
            IAPManager.Instance.OnCoffeePurchased -= ShowThankYouUI;
        }
    }

    /// <summary>
    /// スライダーの初期設定と表示の更新を行います。
    /// </summary>
    private void InitializeSlider(Slider slider, TextMeshProUGUI text, float maxValue, string type)
    {
        // スライダーの設定確認
        slider.minValue = 0f;
        // slider.maxValue は Inspectorで設定された値を使用
        slider.wholeNumbers = true;

        // SoundManagerから現在の音量（0.0〜1.0）を取得
        float initialVolume0to1 = 0f;
        if (type == "BGM")
        {
            initialVolume0to1 = SoundManager.Instance.GetBGMVolume0to1();
        }
        else if (type == "SE")
        {
            initialVolume0to1 = SoundManager.Instance.GetSEVolume0to1();
        }

        // 0-1の音量をスライダーの最大値に基づいてスライダーの値に変換
        // 例: maxValue=100, initialVolume0to1=0.5 -> initialSliderValue=50
        float initialSliderValue = initialVolume0to1 * maxValue;

        // スライダーに初期値を設定
        slider.value = initialSliderValue;

        // パーセンテージ表示を更新
        UpdatePercentageText(text, initialSliderValue, maxValue);
    }

    /// <summary>
    /// BGMスライダーの値が変更されたときに呼び出されます。
    /// </summary>
    private void OnBGMVolumeChanged(float sliderValue)
    {
        float maxValue = bgmSlider.maxValue;
        float volume0to1 = sliderValue / maxValue;

        SoundManager.Instance.SetBGMVolume0to1(volume0to1);
        UpdatePercentageText(bgmPercentageText, sliderValue, maxValue);
        PlaySE(volumeChangeSFX);
    }

    /// <summary>
    /// SEスライダーの値が変更されたときに呼び出されます。
    /// </summary>
    private void OnSEVolumeChanged(float sliderValue)
    {
        float maxValue = seSlider.maxValue;
        float volume0to1 = sliderValue / maxValue;

        SoundManager.Instance.SetSEVolume0to1(volume0to1);
        UpdatePercentageText(sePercentageText, sliderValue, maxValue);

        // 値が変更されたときSEを鳴らす
        PlaySE(volumeChangeSFX);
    }

    /// <summary>
    /// パーセンテージ表示テキストを更新します。
    /// </summary>
    private void UpdatePercentageText(TextMeshProUGUI text, float currentValue, float maxValue)
    {
        // 最大値で割ることで、0-1の割合を取得し、100倍してパーセンテージを計算
        int percentage = Mathf.RoundToInt((currentValue / maxValue) * 100f);
        text.text = percentage.ToString() + "%";
    }


    /// <summary>
    /// 設定パネルを開くボタンが押されたときに呼び出されます。
    /// </summary>
    private void OnOpenButtonClicked()
    {
        if (isPanelOpen) return;

        settingsPanel.SetActive(true);
        if (panelAnimator != null)
        {
            panelAnimator.SetTrigger(OPEN_TRIGGER);
        }

        PlaySE(openSFX);

        openButton.interactable = false;
        closeButton.interactable = true;
        isPanelOpen = true;
        TimeStopManager.Instance.RequestTimeStop(this, 0f);
    }

    /// <summary>
    /// 設定パネルを閉じるボタンが押されたとき、または外部から閉じるときに呼び出されます。
    /// </summary>
    public void Close()
    {
        if (!isPanelOpen) return;

        if (panelAnimator != null)
        {
            panelAnimator.SetTrigger(CLOSE_TRIGGER);
        }

        PlaySE(closeSFX);

        openButton.interactable = true;
        closeButton.interactable = false;
        isPanelOpen = false;
        TimeStopManager.Instance.ReleaseTimeStop(this);
    }

    /// <summary>
    /// X（旧Twitter）ボタンが押されたときに呼び出されます。
    /// 作者のプロフィールページを開きます。
    /// </summary>
    private void OnXButtonClicked()
    {
        // PlaySE(openSFX); // クリック音（任意のSE）
        Application.OpenURL(authorXUrl);
    }

    /// <summary>
    /// コーヒー購入ボタンが押されたときに呼び出されます。
    /// PurchaseMessageUIを生成して表示します。
    /// </summary>
    private void OnBuyCoffeeButtonClicked()
    {
        if (buyCoffeePurchaseUI == null)
        {
            if (purchaseMessageUIPrefab != null)
            {
                // UIキャンバスの子として生成するか、最前面に表示されるようにする
                buyCoffeePurchaseUI = Instantiate(purchaseMessageUIPrefab);
            }
        }

        if (buyCoffeePurchaseUI != null)
        {
            string price = "¥300"; // 仮の金額（デフォルト）
            if (IAPManager.Instance != null)
            {
                string storePrice = IAPManager.Instance.GetProductPriceString(IAPManager.BUY_COFFEE);
                if (!string.IsNullOrEmpty(storePrice))
                {
                    price = storePrice;
                }
            }

            string description = "作者にコーヒーをおごりますか？\n<size=20>※今後の開発の励みになります！";
            buyCoffeePurchaseUI.Init(price, description, () =>
            {
                Debug.Log("[SettingsUI] コーヒー購入処理を開始します。");
                if (IAPManager.Instance != null)
                {
                    IAPManager.Instance.BuyCoffee();
                }
            });
            buyCoffeePurchaseUI.Show();
        }
        else
        {
            Debug.LogWarning("[SettingsUI] purchaseMessageUIPrefabがアサインされていません。インスペクターから設定してください。");
        }
    }

    /// <summary>
    /// コーヒーの購入が完了したときに呼び出される「ありがとうございます」UIの表示処理です。
    /// </summary>
    private void ShowThankYouUI()
    {
        if (buyCoffeePurchaseUI == null)
        {
            if (purchaseMessageUIPrefab != null)
            {
                buyCoffeePurchaseUI = Instantiate(purchaseMessageUIPrefab);
            }
        }

        if (buyCoffeePurchaseUI != null)
        {
            string titleOrText = "OK";
            string description = "コーヒーありがとうございます！";
            buyCoffeePurchaseUI.Init(titleOrText, description, null);
            buyCoffeePurchaseUI.Show();
        }
    }

    /// <summary>
    /// 全データ削除ボタンが押されたときに呼び出されます。
    /// </summary>
    private void OnClearDataButtonClicked()
    {
        if (confirmationUIPrefab == null)
        {
            Debug.LogError("[SettingsUI] confirmationUIPrefab が設定されていません。");
            return;
        }

        // 確認UIを生成 (最前面に表示するため、Canvasや適当な親を指定するかルートに置く)
        // ここでは自身の子として生成します。Canvasが親にある前提です。
        ConfirmationUI confirmationUI = Instantiate(confirmationUIPrefab, transform.parent);
        
        string message = "すべてのセーブデータを削除しますか？\nこの操作は取り消せません。";
        confirmationUI.Initialize(message, () =>
        {
            // Yesが押されたときの処理
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ClearPlayerPrefs();
            }
            PlaySE(clearDataConfirmSFX);
        },
        () => 
        {
            // Noが押されたときは何もしない
            PlaySE(clearDataCancelSFX);
        });
    }

    /// <summary>
    /// 閉じるアニメーションの終了時にAnimatorから呼び出されるメソッド
    /// </summary>
    public void OnCloseAnimationFinished()
    {
        if (!isPanelOpen)
        {
            settingsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// SoundManagerを使用してSEを再生します。
    /// </summary>
    private void PlaySE(AudioClip clip)
    {
        if (SoundManager.Instance != null && clip != null)
            SoundManager.Instance.PlaySE(clip);
    }
}