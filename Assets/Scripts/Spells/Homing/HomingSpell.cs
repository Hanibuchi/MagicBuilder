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
    [SerializeField] private float characterSpringConstant = 50f; // プロジェクタイルがキャラクターの場合のばね定数
    [SerializeField] private float effectDuration = 30f;

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
        List<SpellBase> wandSpells,
        List<ISpellCastListener> listeners,
        int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        // context.ProjectileModifier にホーミングコンポーネントの付与と初期化を登録
        context.ProjectileModifier += (projectile) =>
        {
            LayerMask mask = context.GetTargetLayerMask();
            // プロジェクタイル自体にCharacterControllerがアタッチされているか（モブ召喚など）で値を切り替える
            float targetSpringConstant = projectile.TryGetComponent<MyObjectController>(out var controller) && !controller.IsProjectile ? characterSpringConstant : springConstant;

            if (projectile.TryGetComponent<HomingMover>(out var homing))
            {
                // すでにHomingMoverがある場合はばね定数を加算する
                homing.AddSpringConstant(targetSpringConstant, effectDuration);
            }
            else
            {
                // まだない場合は新しく追加して初期化する
                homing = projectile.AddComponent<HomingMover>();
                homing.Initialize(mask, searchRange, targetSpringConstant, effectDuration);
            }
        };
        FireSpellForNextSpells(
            GetNextSpellOffsets(wandSpells, currentSpellIndex),
            wandSpells, listeners, currentSpellIndex, rotationZ, strength, context
        );
    }
}
