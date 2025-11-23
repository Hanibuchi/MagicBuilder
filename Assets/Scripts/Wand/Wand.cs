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
    private ISpellListChangeListener listener;

    /// <summary>
    /// 現在の杖にセットされている呪文リストを取得します。
    /// </summary>
    /// <returns>呪文のリスト</returns>
    public List<SpellBase> GetSpells()
    {
        return spells;
    }

    public void Add(SpellBase spell, int index)
    {
        // Debug.Log("index: " + index);
        // Debug.Log("spells count: " + spells.Count);
        spells.Insert(index, spell);
        NotifyListChanged();
    }
    public void Remove(int index)
    {
        spells.RemoveAt(index);
        NotifyListChanged();
    }

    /// <summary>
    /// 呪文リストの変更をすべてのリスナーに通知します。
    /// </summary>
    private void NotifyListChanged()
    {
        listener?.OnSpellListChanged(GetSpells());
    }

    /// <summary>
    /// リスナーを追加します。
    /// </summary>
    public void SetListener(ISpellListChangeListener listener)
    {
        this.listener = listener;
    }

    public float GetTotalCooldown()
    {
        float totalCooldown = 0f;
        foreach (var spell in spells)
        {
            if (spell != null)
            {
                totalCooldown += spell.cooldown;
            }
        }
        return totalCooldown;
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
    Steam,
    Wood,
    Dark,
}