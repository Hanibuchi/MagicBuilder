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

    [Tooltip("この杖に追加できる呪文の最大数。")]
    public int maxSpellCount = 12; // デフォルト値として12を設定

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

    /// <summary>
    /// 指定された呪文をこの杖に挿入できるかどうかを判定する。
    /// </summary>
    /// <param name="isMovingFromSelf">挿入を試みている呪文が、もともとこの杖に属していたか (移動) どうか。</param>
    /// <returns>挿入可能であればtrue、そうでなければfalse。</returns>
    public bool CanAddSpell(bool isMovingFromSelf)
    {
        // 1. 杖の中での移動（配置換え）の場合
        if (isMovingFromSelf)
        {
            // 呪文の総数は変わらないため、常に挿入可能
            return true;
        }

        // 2. 新規の呪文を追加する場合
        // 現在の呪文数と最大許容数を比較
        int currentCount = spells.Count;

        if (currentCount < maxSpellCount)
        {
            // 最大スロット数に達していないため、追加可能
            return true;
        }
        else
        {
            // 最大スロット数に達しているため、追加不可
            return false;
        }
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
    Water,
    Ice,
    Fire,
    Wood,
}