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
    [SerializeField] private AudioClip openSFX;
    [SerializeField] private AudioClip closeSFX;
    [SerializeField] private AudioClip volumeChangeSFX;

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

        // 初期状態では設定パネルを非表示にしておく
        settingsPanel.SetActive(false);
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

    private float timeScale = 1f;

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