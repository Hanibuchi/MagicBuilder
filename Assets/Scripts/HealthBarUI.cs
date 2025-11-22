using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HPBarController : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("HPバーとして使用するSliderコンポーネント")]
    public Slider hpSlider;

    [Tooltip("Sliderコンポーネントを含むAnimatorコンポーネント")]
    public Animator hpBarAnimator;

    // アニメーションのトリガー名
    private const string SHOW_TRIGGER = "Show";
    private const string HIDE_TRIGGER = "Hide";

    // HPアニメーションに使用するコルーチン
    private Coroutine _hpUpdateCoroutine;

    /// <summary>
    /// HPバーの表示を更新します。HPの増減に応じてスライダーを徐々に変化させ、
    /// HPバーの表示/非表示アニメーションのトリガーを呼び出します。
    /// </summary>
    /// <param name="maxHP">HPの最大値</param>
    /// <param name="previousHP">ダメージ/回復前のHP</param>
    /// <param name="currentHP">ダメージ/回復後の現在のHP</param>
    public void UpdateHPBar(float maxHP, float previousHP, float currentHP)
    {
        // 入力のバリデーション
        if (hpSlider == null || hpBarAnimator == null)
        {
            Debug.LogError("HP SliderまたはAnimatorがインスペクタで設定されていません。");
            return;
        }

        // 1. HPバーの表示/非表示制御
        if (previousHP >= maxHP && currentHP < maxHP)
        {
            // HPが最大値からダメージを受けた場合（透明状態から表示へ）
            // アニメーションの「Show」トリガーを呼び出し、HPバーを徐々に表出させる
            hpBarAnimator.SetTrigger(SHOW_TRIGGER);
        }
        else if (currentHP == 0 || (previousHP < maxHP && currentHP >= maxHP))
        {
            // HPが最大値に回復した場合（表示状態から透明へ）
            // アニメーションの「Hide」トリガーを呼び出し、HPバーを徐々に透明にする
            hpBarAnimator.SetTrigger(HIDE_TRIGGER);
        }

        // 2. HP値の更新アニメーション

        // 既に実行中のHP更新アニメーションがあれば停止
        if (_hpUpdateCoroutine != null)
        {
            StopCoroutine(_hpUpdateCoroutine);
        }

        // 新しいHP更新アニメーションを開始
        _hpUpdateCoroutine = StartCoroutine(AnimateHPChange(maxHP, previousHP, currentHP));
    }

    /// <summary>
    /// HPバーのスライダー値を徐々に変化させるコルーチン。
    /// </summary>
    private IEnumerator AnimateHPChange(float maxHP, float previousHP, float currentHP)
    {
        // スライダーの最大値を設定
        hpSlider.maxValue = maxHP;

        // アニメーションの開始値と終了値
        float startValue = previousHP;
        float endValue = currentHP;

        // アニメーションの時間（任意で調整してください）
        float duration = 0.3f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // 経過時間に応じた補間値（0.0～1.0）
            float t = elapsedTime / duration;

            // Sliderの値を線形補間（Lerp）で更新
            hpSlider.value = Mathf.Lerp(startValue, endValue, t);

            yield return null; // 1フレーム待機
        }

        // 終了時に正確な最終値を設定
        hpSlider.value = endValue;

        _hpUpdateCoroutine = null;
    }

    // 最初にHPバーを非表示（透明）にするための設定
    private void Start()
    {
        // 初期状態では、HPバー（Slider）の値は0に設定され、Animatorで透明化されていることを前提とします。
        if (hpSlider != null)
        {
            hpSlider.value = 0f;
        }

        // Animatorが初期状態で非表示状態にあることを確認してください。
        // （例：最初は透明度が0になっているか、SetActive(false)になっているなど）
    }
}