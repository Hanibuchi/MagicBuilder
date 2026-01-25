using UnityEngine;
using System;

public enum SpellLayer
{
    Attack_Ally,
    Attack_Enemy,
    Attack_Both
}

/// <summary>
/// 呪文の発射・実行時に、環境や発射元の情報などを伝達するためのクラス。
/// </summary>
public class SpellContext
{
    public Vector2 CasterPosition;
    public Action<GameObject> ProjectileModifier;
    public float errorDegree = 0;
    public Damage damage;
    public SpellLayer layer;
    /// <summary>
    /// 呪文の持続時間（秒）。-1の場合は無限。
    /// </summary>
    public float duration = 0;

    /// <summary>
    /// 呪文が永続的（無限の持続時間）であるかどうかを返します。
    /// </summary>
    /// <returns>duration が -1 の場合は true、それ以外は false。</returns>
    public bool IsPermanent()
    {
        return Mathf.Approximately(duration, -1f);
    }

    /// <summary>
    /// 現在の layer 設定に基づいた Unity のレイヤーインデックスを取得します。
    /// </summary>
    /// <param name="isProjectile">投射物（攻撃用）のレイヤーを取得する場合は true、キャラクター（バリア等）の場合は false。</param>
    /// <returns>Unity のレイヤーインデックス。</returns>
    public int GetUnityLayer(bool isProjectile = true)
    {
        switch (layer)
        {
            case SpellLayer.Attack_Ally:
                return isProjectile ? CharacterHealth.ALLAY_ATTACK_LAYER_INDEX : CharacterHealth.ALLAY_LAYER_INDEX;
            case SpellLayer.Attack_Enemy:
                return isProjectile ? CharacterHealth.ENEMY_ATTACK_LAYER_INDEX : CharacterHealth.ENEMY_LAYER_INDEX;
            case SpellLayer.Attack_Both:
                return isProjectile ? CharacterHealth.BOTH_ATTACK_LAYER_INDEX : CharacterHealth.BOTH_ATTACK_LAYER_INDEX;
            default:
                return isProjectile ? CharacterHealth.ALLAY_ATTACK_LAYER_INDEX : CharacterHealth.ALLAY_LAYER_INDEX;
        }
    }

    /// <summary>
    /// 現在の layer 設定に基づいた、攻撃対象とするレイヤーの LayerMask を取得します。
    /// </summary>
    /// <returns>ターゲットの LayerMask。</returns>
    public LayerMask GetTargetLayerMask()
    {
        switch (layer)
        {
            case SpellLayer.Attack_Ally:
                return 1 << CharacterHealth.ENEMY_LAYER_INDEX;
            case SpellLayer.Attack_Enemy:
                return 1 << CharacterHealth.ALLAY_LAYER_INDEX;
            case SpellLayer.Attack_Both:
                return (1 << CharacterHealth.ALLAY_LAYER_INDEX) | (1 << CharacterHealth.ENEMY_LAYER_INDEX);
            default:
                return 1 << CharacterHealth.ENEMY_LAYER_INDEX;
        }
    }


    public SpellContext()
    {

    }

    /// <summary>
    /// このコンテキストの値をコピーした新しいインスタンスを返す。
    /// </summary>
    /// <returns>値が同じ新しい SpellContext インスタンス。</returns>
    public SpellContext Clone()
    {
        return new SpellContext
        {
            // 値型 (Vector2, float) は値そのものがコピーされる
            CasterPosition = this.CasterPosition,
            errorDegree = this.errorDegree,
            duration = this.duration,
            layer = this.layer,

            // 参照型 (Action) は参照がコピーされるが、Actionは不変(イミュータブル)なので問題なし
            ProjectileModifier = this.ProjectileModifier,
            damage = this.damage
        };
    }
}
