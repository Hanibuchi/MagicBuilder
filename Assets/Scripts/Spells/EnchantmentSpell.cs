using UnityEngine;

/// <summary>
/// 付与呪文用の呪文。ExampleSpell を継承し、軌道予測において重力を無視します。
/// </summary>
[CreateAssetMenu(fileName = "EnchantmentSpell", menuName = "Wand System/Enchantment Spell")]
public class EnchantmentSpell : ExampleSpell
{
    protected override float GetGravityMagnitude(SpellContext context)
    {
        // 重力計算は行うが、context.gravityModifier の影響だけを無視する。
        float gravityMagnitude = Physics2D.gravity.magnitude;
        if (projectilePrefab != null && projectilePrefab.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            gravityMagnitude *= rb.gravityScale;
        }
        return gravityMagnitude;
    }
}
