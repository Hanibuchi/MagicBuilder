using UnityEngine;
using System;

/// <summary>
/// 呪文の発射・実行時に、環境や発射元の情報などを伝達するためのクラス。
/// </summary>
public class SpellContext
{
    public Vector2 CasterPosition;
    public Action<GameObject> ProjectileModifier;
    public float errorDegree = 0;
    public Damage damage;
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

            // 参照型 (Action) は参照がコピーされるが、Actionは不変(イミュータブル)なので問題なし
            ProjectileModifier = this.ProjectileModifier,
            damage = this.damage
        };
    }
}
