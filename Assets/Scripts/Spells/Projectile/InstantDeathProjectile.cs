using UnityEngine;

/// <summary>
/// 衝突した対象のCharacterHealthを取得し、即座にKill()を実行する特殊な放射物クラス。
/// </summary>
public class InstantDeathProjectile : SpellProjectileDamageSource
{
    [Header("即死設定")]
    [SerializeField] private AudioClip killSound;
    [SerializeField] private float killSoundVolume = 1.0f;

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        TryInstantDeath(collision.gameObject);
        // 基本クラスの衝突処理（エフェクト生成や破壊など）を実行
        base.OnCollisionEnter2D(collision);
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        TryInstantDeath(other.gameObject);
        // 基本クラスの衝突処理を実行
        base.OnTriggerEnter2D(other);
    }

    /// <summary>
    /// 対象からCharacterHealthコンポーネントを探し、存在すればKill()を呼びます。
    /// </summary>
    private void TryInstantDeath(GameObject target)
    {
        // 衝突したオブジェクト自身、またはその親からCharacterHealthを取得
        CharacterHealth health = target.GetComponentInParent<CharacterHealth>();
        
        if (health == null)
        {
            health = target.GetComponentInChildren<CharacterHealth>();
        }

        if (health != null)
        {
            health.Kill();
            
            if (SoundManager.Instance != null && killSound != null)
            {
                SoundManager.Instance.PlaySE(killSound, killSoundVolume);
            }
        }
    }
}
