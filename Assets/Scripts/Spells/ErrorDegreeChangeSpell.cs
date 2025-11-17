using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ErrorDegreeChangeSpell", menuName = "Wand System/ErrorDegreeChangeSpell")]
public class ErrorDegreeChangeSpell : SpellBase
{
    [SerializeField] float additionalErrorDegree = 0f;

    public override void FireSpell(
        List<SpellBase> wandSpells, int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        context.errorDegree += additionalErrorDegree;
        FireSpellForNextSpells(GetNextSpellOffsets(wandSpells, currentSpellIndex), wandSpells, currentSpellIndex, rotationZ, strength, context);
    }

    int[] nextSpellOffsets = { 1 };
    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells,
        int currentSpellIndex)
    {
        return nextSpellOffsets;
    }
}
