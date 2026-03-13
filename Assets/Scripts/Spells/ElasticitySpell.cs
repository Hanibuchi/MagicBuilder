using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ElasticitySpell", menuName = "Wand System/Elasticity Spell")]
public class ElasticitySpell : SpellBase
{
    [SerializeField] int additionalBounceCount = 5;

    public override void FireSpell(
        List<SpellBase> wandSpells,
        List<ISpellCastListener> listeners,
        int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        context.bounceCount += additionalBounceCount;
        if (currentSpellIndex >= 0 && currentSpellIndex < listeners.Count)
        {
            listeners[currentSpellIndex]?.PlayCastAnimation();
        }
        FireSpellForNextSpells(GetNextSpellOffsets(wandSpells, currentSpellIndex), wandSpells, listeners, currentSpellIndex, rotationZ, strength, context);
    }

    readonly int[] nextSpellOffsets = { 1 };
    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells,
        int currentSpellIndex)
    {
        return nextSpellOffsets;
    }

    public override List<SpellDescriptionItem> GetDescriptionDetails()
    {
        base.GetDescriptionDetails();
        string text = "バウンド回数 : ";
        if (additionalBounceCount > 0)
        {
            text += "+" + additionalBounceCount.ToString();
        }
        else
        {
            text += additionalBounceCount.ToString();
        }
        
        detailItems.Add(new SpellDescriptionItem
        {
            // アイコンは暫定的にscaleIconを使用（必要に応じてSpellCommonDataに追加してください）
            icon = SpellCommonData.Instance.scaleIcon,
            descriptionText = text,
        });
        return detailItems;
    }
}
