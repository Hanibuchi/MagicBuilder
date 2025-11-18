using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// AttackManagerのWandデータとWandUIを接続し、
/// 呪文編集ロジック（IWandEditor）とリスト変更通知（ISpellListChangeListener）を処理します。
/// </summary>
public class WandController : IWandEditor, ISpellListChangeListener
{
    // このコントローラーが管理するWandデータ
    private Wand managedWand;
    // このコントローラーに対応するWandUI
    private WandUI wandUI;

    /// <summary>
    /// WandControllerの初期化を行います。
    /// </summary>
    /// <param name="wand">管理対象のWandデータ</param>
    /// <param name="ui">対応するWandUI</param>
    public void Initialize(Wand wand, WandUI ui)
    {
        managedWand = wand;
        wandUI = ui;

        // 1. Wandに自身をリスナーとして登録する (ISpellListChangeListener)
        managedWand.SetListener(this);

        // 2. WandUIに自身をエディターとして登録する (IWandEditor)
        wandUI.SetWandEditor(this);

        // 初期状態でUIを構築
        OnSpellListChanged(managedWand.GetSpells());
    }

    // --- IWandEditorの実装 ---

    /// <summary>
    /// WandUIから呼ばれ、Wandデータに呪文を追加する。
    /// </summary>
    public void AddSpell(int index, SpellBase spellToAdd)
    {
        // WandオブジェクトのAddメソッドを呼び出し、データ変更とリスナー通知を行う
        managedWand.Add(spellToAdd, index);
        // リスナー通知は managedWand.Add() の中で自動的に行われる
    }

    /// <summary>
    /// WandUIから呼ばれ、Wandデータから呪文を削除する。
    /// </summary>
    public void RemoveSpell(int index)
    {
        // WandオブジェクトのRemoveメソッドを呼び出し、データ変更とリスナー通知を行う
        managedWand.Remove(index);
        // リスナー通知は managedWand.Remove() の中で自動的に行われる
    }

    // --- ISpellListChangeListenerの実装 ---

    /// <summary>
    /// Wandデータが変更されたときにWandから呼ばれる。
    /// </summary>
    public void OnSpellListChanged(List<SpellBase> spells)
    {
        // WandUIを最新の呪文リストに基づいて再構築する
        wandUI.RebuildUI(spells.ToArray());
    }
}