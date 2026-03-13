using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "TurnRightSpell", menuName = "Wand System/Turn Right Spell")]
public class TurnRightSpell : SpellBase
{
    public enum SpellDirection
    {
        Right,
        Left,
    }

    [SerializeField] SpellDirection direction = SpellDirection.Right;

    private float GetRotation()
    {
        return direction switch
        {
            SpellDirection.Right => -90f,
            SpellDirection.Left => 90f,
            _ => -90f
        };
    }

    public override void DisplayAimingLine(
        List<SpellBase> wandSpells, int currentSpellIndex, float rotationZ,
        float strength, SpellContext context,
        bool clearLine = false)
    {
        DisplayAimingLineForNextSpells(GetNextSpellOffsets(wandSpells, currentSpellIndex), wandSpells, currentSpellIndex, rotationZ + GetRotation(), strength, context, clearLine);
    }

    public override void FireSpell(
        List<SpellBase> wandSpells,
        List<ISpellCastListener> listeners,
        int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        FireSpellForNextSpells(GetNextSpellOffsets(wandSpells, currentSpellIndex), wandSpells, listeners, currentSpellIndex, rotationZ + GetRotation(), strength, context);
    }

    readonly int[] nextSpellOffsets = { 1 };
    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells,
        int currentSpellIndex)
    {
        return nextSpellOffsets;
    }
}