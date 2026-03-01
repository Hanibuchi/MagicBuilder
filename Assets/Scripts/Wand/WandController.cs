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
        wandUI.ChangeAppearance(managedWand.wandSprite);

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
        // Debug.Log("OnSpellListChanged called: " + spells);
        if (AttackManager.Instance.GetCurrentWand() == managedWand)
            SpellInventory.Instance.DeactivateSpellUIs(spells);
        // WandUIを最新の呪文リストに基づいて再構築する
        wandUI.RebuildUI(managedWand.fixedSpells, spells);
    }

    public bool CanAddSpell(bool isMovingFromSelf)
    {
        if (managedWand == null)
        {
            Debug.LogError("管理対象のWandデータが設定されていません。");
            return false;
        }

        // 判定ロジックはWandデータ（モデル）側で行う
        bool canInsert = managedWand.CanAddSpell(isMovingFromSelf);

        if (!canInsert)
        {
            // 新規追加失敗の場合のみログを出す (UIへのフィードバックに利用)
            Debug.Log($"呪文追加不可: 杖 '{managedWand.type}' は最大呪文数 ({managedWand.maxSpellCount}) に達しています。");
        }

        return canInsert;
    }
}