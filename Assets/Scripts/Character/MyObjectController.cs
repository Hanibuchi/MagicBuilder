using UnityEngine;

/// <summary>
/// オブジェクトの基底コントローラークラス。
/// CharacterHealthとHitBoxの管理を行います。
/// </summary>
public class MyObjectController : MonoBehaviour, IDamageNotifier, IDieNotifier, IHealthNotifier
{
    protected CharacterHealth characterHealth;

    [Tooltip("死亡時に無効化するColliderがアタッチされているオブジェクト（攻撃判定を消すために使用）")]
    [SerializeField] protected GameObject hitBoxObject;

    [Header("オブジェクト設定")]
    [Tooltip("このオブジェクトが放射物（Projectile）であるかどうか")]
    [SerializeField] protected bool isProjectile = false;

    /// <summary>
    /// このオブジェクトが放射物（Projectile）であるかどうかを取得します。
    /// </summary>
    public bool IsProjectile => isProjectile;

    protected virtual void Awake()
    {
        if (TryGetComponent<CharacterHealth>(out var health))
        {
            characterHealth = health;
        }
        else
        {
            Debug.LogError("CharacterHealth component is required on " + gameObject.name);
        }
    }

    /// <summary>
    /// ダメージを受け取ったことを通知します。
    /// サブクラスでアニメーションなどを実装してください。
    /// </summary>
    public virtual void NotifyDamage(DamageType damageType, float damageValue)
    {
        if (!characterHealth.IsDead)
        {
            if (damageType == DamageType.Heal)
            {
                PlayHealSound();
            }
            else
            {
                PlayHitSound();
            }
        }
    }

    [SerializeField] AudioClip hitSound;
    [SerializeField] float hitSoundVolume = 1.0f;

    public void PlayHitSound()
    {
        if (SoundManager.Instance != null && hitSound != null)
            SoundManager.Instance.PlaySE(hitSound, hitSoundVolume);
    }

    [SerializeField] AudioClip healSound;
    [SerializeField] float healSoundVolume = 1.0f;

    public void PlayHealSound()
    {
        if (SoundManager.Instance != null && healSound != null)
            SoundManager.Instance.PlaySE(healSound, healSoundVolume);
    }

    /// <summary>
    /// 死亡を通知します。
    /// </summary>
    public virtual void NotifyDie(bool silent = false)
    {
        NotifyDieSilent();
    }

    /// <summary>
    /// 死亡時の静的な処理（HitBox無効化など）を行います。
    /// </summary>
    public virtual void NotifyDieSilent()
    {
        SetHitBoxesEnabled(false);
        Destroy(transform.root.gameObject);
    }

    /// <summary>
    /// キャラクターを復活させ、状態を元に戻します。
    /// </summary>
    public virtual void Revive()
    {
        if (characterHealth != null)
        {
            characterHealth.Revive();
        }

        SetHitBoxesEnabled(true);
    }

    /// <summary>
    /// HPの変化を通知します。
    /// </summary>
    public virtual void NotifyHealthChange(float maxHP, float previousHP, float currentHP)
    {
    }

    /// <summary>
    /// ヒット判定用のCollider2Dの配列を返します。
    /// </summary>
    public Collider2D[] GetHitBoxes()
    {
        if (hitBoxObject == null) return new Collider2D[0];
        return hitBoxObject.GetComponents<Collider2D>();
    }

    protected void SetHitBoxesEnabled(bool enabled)
    {
        if (hitBoxObject != null)
        {
            Collider2D[] colliders = GetHitBoxes();
            foreach (var col in colliders)
            {
                col.enabled = enabled;
            }
        }
    }

    public virtual void NotifyFireStun(float duration)
    {
    }

    public virtual void NotifyFreezeStun(float duration)
    {
    }

    public virtual void NotifyIceSlow(float duration)
    {
    }
}
