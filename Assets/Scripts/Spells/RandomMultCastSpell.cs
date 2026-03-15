using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 指定された呪文候補の中から、指定された数だけランダムに選んで同時詠唱する呪文クラス。
/// </summary>
[CreateAssetMenu(fileName = "RandomMultCastSpell", menuName = "Wand System/Random MultCast Spell")]
public class RandomMultCastSpell : MultCastSpell
{
    public override void DisplayAimingLine(
        List<SpellBase> wandSpells,
        int currentSpellIndex,
        float rotationZ,
        float strength,
        SpellContext context,
        bool clearLine = false)
    {
        int[] allTargetIndices = GetTargetIndices(wandSpells, currentSpellIndex);
        if (allTargetIndices == null || allTargetIndices.Length == 0) return;

        // 呪文候補の1.5倍（切り上げ）の数だけ表示します。ただし1つの場合は1固定。
        int count = allTargetIndices.Length == 1 ? 1 : Mathf.CeilToInt(allTargetIndices.Length * 1.5f);
        int[] selected = new int[count];
        for (int i = 0; i < count; i++)
        {
            selected[i] = allTargetIndices[i % allTargetIndices.Length];
        }

        DisplaySelectedAimingLine(selected, wandSpells, currentSpellIndex, rotationZ, strength, context, clearLine);
    }

    public override void FireSpell(
        List<SpellBase> wandSpells,
        List<ISpellCastListener> listeners,
        int currentSpellIndex,
        float rotationZ,
        float strength,
        SpellContext context)
    {
        int[] allTargetIndices = GetTargetIndices(wandSpells, currentSpellIndex);
        if (allTargetIndices == null || allTargetIndices.Length == 0) return;

        // 呪文候補の1.5倍（切り上げ）の数だけ発射します。ただし1つの場合は1固定。
        // 各ショットに対して、毎回 allTargetIndices から独立してランダムに1つ選択します。
        int count = allTargetIndices.Length == 1 ? 1 : Mathf.CeilToInt(allTargetIndices.Length * 1.5f);
        int[] selected = new int[count];
        for (int i = 0; i < count; i++)
        {
            selected[i] = allTargetIndices[Random.Range(0, allTargetIndices.Length)];
        }

        FireSelectedSpells(selected, wandSpells, listeners, currentSpellIndex, rotationZ, strength, context);
    }
}
