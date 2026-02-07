using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 指定された呪文候補の中から、指定された数だけランダムに選んで同時詠唱する呪文クラス。
/// </summary>
[CreateAssetMenu(fileName = "RandomMultCastSpell", menuName = "Wand System/Random MultCast Spell")]
public class RandomMultCastSpell : MultCastSpell
{
    [Header("ランダムマルチキャスト設定")]
    [Tooltip("ターゲットの中からランダムに選ぶ数")]
    [SerializeField] protected int randomCount = 1;

    public override void FireSpell(
        List<SpellBase> wandSpells,
        int currentSpellIndex,
        float rotationZ,
        float strength,
        SpellContext context)
    {
        int[] allTargetIndices = GetTargetIndices(wandSpells, currentSpellIndex);
        if (allTargetIndices == null || allTargetIndices.Length == 0) return;

        // 実際の発射時は現在のフレームの状態に基づきランダムに選択
        int[] selected = allTargetIndices.OrderBy(x => Random.value).Take(randomCount).ToArray();

        FireSelectedSpells(selected, wandSpells, currentSpellIndex, rotationZ, strength, context);
    }
}
