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
                    go.transform.root.Rotate(0, 0, 180);

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
        
        if (currentLayer == SpellLayer.Attack_Ally)
        {
            ds.SetLayer(SpellLayer.Attack_Enemy);
            // Debug.Log($"[SpellReflectionBarrier] {ds.gameObject.name} を敵の攻撃に変更しました。");
            return true;
        }
        else if (currentLayer == SpellLayer.Attack_Enemy)
        {
            ds.SetLayer(SpellLayer.Attack_Ally);
            // Debug.Log($"[SpellReflectionBarrier] {ds.gameObject.name} を味方の攻撃に変更しました。");
            return true;
        }

        return false;
    }
}
