using UnityEngine;

/// <summary>
/// 敵や地形にぶつかった際にくっついて離れず、クリックされるまでその場に留まる投射物。
/// </summary>
public class GumballProjectile : SpellProjectileDamageSource, IClickTriggerFireListener
{
    private Rigidbody2D rb;
    private bool isStuck = false;

    [Header("Gumball設定")]
    [SerializeField] private AudioClip stickSound;
    [SerializeField] private float stickSoundVolume = 1.0f;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// クリックトリガーが発動した際に呼ばれる。ガムボールは発射時に自身を破壊する。
    /// </summary>
    public void OnFire()
    {
        Destroy();
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (isStuck) return;

        HandleStick(collision.transform);

        // 基本クラスの衝突処理（エフェクト生成など）を実行
        base.OnCollisionEnter2D(collision);
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (isStuck) return;

        // Triggerの場合は、Groundレイヤーなら無視する
        if (other.gameObject.layer != LayerMask.NameToLayer("Ground"))
            HandleStick(other.transform);

        // 基本クラスの衝突処理を実行
        base.OnTriggerEnter2D(other);
    }

    private void HandleStick(Transform target)
    {
        // 何かに衝突したら、その物理挙動を止めて親子関係を設定する（くっつく動作）
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // 衝突相手の子になることで追従する
        transform.SetParent(target);
        isStuck = true;

        // Collider2Dを非アクティブにして、以降の衝突を無効化する
        if (TryGetComponent<Collider2D>(out var col))
        {
            col.enabled = false;
        }

        // くっついた時の音を再生
        if (SoundManager.Instance != null && stickSound != null)
        {
            SoundManager.Instance.PlaySE(stickSound, stickSoundVolume);
        }
    }
}
