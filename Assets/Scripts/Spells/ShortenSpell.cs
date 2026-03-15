using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ShortenSpell", menuName = "Wand System/ShortenSpell")]
public class ShortenSpell : SpellBase
{
    [SerializeField] float coolDownMultiplier = 1f;
    [SerializeField] float coolDownReduction = 0f;

    public override int Preprocess(List<SpellBase> wandSpells, int currentSpellIndex, List<ISpellCastListener> listeners = null)
    {
        if (listeners != null && currentSpellIndex >= 0 && currentSpellIndex < listeners.Count)
        {
            listeners[currentSpellIndex]?.PlayCastAnimation();
        }
        return base.Preprocess(wandSpells, currentSpellIndex, listeners);
    }

    public override float ModifyCooldown(float currentCooldown)
    {
        return currentCooldown * coolDownMultiplier - coolDownReduction;
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
        if (coolDownMultiplier != 1f)
            detailItems.Add(new SpellDescriptionItem
            {
                icon = SpellCommonData.Instance.coolDownIcon,
                descriptionText = $"クールタイム倍率 : x{coolDownMultiplier:F2}",
            });
        if (coolDownReduction != 0f)
            detailItems.Add(new SpellDescriptionItem
            {
                icon = SpellCommonData.Instance.coolDownIcon,
                descriptionText = $"クールタイム減少 : -{coolDownReduction:F2} 秒",
            });
        return detailItems;
    }
}
