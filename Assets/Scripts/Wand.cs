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
    [SerializeReference] public List<SpellBase> spells = new List<SpellBase>();

    // 呪文リストの変更を監視するリスナーのリスト
    private readonly List<ISpellListChangeListener> listeners = new List<ISpellListChangeListener>();

    /// <summary>
    /// 現在の杖にセットされている呪文リストを取得します。
    /// </summary>
    /// <returns>呪文のリスト</returns>
    public List<SpellBase> GetSpells()
    {
        return spells;
    }

    public void Add(SpellBase spell)
    {
        spells.Add(spell);
        NotifyListChanged();
    }
    public void Remove(SpellBase spell)
    {
        spells.Remove(spell);
        NotifyListChanged();
    }

    /// <summary>
    /// 呪文リストの変更をすべてのリスナーに通知します。
    /// </summary>
    private void NotifyListChanged()
    {
        foreach (var listener in listeners)
        {
            if (listener != null)
                listener.OnSpellListChanged(GetSpells());
            else
                listeners.Remove(listener);
        }
    }

    /// <summary>
    /// リスナーを追加します。
    /// </summary>
    public void AddListener(ISpellListChangeListener listener)
    {
        if (!listeners.Contains(listener))
        {
            listeners.Add(listener);
        }
    }

    /// <summary>
    /// リスナーを削除します。
    /// </summary>
    public void RemoveListener(ISpellListChangeListener listener)
    {
        listeners.Remove(listener);
    }
}

/// <summary>
/// 呪文リストの変更を通知するためのインターフェース。
/// </summary>
public interface ISpellListChangeListener
{
    /// <summary>
    /// 呪文リストが変更されたときに呼び出されます。
    /// </summary>
    void OnSpellListChanged(List<SpellBase> spells);
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