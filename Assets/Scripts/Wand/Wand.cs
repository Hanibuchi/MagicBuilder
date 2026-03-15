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
    [Tooltip("この杖の名称")]
    public string wandName;

    [Tooltip("この杖の説明文")]
    [TextArea]
    public string description;

    [Tooltip("この杖の種類")]
    public WandType type = WandType.Default;

    [Tooltip("演出時に使用するスプライト")]
    public Sprite presentationSprite;

    [Tooltip("杖の見た目として使用するスプライト")]
    public Sprite wandSprite;

    [Tooltip("この杖に追加できる呪文の最大数。")]
    public int maxSpellCount = 12; // デフォルト値として12を設定

    [Header("デフォルト呪文")]
    [Tooltip("この杖に固定でセットされている、外すことのできない呪文。")]
    [SerializeReference] public List<SpellBase> fixedSpells = new List<SpellBase>();

    [Header("呪文スロット")]
    [Tooltip("この杖にセットされている呪文の配列。")]
    [SerializeReference] public List<SpellBase> spells = new List<SpellBase>();

    /// <summary>
    /// 固定呪文と通常の呪文を合わせた、この杖のすべての呪文リストを返します。
    /// </summary>
    public List<SpellBase> AllSpells
    {
        get
        {
            List<SpellBase> all = new List<SpellBase>(fixedSpells);
            all.AddRange(spells);
            return all;
        }
    }

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
    /// 杖の状態をリセットします（セットされている通常の呪文をすべてクリアします）。
    /// </summary>
    public void Reset()
    {
        spells.Clear();
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
        return CalculateTotalCooldown(AllSpells);
    }

    /// <summary>
    /// 指定された呪文リストの合計クールタイムを計算します。
    /// </summary>
    /// <param name="spellList">計算対象の呪文リスト</param>
    /// <returns>合計クールタイム（0以上）</returns>
    public static float CalculateTotalCooldown(List<SpellBase> spellList)
    {
        float totalCooldown = 0f;
        // まず全呪文の基本クールタイムを合計する
        foreach (var spell in spellList)
        {
            if (spell != null)
            {
                totalCooldown += spell.cooldown;
            }
        }

        // 次に各呪文の修正（倍率や加減算）を適用する
        foreach (var spell in spellList)
        {
            if (spell != null)
            {
                totalCooldown = spell.ModifyCooldown(totalCooldown);
            }
        }

        return Mathf.Max(0f, totalCooldown);
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
        // 現在の合計呪文数（固定呪文を含む）と最大許容数を比較
        int currentCount = spells.Count + fixedSpells.Count;

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
    Sky,
    Magic,
}