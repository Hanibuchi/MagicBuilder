using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 発射体を指定した方向（上下左右）に加速させる修飾呪文。
/// </summary>
[CreateAssetMenu(fileName = "DirectionalAccelerationSpell", menuName = "Wand System/Modifiers/Directional Acceleration Spell")]
public class DirectionalAccelerationSpell : SpellBase
{
    public enum AccelerationDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    [Header("加速設定")]
    [Tooltip("加速する方向")]
    [SerializeField] private AccelerationDirection direction = AccelerationDirection.Right;

    [Tooltip("加える速度の強さ")]
    [SerializeField] private float accelerationPower = 5f;

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
        // 加速は発射後に適用されるため、基本的な描画処理のみを継承後続に回す
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
        // ProjectileModifierに速度追加の処理を登録
        context.ProjectileModifier += (projectile) =>
        {
            if (projectile.TryGetComponent<Rigidbody2D>(out var rb))
            {
                Vector2 force = GetDirectionVector() * accelerationPower;
                // Unity 2023以降の Rigidbody2D.linearVelocity を使用
                rb.linearVelocity += force;
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

    private Vector2 GetDirectionVector()
    {
        return direction switch
        {
            AccelerationDirection.Up => Vector2.up,
            AccelerationDirection.Down => Vector2.down,
            AccelerationDirection.Left => Vector2.left,
            AccelerationDirection.Right => Vector2.right,
            _ => Vector2.right
        };
    }
}
