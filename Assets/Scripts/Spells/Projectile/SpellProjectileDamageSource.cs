using UnityEngine;

/// <summary>
/// 呪文用。衝突時にダメージ情報をCharacterHealthコンポーネントに提供するコンポーネント。
/// IDamageSourceインターフェースを実装しています。
/// </summary>
public class SpellProjectileDamageSource : DamageSourceBase
{
    // --- 外部参照用変数 (public/SerializeField) ---

    [Header("ダメージ設定")]
    [Tooltip("このダメージ源が与える詳細なダメージ情報")]
    // publicかつ[System.Serializable]な構造体であるDamageを直接SerializeFieldとして定義することで、
    // インスペクタと外部スクリプトの両方から編集可能になります。
    Damage damageData;
    const string DESTROY_TRIGGER = "Destroy";

    [SerializeField] Animator animator;

    [Header("衝突時生成")]
    [Tooltip("衝突時に生成するプレハブ。nullなら生成しない")]
    [SerializeField] GameObject spawnOnCollisionPrefab;

    [Header("自動破壊設定")]
    [SerializeField] bool autoDestroy = true;

    protected float cachedStrength;
    protected SpellContext cachedContext;

    // --- IDamageSourceの実実装 ---
    public virtual void Initialize(float strength, SpellContext spellContext)
    {
        this.cachedStrength = strength;
        this.cachedContext = spellContext;

        if (spellContext != null)
        {
            SetLayer(spellContext.layer);

            if (spellContext.bounceCount > 0)
            {
                AddPierceCount(spellContext.bounceCount);
            }
        }

        // 呪文による放射物の修正（軌道変更やサイズ変更など）を適用
        ApplyProjectileModifier(spellContext);

        var components = GetComponents<ISpellProjectileInitListener>();
        foreach (var component in components)
            component?.Initialize(strength, spellContext);

        Launch();
        damageData = spellContext?.damage ?? new Damage();

        if (spellContext != null && !spellContext.IsPermanent())
        {
            Invoke(nameof(Destroy), spellContext.duration);
        }
    }

    protected virtual void ApplyProjectileModifier(SpellContext spellContext)
    {
        spellContext?.ProjectileModifier?.Invoke(gameObject);
    }

    public override void SetLayer(SpellLayer newLayer)
    {
        base.SetLayer(newLayer);
        if (cachedContext != null)
        {
            cachedContext.layer = newLayer;
        }
    }

    protected override void Awake()
    {
        Launch();
        base.Awake();
    }

    [SerializeField] bool playLaunchSound = true;
    bool isLaunched = false;
    void Launch()
    {
        if (isLaunched) return;
        isLaunched = true;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (playLaunchSound)
            PlayLaunchSound();

        if (enableImpulse)
            GenerateImpulse();
    }

    /// <summary>
    /// このダメージ源が持つダメージ情報を取得します。
    /// </summary>
    /// <returns>設定されたDamage構造体。</returns>
    public override Damage GetDamage()
    {
        // トリガーイベントの実行
        var triggerListeners = GetComponents<ISpellProjectileTriggerListener>();
        foreach (var listener in triggerListeners)
        {
            listener?.OnTrigger();
        }

        return damageData;
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        SpawnCollisionPrefab(collision.contacts.Length > 0 ? collision.contacts[0].point : (Vector2)transform.position);

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (GetDamageSourceType() == DamageSourceType.SingleHit)
                TryConsumePierceCount();
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Ground"))
            SpawnCollisionPrefab(transform.position);
    }

    private void SpawnCollisionPrefab(Vector2 spawnPos)
    {
        if (spawnOnCollisionPrefab == null) return;

        // 指定された位置にプレハブを生成
        GameObject spawned = Instantiate(spawnOnCollisionPrefab, spawnPos, Quaternion.identity);

        // 生成されたオブジェクトを初期化
        if (spawned.TryGetComponent<SpellProjectileDamageSource>(out var ds))
        {
            ds.Initialize(cachedStrength, cachedContext);
        }
    }

    public override void Destroy()
    {
        if (animator != null)
        {
            animator.SetTrigger(DESTROY_TRIGGER);
        }
        var components = GetComponents<ISpellProjectileDestroyListener>();
        foreach (var component in components)
            component?.Destroy();

        if (autoDestroy)
        {
            DestroyImd();
            PlayDestroySound();
        }
    }

    /// <summary>
    /// 即座にオブジェクトを破壊する。アニメーションから呼び出すこともできる。
    /// </summary>
    public void DestroyImd()
    {
        Destroy(gameObject);
    }

    [SerializeField] AudioClip launchSound;
    [SerializeField] float launchSoundVolume = 1.0f;
    [SerializeField] AudioClip destroySound;
    [SerializeField] float destroySoundVolume = 1.0f;

    /// <summary>
    /// 再生する発射音を設定し、再生します。
    /// </summary>
    public void PlayLaunchSound()
    {
        if (SoundManager.Instance != null && launchSound != null)
            SoundManager.Instance.PlaySE(launchSound, launchSoundVolume);
    }
    /// <summary>
    /// 再生するヒット音を設定し、再生します。Animationから呼び出されることもできる。
    /// </summary>
    public void PlayDestroySound()
    {
        if (SoundManager.Instance != null && destroySound != null)
            SoundManager.Instance.PlaySE(destroySound, destroySoundVolume);
    }
}

public interface ISpellProjectileInitListener
{
    void Initialize(float strength, SpellContext spellContext);
}

public interface ISpellProjectileDestroyListener
{
    void Destroy();
}

public interface ISpellProjectileTriggerListener
{
    void OnTrigger();
}