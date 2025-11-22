using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 複数のSliderを使用してクールタイムの進行を表示・管理するUIコントローラー。
/// 1つのSliderの最大値を超えたクールタイムを表現できる。
/// </summary>
public class CooldownUIController : MonoBehaviour, ICooldownChangeListener
{
    // === インスペクタから設定するプロパティ ===

    // スライダー1本あたりの最大クールタイム値
    [Tooltip("スライダー1本が表現できる最大クールタイム値。")]
    public float sliderMaxValue = 60f;

    // クールタイム表示に使用するSliderのリスト
    [Tooltip("クールタイム表示に使用するSliderのリスト。")]
    public List<Slider> cooldownSliders;

    // === プライベート変数 ===

    private int activeSliderCount = 0; // 現在アクティブになっているSliderの数

    // ----------------------------------------------------

    void Start()
    {
        // 1. WizardCooldownManagerを探して登録
        // ここでは仮にシングルトンとして存在するとしています。
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.SetListener(this);
        }
        else
        {
            Debug.LogError("WizardCooldownManagerが見つかりません。");
        }

        InitializeSliders();
    }

    /// <summary>
    /// Sliderの初期設定を行う
    /// </summary>
    private void InitializeSliders()
    {
        // 全てのSliderを非アクティブにする（最初の一つを除く）
        for (int i = 0; i < cooldownSliders.Count; i++)
        {
            Slider slider = cooldownSliders[i];

            // Sliderの最大値・最小値を設定
            slider.minValue = 0f;
            slider.maxValue = sliderMaxValue;
            slider.value = 0f; // 初期値

            // 最初のSliderのみをActiveにする
            if (i == 0)
            {
                slider.gameObject.SetActive(true);
                activeSliderCount = 1;
            }
            else
            {
                slider.gameObject.SetActive(false);
            }

            // Handleを非表示にする（初期状態では全て）
            SetSliderHandleActive(slider, false);
        }

        // 一番後のSlider (初期状態では最初の一つ) のHandleをActiveにする
        if (cooldownSliders.Count > 0)
        {
            SetSliderHandleActive(cooldownSliders[activeSliderCount - 1], true);
        }
    }

    /// <summary>
    /// クールタイムの合計値に基づきUIを更新する
    /// </summary>
    /// <param name="totalCooldownValue">現在の合計クールタイム</param>
    public void UpdateCooldownUI(float totalCooldownValue)
    {
        // 必要なアクティブSliderの数を計算
        // 例: totalCooldownValue=150, sliderMaxValue=100 の場合、必要な数は 2
        int requiredSliders = Mathf.CeilToInt(totalCooldownValue / sliderMaxValue);

        // クールタイムが0の場合は最初のSliderだけをActiveにする
        if (totalCooldownValue <= 0.01f) // 微小な値も考慮
        {
            requiredSliders = 1;
            totalCooldownValue = 0f;
        }

        // アクティブなSliderの数を更新
        UpdateActiveSliders(requiredSliders);

        // 各Sliderの値を設定
        for (int i = 0; i < activeSliderCount; i++)
        {
            Slider slider = cooldownSliders[i];

            if (i < activeSliderCount - 1)
            {
                // 現在のSliderが最後のSliderでない場合（フルになっている）
                slider.value = sliderMaxValue;
            }
            else
            {
                if (activeSliderCount < requiredSliders)
                {
                    slider.value = sliderMaxValue;
                    continue;
                }
                // 最後のSliderの場合（超えた値が表示される部分）
                float remainder = totalCooldownValue % sliderMaxValue;
                if (remainder == 0f && totalCooldownValue > 0f)
                {
                    // ちょうど割り切れた場合（最後のスライダーがフル）
                    slider.value = sliderMaxValue;
                }
                else
                {
                    slider.value = remainder;
                }
            }
        }
    }

    /// <summary>
    /// アクティブなSliderの数を更新し、Handleの表示を制御する
    /// </summary>
    private void UpdateActiveSliders(int newActiveCount)
    {
        // 設定ミス防止
        newActiveCount = Mathf.Clamp(newActiveCount, 1, cooldownSliders.Count);

        if (newActiveCount == activeSliderCount)
        {
            // 必要な数が変わらなければ処理をスキップ
            return;
        }

        // Active数を増やす/減らす
        for (int i = 0; i < cooldownSliders.Count; i++)
        {
            Slider slider = cooldownSliders[i];
            bool shouldBeActive = i < newActiveCount;
            slider.gameObject.SetActive(shouldBeActive);

            // 新しく非ActiveになるSliderのHandleを非表示にする
            if (i == newActiveCount - 1)
                SetSliderHandleActive(slider, true);
            else
                SetSliderHandleActive(slider, false);
        }

        activeSliderCount = newActiveCount;
    }

    /// <summary>
    /// SliderのHandleをActive/Non-Activeにするヘルパー関数
    /// </summary>
    private void SetSliderHandleActive(Slider slider, bool isActive)
    {
        // Handleの子オブジェクトを探してActive/Non-Activeを設定
        Transform handleTransform = slider.handleRect;
        if (handleTransform != null)
        {
            handleTransform.gameObject.SetActive(isActive);
        }
    }

    // --- デバッグ用 ---
    [ContextMenu("Test Cooldown 50")]
    public void Test50() { UpdateCooldownUI(50f); }

    [ContextMenu("Test Cooldown 60 (Full 1)")]
    public void Test60() { UpdateCooldownUI(60f); }
    [ContextMenu("Test Cooldown 180 (Full 1)")]
    public void Test180() { UpdateCooldownUI(180f); }

    [ContextMenu("Test Cooldown 150")]
    public void Test150() { UpdateCooldownUI(150f); }

    [ContextMenu("Test Cooldown 10000")]
    public void Test10000() { UpdateCooldownUI(10000f); }

    [ContextMenu("Test Cooldown 0")]
    public void Test0() { UpdateCooldownUI(0f); }

    public void OnCooldownChanged(float remainingCooldown)
    {
        UpdateCooldownUI(remainingCooldown);
    }
}