using UnityEngine;

/// <summary>
/// 各種interfaceを実装してアニメーションを再生するためのクラス
/// </summary>
[RequireComponent(typeof(Animator))] // Animatorコンポーネントが必須であることを保証
public class CharacterAnimatorController : MonoBehaviour, IDamageNotifier, IDieNotifier
{
    private Animator animator;

    // --- アニメーションのトリガー名 ---
    private const string HIT_TRIGGER = "damage";          // ダメージを受けた時のアニメーション
    private const string DIE_TRIGGER = "die";          // 死亡時のアニメーション
    private const string FIRE_TRIGGER = "attack";
    private const string FIRE_DEGREE_PARAM = "attack_degree";
    private const string AIM_TRIGGER = "aim";
    private const string AIM_DEGREE_PARAM = "aim_degree";

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on " + gameObject.name);
        }

    }

    /// <summary>
    /// ダメージを受け取ったことを通知し、アニメーション表示処理を委譲します。
    /// </summary>
    public void NotifyDamage(DamageType damageType, float damageValue, Vector2 collisionPoint)
    {
        if (animator == null || !animator.enabled) return;

        // 例: 基本/全てのダメージで共通の"Hit"アニメーションを再生
        animator.SetTrigger(HIT_TRIGGER);
    }

    /// <summary>
    /// 死亡を通知し、死亡アニメーションのトリガーを設定します。
    /// </summary>
    public void NotifyDie()
    {
        if (animator == null || !animator.enabled) return;

        animator.SetTrigger(DIE_TRIGGER);
    }

    /// <summary>
    /// 魔法発射を通知し、攻撃アニメーションのトリガーを設定します。
    /// </summary>
    public void NotifyFire(float angle)
    {
        if (animator == null || !animator.enabled) return;

        // 1. 角度を0度から90度の範囲にクランプ（制限）
        // 例: -45 -> 0, 125 -> 90, 45 -> 45
        float clampedAngle = Mathf.Clamp(angle, 0f, 90f);

        // 2. クランプされた角度を0.0から1.0の値に正規化
        // Mathf.InverseLerp(最小値, 最大値, 現在の値) は、最小値〜最大値の範囲内で
        // 現在の値がどの位置にあるかを0.0〜1.0で返します。
        // 例: 0 -> 0.0, 90 -> 1.0, 45 -> 0.5
        float normalizedValue = Mathf.InverseLerp(0f, 90f, clampedAngle);
        // 3. Animatorのfloatパラメーターに設定
        animator.SetFloat(FIRE_DEGREE_PARAM, normalizedValue);
        animator.SetTrigger(FIRE_TRIGGER);
    }

    /// <param name="angle">狙いの角度 (度)</param>
    public void SetAimRotation(float angle, bool reset = false)
    {
        if (animator == null || !animator.enabled) return;
        if (reset)
            return;

        // 1. 角度を0度から90度の範囲にクランプ（制限）
        // 例: -45 -> 0, 125 -> 90, 45 -> 45
        float clampedAngle = Mathf.Clamp(angle, 0f, 90f);

        // 2. クランプされた角度を0.0から1.0の値に正規化
        // Mathf.InverseLerp(最小値, 最大値, 現在の値) は、最小値〜最大値の範囲内で
        // 現在の値がどの位置にあるかを0.0〜1.0で返します。
        // 例: 0 -> 0.0, 90 -> 1.0, 45 -> 0.5
        float normalizedValue = Mathf.InverseLerp(0f, 90f, clampedAngle);
        // 3. Animatorのfloatパラメーターに設定
        animator.SetFloat(AIM_DEGREE_PARAM, normalizedValue);
        animator.SetTrigger(AIM_TRIGGER);
    }
}