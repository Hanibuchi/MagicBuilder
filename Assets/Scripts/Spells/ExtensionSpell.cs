using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ExtensionSpell", menuName = "Wand System/Modifier/Extension Spell")]
public class ExtensionSpell : SpellBase
{
    [Header("延長設定")]
    [Tooltip("追加する持続時間（秒）")]
    [SerializeField] private float additionalDuration = 2.0f;

    public override void FireSpell(
        List<SpellBase> wandSpells, int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        // duration を増加させる
        context.duration += additionalDuration;

        // 次の呪文へ処理を移行
        FireSpellForNextSpells(GetNextSpellOffsets(wandSpells, currentSpellIndex), wandSpells, currentSpellIndex, rotationZ, strength, context);
    }

    public override void DisplayAimingLine(
        List<SpellBase> wandSpells, int currentSpellIndex,
        float rotationZ, float strength, SpellContext context, bool clearLine = false)
    {
        // 予測線でも duration を増加させる（弾丸の到達距離などが変わる可能性があるため）
        context.duration += additionalDuration;

        DisplayAimingLineForNextSpells(GetNextSpellOffsets(wandSpells, currentSpellIndex), wandSpells, currentSpellIndex, rotationZ, strength, context, clearLine);
    }

    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells, int currentSpellIndex)
    {
        return new int[] { 1 };
    }

    public override List<SpellDescriptionItem> GetDescriptionDetails()
    {
        base.GetDescriptionDetails();
        detailItems.Add(new SpellDescriptionItem
        {
            icon = null, // 必要に応じて時計アイコンなどを指定
            descriptionText = $"持続時間 : {additionalDuration:+0.0;-0.0;0.0} 秒",
        });
        return detailItems;
    }
}
