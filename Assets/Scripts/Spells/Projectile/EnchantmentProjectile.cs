using UnityEngine;

/// <summary>
/// 衝突したオブジェクトに対して SpellContext.ProjectileModifier を実行する投射物。
/// </summary>
public class EnchantmentProjectile : SpellProjectileDamageSource
{
    [Header("エンチャント設定")]
    [SerializeField] AudioClip enchantSound;
    [SerializeField] float enchantSoundVolume = 1.0f;

    protected override void ApplyProjectileModifier(SpellContext spellContext)
    {
        // 自身にはモディファイアを適用しない
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        ApplyEnchantment(collision.gameObject);
        // 基本クラスの処理（衝突時のプレハブ生成や破壊など）を呼び出す
        base.OnCollisionEnter2D(collision);
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        ApplyEnchantment(other.gameObject);
        // 基本クラスの処理（衝突時のプレハブ生成や破壊など）を呼び出す
        base.OnTriggerEnter2D(other);
    }

    private void ApplyEnchantment(GameObject hitObject)
    {
        if (hitObject == null) return;

        // 衝突した相手に対してモディファイアを適用（地面以外の場合）
        if (hitObject.layer != LayerMask.NameToLayer("Ground"))
        {
            if (cachedContext != null && cachedContext.ProjectileModifier != null)
            {
                cachedContext.ProjectileModifier.Invoke(hitObject.transform.root.gameObject);
                PlayEnchantSound();
            }
        }
    }

    private void PlayEnchantSound()
    {
        if (SoundManager.Instance != null && enchantSound != null)
        {
            SoundManager.Instance.PlaySE(enchantSound, enchantSoundVolume);
        }
    }
}
