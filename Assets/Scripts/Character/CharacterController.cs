using UnityEngine;

/// <summary>
/// 各種interfaceを実装してアニメーションを再生するためのクラス
/// </summary>
public class MyCharacterController : MonoBehaviour, IDamageNotifier, IDieNotifier, IHealthNotifier, IStatusEffectReceiver
{
    [SerializeField] protected Animator animator;
    protected HPBarController hpBarController;
    // --- アニメーションのトリガー名 ---
    protected const string HIT_TRIGGER = "damage";          // ダメージを受けた時のアニメーション
    protected const string DIE_TRIGGER = "die";          // 死亡時のアニメーション
    protected const string FIRE_TRIGGER = "attack";
    protected const string FIRE_STUN_TRIGGER = "fireStun";
    protected const string FIRE_STUN_END_TRIGGER = "fireStunEnd";
    protected const string FREEZE_STUN_TRIGGER = "freezeStun";
    protected const string FREEZE_STUN_END_TRIGGER = "freezeStunEnd";
    protected const string ICE_SLOW_TRIGGER = "iceSlow";
    protected const string ICE_SLOW_END_TRIGGER = "iceSlowEnd";

    private StatusEffectModel _statusModel;

    protected CharacterHealth characterHealth;
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

        if (TryGetComponent<CharacterHealth>(out var health))
        {
            characterHealth = health;
        }
        else
        {
            Debug.LogError("CharacterHealth component is required on " + gameObject.name);
        }
    }

    [SerializeField] Transform damageTextSpawnPoint;
    /// <summary>
    /// ダメージを受け取ったことを通知し、アニメーション表示処理を委譲します。
    /// </summary>
    public void NotifyDamage(DamageType damageType, float damageValue)
    {
        if (animator == null || !animator.enabled || characterHealth.IsDead) return;

        // 例: 基本/全てのダメージで共通の"Hit"アニメーションを再生
        animator.SetTrigger(HIT_TRIGGER);
        PlayHitSound();

        Vector3 spawnPoint;
        if (damageTextSpawnPoint != null)
            spawnPoint = damageTextSpawnPoint.position;
        else
            spawnPoint = transform.position;
        DamageTextManager.Instance.ShowDamageText(damageValue, damageType, spawnPoint);
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

    [SerializeField] private bool playHitSoundOnDead = true;
    /// <summary>
    /// 死亡を通知し、死亡アニメーションのトリガーを設定します。
    /// </summary>
    public virtual void NotifyDie()
    {
        // if (gameObject.TryGetComponent<Collider2D>(out Collider2D collider))
        // collider.enabled = false;
        if (animator == null || !animator.enabled) return;
        animator.SetTrigger(DIE_TRIGGER);
        if (playHitSoundOnDead)
            PlayHitSound();

        PlayDieSound();
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
        PlayFireStunSound();
        animator.SetTrigger(FIRE_STUN_TRIGGER);
        Debug.Log("FireStun started");
    }

    public virtual void OnFireStunEnd()
    {
        animator.SetTrigger(FIRE_STUN_END_TRIGGER);
        Debug.Log("FireStun ended");
    }

    public virtual void OnFreezeStunStart()
    {
        PlayFreezeStunSound();
        animator.SetTrigger(FREEZE_STUN_TRIGGER);
    }

    public virtual void OnFreezeStunEnd()
    {
        animator.SetTrigger(FREEZE_STUN_END_TRIGGER);
    }

    public virtual void OnIceSlowStart()
    {
        animator.SetTrigger(ICE_SLOW_TRIGGER);
    }

    public virtual void OnIceSlowEnd()
    {
        animator.SetTrigger(ICE_SLOW_END_TRIGGER);
    }


    [SerializeField] AudioClip hitSound;
    [SerializeField] float hitSoundVolume = 1.0f;

    public void PlayHitSound()
    {
        if (SoundManager.Instance != null && hitSound != null)
            SoundManager.Instance.PlaySE(hitSound, hitSoundVolume);
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