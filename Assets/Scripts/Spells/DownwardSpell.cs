using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "DownwardSpell", menuName = "Wand System/Downward Spell")]
public class DownwardSpell : SpellBase
{
    public enum SpellDirection
    {
        Down,
        Up,
    }

    [SerializeField] SpellDirection direction = SpellDirection.Down;

    private float GetRotation()
    {
        return direction switch
        {
            SpellDirection.Up => 90f,
            SpellDirection.Down => -90f,
            _ => -90f
        };
    }

    public override void DisplayAimingLine(
        List<SpellBase> wandSpells, int currentSpellIndex, float rotationZ,
        float strength, SpellContext context,
        bool clearLine = false)
    {
        DisplayAimingLineForNextSpells(GetNextSpellOffsets(wandSpells, currentSpellIndex), wandSpells, currentSpellIndex, GetRotation(), strength, context, clearLine);
    }

    public override void FireSpell(
        List<SpellBase> wandSpells, int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        FireSpellForNextSpells(GetNextSpellOffsets(wandSpells, currentSpellIndex), wandSpells, currentSpellIndex, GetRotation(), strength, context);
    }

    readonly int[] nextSpellOffsets = { 1 };
    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells,
        int currentSpellIndex)
    {
        return nextSpellOffsets;
    }
}