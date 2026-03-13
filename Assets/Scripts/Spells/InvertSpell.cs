using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "InvertSpell", menuName = "Wand System/Invert Spell")]
public class InvertSpell : SpellBase
{
    public enum InvertMode
    {
        Horizontal, // 左右反転
        Vertical,   // 上下反転
        Backward    // 後方反転（反対向き）
    }

    [SerializeField] private InvertMode invertMode = InvertMode.Horizontal;

    private float GetInvertedRotation(float rotationZ)
    {
        if (invertMode == InvertMode.Horizontal)
        {
            // 左右反転: 180度から引く（例：30度 -> 150度, -30度 -> -150度）
            // 角度の正規化等はUnityの回転系で処理されるため、単純な180度反転ロジック
            return 180f - rotationZ;
        }
        else if (invertMode == InvertMode.Vertical)
        {
            // 上下反転: 符号を反転（例：30度 -> -30度）
            return -rotationZ;
        }
        else
        {
            // 後方反転: 180度回転させる
            return rotationZ + 180f;
        }
    }

    public override void DisplayAimingLine(
        List<SpellBase> wandSpells, int currentSpellIndex, float rotationZ,
        float strength, SpellContext context,
        bool clearLine = false)
    {
        DisplayAimingLineForNextSpells(
            GetNextSpellOffsets(wandSpells, currentSpellIndex),
            wandSpells, currentSpellIndex, GetInvertedRotation(rotationZ), strength, context, clearLine);
    }

    public override void FireSpell(
        List<SpellBase> wandSpells,
        List<ISpellCastListener> listeners,
        int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        FireSpellForNextSpells(
            GetNextSpellOffsets(wandSpells, currentSpellIndex),
            wandSpells, listeners, currentSpellIndex, GetInvertedRotation(rotationZ), strength, context);
    }

    readonly int[] nextSpellOffsets = { 1 };
    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells,
        int currentSpellIndex)
    {
        return nextSpellOffsets;
    }
}
