using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "OffsetCastSpell", menuName = "Wand System/OffsetCast Spell")]
public class OffsetCastSpell : SpellBase
{
    [Header("オフセット設定")]
    [Tooltip("呼び出す呪文の相対位置（オフセット）の配列。1は次のスロット。")]
    [SerializeField] private int[] nextSpellOffsets = { 1 };

    /// <summary>
    /// この呪文から呼び出される次の呪文の相対オフセットを返します。
    /// </summary>
    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells, int currentSpellIndex)
    {
        return nextSpellOffsets;
    }

    public override void DisplayAimingLine(
        List<SpellBase> wandSpells,
        int currentSpellIndex,
        float rotationZ,
        float strength,
        Vector2 casterPosition,
        System.Action<GameObject> aimingModifier,
        bool clearLine = false)
    {
        // 指定されたオフセット位置にある呪文の補助線を表示します。
        DisplayAimingLineForNextSpells(nextSpellOffsets, wandSpells, currentSpellIndex, rotationZ, strength, casterPosition, aimingModifier, clearLine);
    }

    public override void FireSpell(
        List<SpellBase> wandSpells,
        int currentSpellIndex,
        float rotationZ,
        float strength,
        SpellContext context)
    {
        // 指定されたオフセット位置にある呪文を発射します。
        FireSpellForNextSpells(nextSpellOffsets, wandSpells, currentSpellIndex, rotationZ, strength, context);
    }
}
