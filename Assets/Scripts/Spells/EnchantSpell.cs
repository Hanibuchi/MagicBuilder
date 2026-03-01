using UnityEngine;

[CreateAssetMenu(fileName = "EnchantSpell", menuName = "Wand System/Enchant Spell")]
public class EnchantSpell : ExampleSpell
{
    protected override void ApplyTrajectoryModifier(GameObject trajectoryObj, SpellContext context)
    {
        // 補助線（軌道プレハブ）には修飾子を適用しない
    }
}
