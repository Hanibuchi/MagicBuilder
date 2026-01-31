using UnityEngine;

/// <summary>
/// 衝突したオブジェクトに対して SpellContext.ProjectileModifier を実行する投射物。
/// </summary>
public class ImpactModifierProjectile : SpellProjectileDamageSource
{
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        // 衝突した相手に対してモディファイアを適用
        if (cachedContext != null && cachedContext.ProjectileModifier != null)
        {
            cachedContext.ProjectileModifier.Invoke(collision.gameObject);
        }

        // 基本クラスの処理（衝突時のプレハブ生成や破壊など）を呼び出す
        base.OnCollisionEnter2D(collision);
    }
}
