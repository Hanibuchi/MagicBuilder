// ファイル名: WandUI.cs

using UnityEngine;
using System.Collections.Generic;

public class WandUI : MonoBehaviour
{
    // 実装クラスのインターフェース
    private IWandEditor wandEditor;

    public GameObject spacingUIPrefab;

    // 呪文UIとスペーシングUIを格納するリスト（交互に並ぶ）
    private List<GameObject> uiElements = new List<GameObject>();

    void Awake()
    {
        // 初期状態では空の杖を表現するために、スペーシングUIを一つ配置する
        CreateSpacingUI(0);
    }

    public void SetWandEditor(IWandEditor wandEditor)
    {
        this.wandEditor = wandEditor;
    }

    [SerializeField] RectTransform spellFrame;
    private void CreateSpellUI(int index, SpellBase spell)
    {
        SpellUI spellUI = spell.CreateUI();
        if (spellUI != null)
        {
            spellUI.transform.SetParent(spellFrame);
            spellUI.SetIndex(index);
            spellUI.Initialize(this); // このWandUI自身への参照を渡す
        }

        // リストに挿入 (UIの並び順は (Spacing, Spell, Spacing, Spell, Spacing, ...) となる)
        uiElements.Insert(index * 2 + 1, spellUI.gameObject);
    }

    private void CreateSpacingUI(int index)
    {
        GameObject spacingObj = Instantiate(spacingUIPrefab, spellFrame);
        SpacingUI spacingUI = spacingObj.GetComponent<SpacingUI>();
        if (spacingUI != null)
        {
            spacingUI.SetIndex(index);
            spacingUI.Initialize(this); // このWandUI自身への参照を渡す
        }

        // リストに挿入 (SpacingUIはSpellUIの前後に配置される)
        int insertionIndex = index * 2;

        uiElements.Insert(insertionIndex, spacingObj);
    }

    /// <summary>
    /// SpellUI/SpacingUIから呪文の追加リクエストを受け取る。
    /// </summary>
    public void NotifySpellAdded(int index, SpellBase spell)
    {
        if (wandEditor != null)
        {
            // 編集ロジックに通知
            wandEditor.AddSpell(index, spell);
        }
    }

    /// <summary>
    /// SpellUIから呪文の削除リクエストを受け取る。
    /// </summary>
    public void NotifySpellRemoved(int index)
    {
        if (wandEditor != null)
        {
            uiElements.RemoveAt(2 * index + 1); // ドラッグ中の呪文UIを削除しないよう、リストから外す。
            // 編集ロジックに通知
            wandEditor.RemoveSpell(index);
        }
    }

    // UI要素をクリアし、現在の呪文の並びに基づいて再生成する
    public void RebuildUI(SpellBase[] newSequence)
    {
        // 全てのUI要素をクリア
        foreach (var element in uiElements)
        {
            Destroy(element);
        }
        uiElements.Clear();

        // スペーシングの数 = 呪文の数 + 1
        // 例：呪文が3つあれば、SpacingUIは4つ
        // 実際の実装では、IWandEditorから最新の呪文リストを取得し、それに基づいて
        // CreateSpacingUIとCreateSpellUIを交互に呼び出す

        // 例：現在の呪文リストがwandEditor.GetCurrentSpells()で取得できる場合
        for (int i = 0; i < newSequence.Length; i++)
        {
            CreateSpacingUI(i);
            CreateSpellUI(i, newSequence[i]);
        }
        CreateSpacingUI(newSequence.Length);
    }
}


// ファイル名: IWandEditor.cs

/// <summary>
/// WandUIからのユーザー操作通知を受け取るインターフェース。
/// </summary>
public interface IWandEditor
{
    /// <summary>
    /// 指定された位置に新しい呪文を追加する。
    /// </summary>
    /// <param name="index">呪文を追加する位置（0から始まる）。</param>
    /// <param name="spellToAdd">追加するSpellBaseオブジェクト。</param>
    void AddSpell(int index, SpellBase spellToAdd);

    /// <summary>
    /// 指定された位置の呪文を削除する。
    /// </summary>
    /// <param name="index">削除する呪文の位置（0から始まる）。</param>
    void RemoveSpell(int index);
}