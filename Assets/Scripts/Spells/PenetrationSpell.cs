using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PenetrationSpell", menuName = "Wand System/PenetrationSpell")]
public class PenetrationSpell : SpellBase
{
    [SerializeField] private float effectDuration = 30f;

    public override void FireSpell(List<SpellBase> wandSpells,
        List<ISpellCastListener> listeners,
        int currentSpellIndex, float rotationZ, float strength, SpellContext context)
    {
        AddPenetrationModifier(context);
        FireSpellForNextSpells(GetNextSpellOffsets(wandSpells, currentSpellIndex), wandSpells, listeners, currentSpellIndex, rotationZ, strength, context);
    }

    public override void DisplayAimingLine(List<SpellBase> wandSpells, int currentSpellIndex, float rotationZ, float strength, SpellContext context, bool clearLine = false)
    {
        DisplayAimingLineForNextSpells(GetNextSpellOffsets(wandSpells, currentSpellIndex), wandSpells, currentSpellIndex, rotationZ, strength, context, clearLine);
    }

    private void AddPenetrationModifier(SpellContext context)
    {
        context.ProjectileModifier += (GameObject obj) =>
        {
            if (obj != null)
            {
                if (obj.TryGetComponent<PenetrationModifier>(out var modifier))
                {
                    modifier.AddEffect(effectDuration);
                }
                else
                {
                    modifier = obj.AddComponent<PenetrationModifier>();
                    modifier.Initialize(effectDuration);
                }
            }
        };
    }

    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells, int currentSpellIndex)
    {
        // 1つの呪文（次のまとまり）を対象とする修飾呪文
        return new int[] { 1 };
    }
}