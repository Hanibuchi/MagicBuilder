using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AccelerationSpell", menuName = "Wand System/Acceleration Spell")]
public class AccelerationSpell : SpellBase
{
    [SerializeField] private float strengthMultiplier = 1.5f;

    public override void DisplayAimingLine(
        List<SpellBase> wandSpells, int currentSpellIndex, float rotationZ,
        float strength, SpellContext context,
        bool clearLine = false)
    {
        DisplayAimingLineForNextSpells(GetNextSpellOffsets(wandSpells, currentSpellIndex), wandSpells, currentSpellIndex, rotationZ, strength * strengthMultiplier, context, clearLine);
    }

    public override void FireSpell(
        List<SpellBase> wandSpells, int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        FireSpellForNextSpells(GetNextSpellOffsets(wandSpells, currentSpellIndex), wandSpells, currentSpellIndex, rotationZ, strength * strengthMultiplier, context);
    }

    readonly int[] nextSpellOffsets = { 1 };
    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells,
        int currentSpellIndex)
    {
        return nextSpellOffsets;
    }

    public override List<SpellDescriptionItem> GetDescriptionDetails()
    {
        var items = base.GetDescriptionDetails();
        items.Add(new SpellDescriptionItem
        {
            icon = null,
            descriptionText = "初速倍率 : " + strengthMultiplier.ToString("F2") + " 倍",
        });
        return items;
    }
}
