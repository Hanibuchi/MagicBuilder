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

    [SerializeField] private float effectDuration = 30f;

    [Header("エフェクト設定")]
    [Tooltip("重力軽減時の風船エフェクトプレハブ")]
    [SerializeField] private GameObject balloonEffectPrefab;
    [Tooltip("風船エフェクト表示のランダム範囲 (最小)")]
    [SerializeField] private Vector2 balloonAreaMin = new Vector2(-0.3f, 0f);
    [Tooltip("風船エフェクト表示のランダム範囲 (最大)")]
    [SerializeField] private Vector2 balloonAreaMax = new Vector2(0.3f, 0.5f);
    
    [Tooltip("重力増加時のおもりエフェクトプレハブ")]
    [SerializeField] private GameObject weightEffectPrefab;
    [Tooltip("おもりエフェクト表示のランダム範囲 (最小)")]
    [SerializeField] private Vector2 weightAreaMin = new Vector2(-0.3f, -0.5f);
    [Tooltip("おもりエフェクト表示のランダム範囲 (最大)")]
    [SerializeField] private Vector2 weightAreaMax = new Vector2(0.3f, 0f);

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
            if (projectile != null)
            {
                // エフェクトの種類と相対位置をSpell側で計算してModifierに渡す
                GameObject prefabToUse = gravityChange < 0 ? balloonEffectPrefab : weightEffectPrefab;
                Vector2 minArea = gravityChange < 0 ? balloonAreaMin : weightAreaMin;
                Vector2 maxArea = gravityChange < 0 ? balloonAreaMax : weightAreaMax;
                
                Vector3 randomOffset = new Vector3(
                    UnityEngine.Random.Range(minArea.x, maxArea.x),
                    UnityEngine.Random.Range(minArea.y, maxArea.y),
                    0f
                );

                if (projectile.TryGetComponent<LowGravityModifier>(out var modifier))
                {
                    modifier.AddEffect(gravityChange, effectDuration, prefabToUse, randomOffset);
                }
                else
                {
                    modifier = projectile.AddComponent<LowGravityModifier>();
                    modifier.Initialize(gravityChange, effectDuration, prefabToUse, randomOffset);
                }
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
