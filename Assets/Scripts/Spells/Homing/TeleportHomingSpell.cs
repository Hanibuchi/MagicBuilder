using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 後続の呪文によって生成される発射体を、近くの敵の場所へ即座にワープさせる修飾呪文。
/// </summary>
[CreateAssetMenu(fileName = "TeleportHomingSpell", menuName = "Wand System/Modifiers/Teleport Homing Spell")]
public class TeleportHomingSpell : SpellBase
{
    [Header("転位設定")]
    [Tooltip("ターゲットを検索する範囲")]
    [SerializeField] private float searchRange = 10f;
    [Tooltip("ワープの間隔（秒）。0にすると毎フレーム実行。")]
    [SerializeField] private float teleportInterval = 0.1f;

    [Header("範囲表示設定")]
    [Tooltip("範囲を示すプレハブ")]
    [SerializeField] private GameObject rangeVisualPrefab;

    [Tooltip("出現・消失にかける時間（秒）")]
    [SerializeField] private float animationDuration = 0.2f;

    [Tooltip("効果持続時間（秒）")]
    [SerializeField] private float effectDuration = 30f;

    readonly int[] nextSpellOffsets = { 1 };

    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells, int currentSpellIndex)
    {
        return nextSpellOffsets;
    }

    public override void DisplayAimingLine(
        List<SpellBase> wandSpells, int currentSpellIndex, float rotationZ,
        float strength, SpellContext context,
        bool clearLine = false)
    {
        // 転位は発射時の挙動であるため、通常の軌道予測を表示する
        DisplayAimingLineForNextSpells(
            GetNextSpellOffsets(wandSpells, currentSpellIndex),
            wandSpells, currentSpellIndex, rotationZ, strength, context, clearLine
        );
    }

    public override void FireSpell(
        List<SpellBase> wandSpells,
        List<ISpellCastListener> listeners,
        int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        // ProjectileModifierに転位コンポーネントの付与と初期化を登録
        context.ProjectileModifier += (projectile) =>
        {
            LayerMask mask = context.GetTargetLayerMask();

            if (projectile.TryGetComponent<TeleportHomingMover>(out var mover))
            {
                mover.AddTeleportHomingData(searchRange, teleportInterval, effectDuration);
            }
            else
            {
                mover = projectile.AddComponent<TeleportHomingMover>();
                mover.Initialize(mask, searchRange, teleportInterval, rangeVisualPrefab, animationDuration, effectDuration);
            }
            if (currentSpellIndex >= 0 && currentSpellIndex < listeners.Count)
            {
                listeners[currentSpellIndex]?.PlayCastAnimation();
            }
        };

        FireSpellForNextSpells(
            GetNextSpellOffsets(wandSpells, currentSpellIndex),
            wandSpells, listeners, currentSpellIndex, rotationZ, strength, context
        );
    }
}
