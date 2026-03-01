using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SelfHarmSpell", menuName = "Wand System/SelfHarmSpell")]
public class SelfHarmSpell : SpellBase
{
    public override void FireSpell(
        List<SpellBase> wandSpells, int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        // 詠唱者自身にあたるようにレイヤーを入れ替える
        if (context.layer == SpellLayer.Attack_Ally)
        {
            context.layer = SpellLayer.Attack_Enemy;
        }
        else if (context.layer == SpellLayer.Attack_Enemy)
        {
            context.layer = SpellLayer.Attack_Ally;
        }

        FireSpellForNextSpells(GetNextSpellOffsets(wandSpells, currentSpellIndex), wandSpells, currentSpellIndex, rotationZ, strength, context);
    }

    public override void DisplayAimingLine(
        List<SpellBase> wandSpells, int currentSpellIndex,
        float rotationZ, float strength, SpellContext context, bool clearLine = false)
    {
        // 予測線でもレイヤーの変更を反映させる
        if (context.layer == SpellLayer.Attack_Ally)
        {
            context.layer = SpellLayer.Attack_Enemy;
        }
        else if (context.layer == SpellLayer.Attack_Enemy)
        {
            context.layer = SpellLayer.Attack_Ally;
        }

        base.DisplayAimingLine(wandSpells, currentSpellIndex, rotationZ, strength, context, clearLine);
    }

    int[] nextSpellOffsets = { 1 };
    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells,
        int currentSpellIndex)
    {
        return nextSpellOffsets;
    }
}
