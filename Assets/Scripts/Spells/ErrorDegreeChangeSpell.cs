using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ErrorDegreeChangeSpell", menuName = "Wand System/ErrorDegreeChangeSpell")]
public class ErrorDegreeChangeSpell : SpellBase
{
    [SerializeField] float additionalErrorDegree = 0f;

    public override void FireSpell(
        List<SpellBase> wandSpells,
        List<ISpellCastListener> listeners,
        int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        context.errorDegree += additionalErrorDegree;
        FireSpellForNextSpells(GetNextSpellOffsets(wandSpells, currentSpellIndex), wandSpells, listeners, currentSpellIndex, rotationZ, strength, context);
    }

    int[] nextSpellOffsets = { 1 };
    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells,
        int currentSpellIndex)
    {
        return nextSpellOffsets;
    }


    public override List<SpellDescriptionItem> GetDescriptionDetails()
    {
        base.GetDescriptionDetails();
        string text = "誤差 : ";
        if (additionalErrorDegree > 0)
        {
            text += "+" + additionalErrorDegree.ToString() + "度";
        }
        else
        {
            text += additionalErrorDegree.ToString() + "度";
        }
        detailItems.Add(new SpellDescriptionItem
        {
            icon = SpellCommonData.Instance.errorDegreeIcon,
            descriptionText = text,
        });
        return detailItems;
    }
}
