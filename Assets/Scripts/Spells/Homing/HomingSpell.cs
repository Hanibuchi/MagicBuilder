using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 後続の呪文によって生成される発射体にホーミング機能を付与する修飾呪文。
/// </summary>
[CreateAssetMenu(fileName = "HomingSpell", menuName = "Wand System/Homing Spell")]
public class HomingSpell : SpellBase
{
    [Header("ホーミング設定")]
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private float searchRange = 10f;
    [SerializeField] private float springConstant = 1f; // ばね定数

    readonly int[] nextSpellOffsets = { 1 };

    /// <summary>
    /// この呪文の次に発射される呪文のオフセット（インデックスの差分）を返します。
    /// </summary>
    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells, int currentSpellIndex)
    {
        return nextSpellOffsets;
    }

    /// <summary>
    /// 補助線（軌道予測）を表示します。
    /// ホーミングは予測が難しいため、ここでは特に予測線を変更しません。
    /// </summary>
    public override void DisplayAimingLine(
        List<SpellBase> wandSpells, int currentSpellIndex, float rotationZ,
        float strength, SpellContext context,
        bool clearLine = false)
    {
        DisplayAimingLineForNextSpells(
            GetNextSpellOffsets(wandSpells, currentSpellIndex),
            wandSpells, currentSpellIndex, rotationZ, strength, context, clearLine
        );
    }

    /// <summary>
    /// 呪文の主要な効果を実行します。
    /// </summary>
    public override void FireSpell(
        List<SpellBase> wandSpells, int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        // context.ProjectileModifier にホーミングコンポーネントの付与と初期化を登録
        context.ProjectileModifier += (projectile) =>
        {
            if (!projectile.TryGetComponent<HomingMover>(out var homing))
            {
                homing = projectile.AddComponent<HomingMover>();
            }
            // context のレイヤー設定に基づいたターゲットレイヤーを優先し、
            // 未設定（デフォルト）の場合はインスペクタの targetLayer を使用する
            LayerMask mask = context.GetTargetLayerMask();
            homing.Initialize(mask, searchRange, springConstant);
        };

        FireSpellForNextSpells(
            GetNextSpellOffsets(wandSpells, currentSpellIndex),
            wandSpells, currentSpellIndex, rotationZ, strength, context
        );
    }
}
