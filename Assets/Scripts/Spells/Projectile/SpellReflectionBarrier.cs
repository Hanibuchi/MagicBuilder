using UnityEngine;

/// <summary>
/// 触れた投射物（DamageSourceBaseを持つオブジェクト）の攻撃対象レイヤーを反転させるコンポーネント。
/// </summary>
public class SpellReflectionBarrier : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleReflection(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleReflection(collision.gameObject);
    }

    /// <summary>
    /// 対象のオブジェクトからDamageSourceBaseを探し、攻撃レイヤーを反転させます。
    /// </summary>
    /// <param name="go">衝突したオブジェクト</param>
    private void HandleReflection(GameObject go)
    {
        // 親方向、またはルートから DamageSourceBase を探す
        DamageSourceBase ds = go.GetComponentInParent<DamageSourceBase>();
        
        if (ds != null)
        {
            Reflect(ds);
        }
    }

    private void Reflect(DamageSourceBase ds)
    {
        SpellLayer currentLayer = ds.GetSpellLayer();
        
        if (currentLayer == SpellLayer.Attack_Ally)
        {
            ds.SetLayer(SpellLayer.Attack_Enemy);
            // Debug.Log($"[SpellReflectionBarrier] {ds.gameObject.name} を敵の攻撃に変更しました。");
        }
        else if (currentLayer == SpellLayer.Attack_Enemy)
        {
            ds.SetLayer(SpellLayer.Attack_Ally);
            // Debug.Log($"[SpellReflectionBarrier] {ds.gameObject.name} を味方の攻撃に変更しました。");
        }
    }
}
