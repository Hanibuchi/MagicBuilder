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

    // --- IDamageSourceの実装 ---
    public void Initialize(float strength, SpellContext spellContext)
    {
        var components = GetComponents<ISpellProjectileInitListener>();
        foreach (var component in components)
            component?.Initialize(strength, spellContext);

        Launch();
        damageData = spellContext.damage;
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
        return damageData;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy();
    }

    public override void Destroy()
    {
        animator?.SetTrigger(DESTROY_TRIGGER);
        var components = GetComponents<ISpellProjectileDestroyListener>();
        foreach (var component in components)
            component?.Destroy();
    }

    /// <summary>
    /// 即座にオブジェクトを破壊する。アニメーションから呼び出す想定。
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
    /// 再生するヒット音を設定し、再生します。Animationから呼び出されることを想定。
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