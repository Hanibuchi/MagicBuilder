using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PenetrationSpell", menuName = "Wand System/PenetrationSpell")]
public class PenetrationSpell : SpellBase
{
    public override void FireSpell(List<SpellBase> wandSpells, int currentSpellIndex, float rotationZ, float strength, SpellContext context)
    {
        AddPenetrationModifier(context);
        FireSpellForNextSpells(GetNextSpellOffsets(wandSpells, currentSpellIndex), wandSpells, currentSpellIndex, rotationZ, strength, context);
    }

    public override void DisplayAimingLine(List<SpellBase> wandSpells, int currentSpellIndex, float rotationZ, float strength, SpellContext context, bool clearLine = false)
    {
        DisplayAimingLineForNextSpells(GetNextSpellOffsets(wandSpells, currentSpellIndex), wandSpells, currentSpellIndex, rotationZ, strength, context, clearLine);
    }

    private void AddPenetrationModifier(SpellContext context)
    {
        context.ProjectileModifier += (GameObject obj) =>
        {
            if (obj != null)
            {
                if (obj.TryGetComponent<Collider2D>(out var col))
                {
                    col.isTrigger = true;
                }
                // 衝突相手からCharacterControllerを取得
                if (obj.TryGetComponent<MyCharacterController>(out var controller))
                {
                    // ヒットボックスを取得してすべてTriggerにする
                    foreach (var collider in controller.GetHitBoxes())
                    {
                        if (collider != null)
                        {
                            collider.isTrigger = true;
                        }
                    }
                }
            }
        };
    }

    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells, int currentSpellIndex)
    {
        // 1つの呪文（次のまとまり）を対象とする修飾呪文
        return new int[] { 1 };
    }
}