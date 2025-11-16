using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DownwardSpell", menuName = "Wand System/Downward Spell")]
public class Downward : SpellBase
{
    public override void DisplayAimingLine(
        List<SpellBase> wandSpells, int currentSpellIndex, float rotationZ,
        float strength, Vector2 casterPosition, bool clearLine = false)
    {
        DisplayAimingLineForNextSpells(GetNextSpellOffsets(), wandSpells, currentSpellIndex, -90, strength, casterPosition, clearLine);
    }

    public override void FireSpell(
        List<SpellBase> wandSpells, int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        FireSpellForNextSpells(GetNextSpellOffsets(), wandSpells, currentSpellIndex, -90, strength, context);
    }

    int[] nextSpellOffsets = { 1 };
    public override int[] GetNextSpellOffsets()
    {
        return nextSpellOffsets;
    }
}