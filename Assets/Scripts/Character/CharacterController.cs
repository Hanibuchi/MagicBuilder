using UnityEngine;

/// <summary>
/// 各種interfaceを実装してアニメーションを再生するためのクラス
/// </summary>
public class CharacterController : MonoBehaviour, IDamageNotifier, IDieNotifier, IHealthNotifier, IStatusEffectReceiver
{
    [SerializeField] protected Animator animator;
    protected HPBarController hpBarController;
    // --- アニメーションのトリガー名 ---
    protected const string HIT_TRIGGER = "damage";          // ダメージを受けた時のアニメーション
    protected const string DIE_TRIGGER = "die";          // 死亡時のアニメーション
    protected const string FIRE_TRIGGER = "attack";

    private StatusEffectModel _statusModel;
    protected virtual void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
                Debug.LogError("Animator component not found on " + gameObject.name);
        }
        hpBarController = GetComponent<HPBarController>();
        _statusModel = new StatusEffectModel(this);
    }

    /// <summary>
    /// ダメージを受け取ったことを通知し、アニメーション表示処理を委譲します。
    /// </summary>
    public void NotifyDamage(DamageType damageType, float damageValue)
    {
        if (animator == null || !animator.enabled) return;

        // 例: 基本/全てのダメージで共通の"Hit"アニメーションを再生
        animator.SetTrigger(HIT_TRIGGER);

        DamageTextManager.Instance.ShowDamageText(damageValue, damageType, transform.position);
    }

    public void NotifyFireStun(float duration)
    {
        _statusModel.FireStun(duration);
    }

    public void NotifyFreezeStun(float duration)
    {
        _statusModel.FreezeStun(duration);
    }

    public void NotifyIceSlow(float duration)
    {
        _statusModel.IceSlow(duration);
    }

    /// <summary>
    /// 死亡を通知し、死亡アニメーションのトリガーを設定します。
    /// </summary>
    public void NotifyDie()
    {
        // if (gameObject.TryGetComponent<Collider2D>(out Collider2D collider))
        // collider.enabled = false;
        if (animator == null || !animator.enabled) return;
        animator.SetTrigger(DIE_TRIGGER);
    }

    /// <summary>
    /// HPの変化を通知し、表示処理を委譲します。（IHealthNotifierの実装）
    /// </summary>
    /// <param name="maxHP">最大HP</param>
    /// <param name="previousHP">ダメージ/回復前のHP</param>
    /// <param name="currentHP">ダメージ/回復後の現在のHP</param>
    public void NotifyHealthChange(float maxHP, float previousHP, float currentHP)
    {
        if (hpBarController != null)
        {
            // HPバーの表示更新を委譲
            hpBarController.UpdateHPBar(maxHP, previousHP, currentHP);
        }
    }

    protected virtual void Update()
    {
        _statusModel.Update(Time.deltaTime);
    }

    public virtual void OnFireStunStart()
    {
        Debug.Log("FireStun started");
    }

    public virtual void OnFireStunEnd()
    {
        Debug.Log("FireStun ended");
    }

    public void OnFreezeStunStart()
    {
    }

    public virtual void OnFreezeStunEnd()
    {
    }

    public virtual void OnIceSlowStart()
    {
    }

    public virtual void OnIceSlowEnd()
    {
    }
}