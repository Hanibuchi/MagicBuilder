using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "AttractionSpell", menuName = "Wand System/Attraction Spell")]
public class AttractionSpell : SpellBase
{
    [Header("引力/斥力設定")]
    [Tooltip("引き寄せる範囲（負の値で斥力演出）")]
    public float range = 5.0f;
    [Tooltip("引き寄せる力（負の値で斥力）")]
    public float force = 10.0f;
    [Tooltip("引き寄せる対象のレイヤー")]
    public LayerMask targetLayer;
    [Tooltip("引力時の演出用プレハブ")]
    public GameObject attractionEffectPrefab;
    [Tooltip("斥力時の演出用プレハブ")]
    public GameObject repulsionEffectPrefab;

    public override void FireSpell(
        List<SpellBase> wandSpells, int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        context.ProjectileModifier += (GameObject obj) =>
        {
            AttractionMover mover = obj.GetComponent<AttractionMover>();
            if (mover == null)
            {
                mover = obj.AddComponent<AttractionMover>();
            }
            
            mover.AddAttractionData(range, force, attractionEffectPrefab, repulsionEffectPrefab, targetLayer);
        };

        // 次の呪文を実行
        FireSpellForNextSpells(
            GetNextSpellOffsets(wandSpells, currentSpellIndex),
            wandSpells, currentSpellIndex, rotationZ, strength, context);
    }

    private readonly int[] _nextSpellOffsets = { 1 };
    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells, int currentSpellIndex)
    {
        return _nextSpellOffsets;
    }
}
