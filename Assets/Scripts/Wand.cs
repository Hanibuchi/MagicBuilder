using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 魔法の杖を表すクラス。
/// 杖の種類と、セットされた呪文の配列を持ちます。
/// </summary>
[Serializable]
public class Wand
{
    [Header("杖の特性")]
    [Tooltip("この杖の種類")]
    public WandType type = WandType.Default;

    [Header("呪文スロット")]
    [Tooltip("この杖にセットされている呪文の配列。")]
    [SerializeReference] public List<ISpell> spells = new List<ISpell>();

    /// <summary>
    /// 現在の杖にセットされている呪文リストを取得します。
    /// </summary>
    /// <returns>呪文のリスト</returns>
    public List<ISpell> GetSpells()
    {
        return spells;
    }

    public void Add(ISpell spell)
    {
        spells.Add(spell);
    }
    public void Remove(ISpell spell)
    {
        spells.Remove(spell);
    }
}


// 呪文を表すマーカーインターフェース 
public interface ISpell
{
    // ISpellを継承したクラスが呪文として扱われます。
}

/// <summary>
/// 杖の種類を定義する列挙型（Enum）。
/// </summary>
public enum WandType
{
    Default,
    Crystal,
    Wood,
    Steam,
    Dark,
}