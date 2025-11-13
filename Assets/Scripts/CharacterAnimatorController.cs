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
}