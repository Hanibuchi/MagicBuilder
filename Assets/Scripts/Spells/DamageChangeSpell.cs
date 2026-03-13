using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DamageChangeSpell", menuName = "Wand System/DamageChangeSpell")]
public class DamageChangeSpell : SpellBase
{
    [SerializeField] Damage additionalDamage;

    public override void FireSpell(
        List<SpellBase> wandSpells,
        List<ISpellCastListener> listeners,
        int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        context.damage += additionalDamage;
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

        if (additionalDamage.baseDamage != 0)
        {
            AddDamageDetail(SpellCommonData.Instance.damageIcon, "ダメージ", additionalDamage.baseDamage);
        }
        if (additionalDamage.FireDamage != 0)
        {
            AddDamageDetail(SpellCommonData.Instance.fireDamageIcon, "火ダメージ", additionalDamage.FireDamage);
        }
        if (additionalDamage.IceDamage != 0)
        {
            AddDamageDetail(SpellCommonData.Instance.iceDamageIcon, "氷ダメージ", additionalDamage.IceDamage);
        }
        if (additionalDamage.waterDamage != 0)
        {
            AddDamageDetail(SpellCommonData.Instance.waterDamageIcon, "水ダメージ", additionalDamage.waterDamage);
        }
        if (additionalDamage.woodDamage != 0)
        {
            AddDamageDetail(SpellCommonData.Instance.woodDamageIcon, "木ダメージ", additionalDamage.woodDamage);
        }
        if (additionalDamage.healing != 0)
        {
            AddDamageDetail(SpellCommonData.Instance.healingIcon, "回復量", additionalDamage.healing);
        }
        if (additionalDamage.knockback != 0)
        {
            AddDamageDetail(SpellCommonData.Instance.knockbackIcon, "ノックバック", additionalDamage.knockback);
        }

        return detailItems;
    }

    private void AddDamageDetail(Sprite icon, string label, float value)
    {
        string text = $"{label} : {(value > 0 ? "+" : "")}{value}";
        detailItems.Add(new SpellDescriptionItem
        {
            icon = icon,
            descriptionText = text,
        });
    }
}
