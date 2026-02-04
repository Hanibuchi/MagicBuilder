using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 後続の呪文の重力（Rigidbody2DのgravityScale）を変更する修飾呪文。
/// </summary>
[CreateAssetMenu(fileName = "LowGravitySpell", menuName = "Wand System/Low Gravity Spell")]
public class LowGravitySpell : SpellBase
{
    [Header("重力設定")]
    [Tooltip("Rigidbody2DのgravityScaleに加える値。負の値で重力が軽くなります。")]
    [SerializeField] private float gravityChange = -0.5f;

    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells, int currentSpellIndex)
    {
        return new int[] { 1 };
    }

    public override void DisplayAimingLine(
        List<SpellBase> wandSpells, int currentSpellIndex, float rotationZ,
        float strength, SpellContext context,
        bool clearLine = false)
    {
        // 軌道予測用にコンテキストの重力係数を更新
        context.gravityModifier += gravityChange;

        DisplayAimingLineForNextSpells(
            GetNextSpellOffsets(wandSpells, currentSpellIndex),
            wandSpells, currentSpellIndex, rotationZ, strength, context, clearLine
        );
    }

    public override void FireSpell(
        List<SpellBase> wandSpells, int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        // 念のためコンテキストも更新（後続の呪文が参照する場合のため）
        context.gravityModifier += gravityChange;

        // 発射体のRigidbody2Dを修正するアクションを追加
        context.ProjectileModifier += (projectile) =>
        {
            if (projectile != null && projectile.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            {
                rb.gravityScale += gravityChange;
            }
        };

        FireSpellForNextSpells(
            GetNextSpellOffsets(wandSpells, currentSpellIndex),
            wandSpells, currentSpellIndex, rotationZ, strength, context
        );
    }

    public override List<SpellDescriptionItem> GetDescriptionDetails()
    {
        var items = base.GetDescriptionDetails();
        items.Add(new SpellDescriptionItem
        {
            icon = null,
            descriptionText = "重力影響 : " + (gravityChange > 0 ? "+" : "") + gravityChange.ToString("F1"),
        });
        return items;
    }
}
