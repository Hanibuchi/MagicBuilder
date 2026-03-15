using UnityEngine;

/// <summary>
/// 各種interfaceを実装してアニメーションを再生するためのクラス
/// </summary>
public class MyCharacterController : MyObjectController, IStatusEffectReceiver
{
    [SerializeField] protected Animator animator;
    protected HPBarController hpBarController;
    // --- アニメーションのパラメータ名 ---
    protected const string PARAM_IS_IDLE = "idle";
    protected const string PARAM_IS_RUNNING = "run";
    protected const string PARAM_IS_STUNNED = "stun";
    protected const string PARAM_IS_DEAD = "die";

    protected const string PARAM_IS_FIRE_STUNNED = "FireStun";
    protected const string PARAM_IS_FROZEN = "FreezeStun";
    protected const string PARAM_IS_SLOWED = "IceSlow";

    protected const string PARAM_DAMAGE_TRIGGER = "damage";
    protected const string PARAM_ATTACK_TRIGGER = "attack"; // 汎用、または ID を直接使用

    private StatusEffectModel _statusModel;

    protected override void Awake()
    {
        base.Awake();
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
                Debug.LogError("Animator component not found on " + gameObject.name);
        }
        hpBarController = GetComponent<HPBarController>();
        _statusModel = new StatusEffectModel(this);
    }

    [SerializeField] Transform damageTextSpawnPoint;
    /// <summary>
    /// ダメージを受け取ったことを通知し、アニメーション表示処理を委譲します。
    /// </summary>
    public override void NotifyDamage(DamageType damageType, float damageValue)
    {
        base.NotifyDamage(damageType, damageValue);
        if (animator == null || !animator.enabled) return;

        Vector3 spawnPoint;
        if (damageTextSpawnPoint != null)
            spawnPoint = damageTextSpawnPoint.position;
        else
            spawnPoint = transform.position;
        DamageTextManager.Instance.ShowDamageText(damageValue, damageType, spawnPoint);

        if (!characterHealth.IsDead)
        {
            if (damageType != DamageType.Heal)
                // 例: 基本/全てのダメージで共通の"Hit"アニメーションを再生
                animator.SetTrigger(PARAM_DAMAGE_TRIGGER);
        }
    }

    public override void NotifyFireStun(float duration)
    {
        _statusModel.FireStun(duration);
    }

    public override void NotifyFreezeStun(float duration)
    {
        _statusModel.FreezeStun(duration);
    }

    public override void NotifyIceSlow(float duration)
    {
        _statusModel.IceSlow(duration);
    }

    [SerializeField] private bool playHitSoundOnDead = true;
    /// <summary>
    /// 死亡を通知し、死亡アニメーションのトリガーを設定します。
    /// </summary>
    public override void NotifyDie(bool silent = false)
    {
        base.NotifyDie(silent);
        if (!silent)
        {
            if (playHitSoundOnDead)
                PlayHitSound();

            PlayDieSound();
        }
    }

    /// <summary>
    /// キャラクターを復活させ、状態を元に戻します。
    /// </summary>
    public override void Revive()
    {
        base.Revive();

        if (animator != null && animator.enabled)
        {
            animator.SetBool(PARAM_IS_DEAD, false);
        }
    }

    public override void NotifyDieSilent()
    {
        SetHitBoxesEnabled(false);
        if (animator == null || !animator.enabled) return;
        animator.SetBool(PARAM_IS_DEAD, true);
    }

    /// <summary>
    /// HPの変化を通知し、表示処理を委譲します。（IHealthNotifierの実装）
    /// </summary>
    /// <param name="maxHP">最大HP</param>
    /// <param name="previousHP">ダメージ/回復前のHP</param>
    /// <param name="currentHP">ダメージ/回復後の現在のHP</param>
    public override void NotifyHealthChange(float maxHP, float previousHP, float currentHP)
    {
        base.NotifyHealthChange(maxHP, previousHP, currentHP);
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
        PlayFireStunSound();
        animator.SetBool(PARAM_IS_FIRE_STUNNED, true);
        Debug.Log("FireStun started");
    }

    public virtual void OnFireStunEnd()
    {
        animator.SetBool(PARAM_IS_FIRE_STUNNED, false);
        Debug.Log("FireStun ended");
    }

    public virtual void OnFreezeStunStart()
    {
        PlayFreezeStunSound();
        animator.SetBool(PARAM_IS_FROZEN, true);
    }

    public virtual void OnFreezeStunEnd()
    {
        animator.SetBool(PARAM_IS_FROZEN, false);
    }

    public virtual void OnIceSlowStart()
    {
        animator.SetBool(PARAM_IS_SLOWED, true);
    }

    public virtual void OnIceSlowEnd()
    {
        animator.SetBool(PARAM_IS_SLOWED, false);
    }

    [SerializeField] AudioClip fireStunSound;
    [SerializeField] float fireStunSoundVolume = 1.0f;

    public void PlayFireStunSound()
    {
        if (SoundManager.Instance != null && fireStunSound != null)
            SoundManager.Instance.PlaySE(fireStunSound, fireStunSoundVolume);
    }

    [SerializeField] AudioClip freezeStunSound;
    [SerializeField] float freezeStunSoundVolume = 1.0f;

    public void PlayFreezeStunSound()
    {
        if (SoundManager.Instance != null && freezeStunSound != null)
            SoundManager.Instance.PlaySE(freezeStunSound, freezeStunSoundVolume);
    }

    [SerializeField] AudioClip dieSound;
    [SerializeField] float dieSoundVolume = 1.0f;

    public void PlayDieSound()
    {
        if (SoundManager.Instance != null && dieSound != null)
            SoundManager.Instance.PlaySE(dieSound, dieSoundVolume);
    }
}