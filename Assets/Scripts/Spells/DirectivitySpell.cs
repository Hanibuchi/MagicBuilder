using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 発射時の方向を最寄りの敵に向かわせる修飾呪文。
/// </summary>
[CreateAssetMenu(fileName = "DirectivitySpell", menuName = "Wand System/Directivity Spell")]
public class DirectivitySpell : SpellBase
{
    [Header("指向設定")]
    [Tooltip("敵を捜索する範囲")]
    [SerializeField] private float searchRange = 20f;

    public override void DisplayAimingLine(
        List<SpellBase> wandSpells, int currentSpellIndex, float rotationZ,
        float strength, SpellContext context,
        bool clearLine = false)
    {
        float targetRotationZ = rotationZ;
        if (TryGetAngleToNearestTarget(context.CasterPosition, context.GetTargetLayerMask(), out float angle))
        {
            targetRotationZ = angle;
        }

        DisplayAimingLineForNextSpells(
            GetNextSpellOffsets(wandSpells, currentSpellIndex),
            wandSpells, currentSpellIndex, targetRotationZ, strength, context, clearLine
        );
    }

    public override void FireSpell(
        List<SpellBase> wandSpells, int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        float targetRotationZ = rotationZ;
        LayerMask mask = context.GetTargetLayerMask();

        if (TryGetAngleToNearestTarget(context.CasterPosition, mask, out float angle))
        {
            targetRotationZ = angle;

            // ProjectileModifierに、生成されたオブジェクトの速度をターゲット方向に向ける処理を登録
            context.ProjectileModifier += (projectile) =>
            {
                if (projectile != null && projectile.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
                {
                    // 改めて方向を計算（発射直後の位置から）
                    if (TryGetDirectionToNearestTarget((Vector2)projectile.transform.position, mask, out Vector2 dir))
                    {
                        // 速度の大きさ（スカラー）を維持しつつ、方向をターゲットに向ける
                        float speed = rb.linearVelocity.magnitude;
                        rb.linearVelocity = dir * speed;
                    }
                }
            };
        }

        FireSpellForNextSpells(
            GetNextSpellOffsets(wandSpells, currentSpellIndex),
            wandSpells, currentSpellIndex, targetRotationZ, strength, context
        );
    }

    private bool TryGetAngleToNearestTarget(Vector2 origin, LayerMask mask, out float angle)
    {
        if (TryGetDirectionToNearestTarget(origin, mask, out Vector2 dir))
        {
            angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            return true;
        }
        angle = 0;
        return false;
    }

    private bool TryGetDirectionToNearestTarget(Vector2 origin, LayerMask mask, out Vector2 direction)
    {
        Transform target = GetNearestTarget(origin, mask);
        if (target != null)
        {
            direction = ((Vector2)target.position - origin).normalized;
            return true;
        }
        direction = Vector2.zero;
        return false;
    }

    private Transform GetNearestTarget(Vector2 position, LayerMask mask)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, searchRange, mask);
        Transform nearest = null;
        float minDistance = float.MaxValue;

        foreach (var col in colliders)
        {
            float dist = Vector2.Distance(position, col.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = col.transform;
            }
        }
        return nearest;
    }

    readonly int[] nextSpellOffsets = { 1 };
    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells, int currentSpellIndex)
    {
        return nextSpellOffsets;
    }

    public override List<SpellDescriptionItem> GetDescriptionDetails()
    {
        var items = base.GetDescriptionDetails();
        items.Add(new SpellDescriptionItem
        {
            icon = null,
            descriptionText = "索敵範囲 : " + searchRange.ToString("F1"),
        });
        return items;
    }
}
