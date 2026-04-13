using UnityEngine;

/// <summary>
/// 触れた投射物（DamageSourceBaseを持つオブジェクト）の攻撃対象レイヤーを反転させるコンポーネント。
/// </summary>
public class SpellReflectionBarrier : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip reflectSE;

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleReflection(other.gameObject, other.isTrigger);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleReflection(collision.gameObject, collision.collider.isTrigger);
    }

    /// <summary>
    /// 対象のオブジェクトからDamageSourceBaseを探し、攻撃レイヤーを反転させます。
    /// </summary>
    /// <param name="go">衝突したオブジェクト</param>
    private void HandleReflection(GameObject go, bool isTrigger)
    {
        // 親方向、またはルートから DamageSourceBase を探す
        DamageSourceBase ds = go.GetComponentInParent<DamageSourceBase>();
        
        if (ds != null)
        {
            bool reflected = Reflect(ds);

            if (reflected)
            {
                // SEの再生
                if (reflectSE != null && SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySE(reflectSE);
                }

                // isTriggerがtrueの場合に方向と速度を反転
                if (isTrigger)
                {
                    // 向いている方向を反転
                    ds.transform.Rotate(0, 0, 180);

                    // 速度を反転
                    Rigidbody2D rb = go.GetComponentInParent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.linearVelocity = -rb.linearVelocity;
                    }
                }
            }
        }
    }

    private bool Reflect(DamageSourceBase ds)
    {
        SpellLayer currentLayer = ds.GetSpellLayer();
        bool reflected = false;
        
        if (currentLayer == SpellLayer.Attack_Ally)
        {
            ds.SetLayer(SpellLayer.Attack_Enemy);
            reflected = true;
        }
        else if (currentLayer == SpellLayer.Attack_Enemy)
        {
            ds.SetLayer(SpellLayer.Attack_Ally);
            reflected = true;
        }

        if (reflected)
        {
            // クリックトリガーなど、遅延発射用に内部でコンテキストを保持しているコンポーネントのレイヤーも反転する
            var clickModifiers = ds.GetComponentsInChildren<ClickTriggerProjectileModifier>();
            foreach (var mod in clickModifiers)
            {
                if (mod.context != null)
                {
                    if (mod.context.layer == SpellLayer.Attack_Ally)
                    {
                        mod.context.layer = SpellLayer.Attack_Enemy;
                    }
                    else if (mod.context.layer == SpellLayer.Attack_Enemy)
                    {
                        mod.context.layer = SpellLayer.Attack_Ally;
                    }
                }
            }

            // 何かにヒットした際に発動するトリガー呪文も同様に反転する
            var triggerModifiers = ds.GetComponentsInChildren<TriggerProjectileModifier>();
            foreach (var mod in triggerModifiers)
            {
                if (mod.context != null)
                {
                    if (mod.context.layer == SpellLayer.Attack_Ally)
                    {
                        mod.context.layer = SpellLayer.Attack_Enemy;
                    }
                    else if (mod.context.layer == SpellLayer.Attack_Enemy)
                    {
                        mod.context.layer = SpellLayer.Attack_Ally;
                    }
                }
            }
        }

        return reflected;
    }
}
