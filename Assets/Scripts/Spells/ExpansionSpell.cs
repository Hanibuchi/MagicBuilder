using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 後続の呪文によって生成される発射体のスケールを拡大・縮小する修飾呪文。
/// </summary>
[CreateAssetMenu(fileName = "ExpansionSpell", menuName = "Wand System/Expansion Spell")]
public class ExpansionSpell : SpellBase
{
    [Header("拡張設定")]
    [Tooltip("発射体のスケールを何倍にするか。1.0で変化なし。0.5で半分の大きさ（収縮）。2.0で2倍の大きさ（膨張）。")]
    [SerializeField] private float scaleMultiplier = 1.2f;

    readonly int[] nextSpellOffsets = { 1 };
    /// <summary>
    /// この呪文の次に発射される呪文のオフセット（インデックスの差分）を返します。
    /// </summary>
    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells,
        int currentSpellIndex)
    {
        // 次の呪文（インデックス + 1）のみを対象とする
        return nextSpellOffsets;
    }

    /// <summary>
    /// 補助線（軌道予測）を表示します。
    /// ここでは、発射体のスケールを変更する修飾子を DisplayAimingLine に追加します。
    /// </summary>
    public override void DisplayAimingLine(
        List<SpellBase> wandSpells, int currentSpellIndex, float rotationZ,
        float strength, SpellContext context,
        bool clearLine = false)
    {
        AddAimingModifier(context);

        // 2. 次の呪文に対して、新しい修飾子で DisplayAimingLine を呼び出す
        DisplayAimingLineForNextSpells(
            GetNextSpellOffsets(wandSpells, currentSpellIndex),
            wandSpells, currentSpellIndex, rotationZ, strength, context, clearLine
        );
    }

    void AddAimingModifier(SpellContext context)
    {
        context.AimingModifier += (projectile) =>
        {
            if (projectile != null)
                projectile.transform.localScale *= scaleMultiplier;
        };
    }

    /// <summary>
    /// 呪文の主要な効果を実行します。
    /// ここでは、発射体のスケールを変更する修飾子を SpellContext に追加します。
    /// </summary>
    public override void FireSpell(
        List<SpellBase> wandSpells, int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        AddAimingModifier(context);
        // 新しいコンテキストに新しい修飾子を設定
        context.ProjectileModifier += (projectile) =>
        {
            projectile.transform.localScale *= scaleMultiplier;
        };

        // 2. 次の呪文に対して、新しいコンテキストで FireSpell を呼び出す
        FireSpellForNextSpells(
            GetNextSpellOffsets(wandSpells, currentSpellIndex),
            wandSpells, currentSpellIndex, rotationZ, strength, context
        );
    }


    public override List<SpellDescriptionItem> GetDescriptionDetails()
    {
        base.GetDescriptionDetails();
        detailItems.Add(new SpellDescriptionItem
        {
            icon = SpellCommonData.Instance.scaleIcon,
            descriptionText = "膨張率 : ×" + scaleMultiplier.ToString(),
        });
        return detailItems;
    }
}