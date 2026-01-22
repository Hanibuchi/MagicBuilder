// ファイル名: WandUI.cs

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.U2D.Animation;
using UnityEngine.UI;

public class WandUI : MonoBehaviour, ISpellContainer
{
    // 実装クラスのインターフェース
    private IWandEditor wandEditor;

    public GameObject spacingUIPrefab;

    // 呪文UIとスペーシングUIを格納するリスト（交互に並ぶ）
    private List<GameObject> uiElements = new List<GameObject>();

    private List<SpellBase> fixedSpellBasesCashe = new List<SpellBase>();
    private List<SpellBase> spellBasesCashe = new List<SpellBase>();

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

        // リストに挿入 (UIの並び順は Fixed..., Spacing, Spell, Spacing, Spell, Spacing, ...) となる
        int fixedCount = fixedSpellBasesCashe.Count;
        uiElements.Insert(fixedCount + index * 2 + 1, spellUI.gameObject);
    }

    private void CreateFixedSpellUI(SpellBase spell)
    {
        SpellUI spellUI = spell.CreateUI();
        if (spellUI != null)
        {
            spellUI.transform.SetParent(spellFrame);
            spellUI.Initialize(this);
            spellUI.SetActive(false); // 固定呪文は移動・削除不可
        }

        // 固定呪文はリストの先頭付近（他の固定呪文の後）に追加
        uiElements.Add(spellUI.gameObject);
    }

    private void CreateSpacingUI(int index, bool isAlwaysHighlight = false)
    {
        GameObject spacingObj = Instantiate(spacingUIPrefab, spellFrame);
        SpacingUI spacingUI = spacingObj.GetComponent<SpacingUI>();
        if (spacingUI != null)
        {
            spacingUI.SetIndex(index);
            spacingUI.Initialize(this); // このWandUI自身への参照を渡す
            spacingUI.SetAlwaysHighlight(isAlwaysHighlight);
        }

        // リストに挿入 (SpacingUIはSpellUIの前後に配置される)
        int fixedCount = fixedSpellBasesCashe.Count;
        int insertionIndex = fixedCount + index * 2;

        uiElements.Insert(insertionIndex, spacingObj);
    }

    /// <summary>
    /// SpacingUIからの要求に基づき、ドラッグ中の呪文を挿入可能か判定する。
    /// </summary>
    /// <param name="isMovingFromSelf">挿入を試みている呪文が、もともとこのWandUIに属していたかどうか。</param>
    /// <returns>挿入可能であればtrue。</returns>
    public bool CanDropSpell(bool isMovingFromSelf)
    {
        if (wandEditor == null)
        {
            Debug.LogError("WandEditorが設定されていません。挿入判定ができません。");
            return false;
        }
        // Editor側の新しい判定メソッドを呼び出す
        return wandEditor.CanAddSpell(isMovingFromSelf);
    }

    /// <summary>
    /// SpellUI/SpacingUIから呪文の追加リクエストを受け取る。
    /// </summary>
    public void NotifySpellAdded(int index, SpellBase spell)
    {
        if (wandEditor != null)
        {
            Debug.Log($"呪文が追加されました。インデックス: {index} | 呪文タイプ: {spell.spellName}");
            // 編集ロジックに通知
            wandEditor.AddSpell(index, spell);
        }
        else
            Debug.LogError("WandEditorが設定されていません。呪文の追加ができません。");
    }

    /// <summary>
    /// SpellUIから呪文の削除リクエストを受け取る。
    /// </summary>
    public void NotifySpellRemoved(int index)
    {
        if (wandEditor != null)
        {
            // uiElements.RemoveAt(2 * index + 1); // ドラッグ中の呪文UIを削除しないよう、リストから外す。
            // 編集ロジックに通知
            wandEditor.RemoveSpell(index);
        }
    }

    // UI要素をクリアし、現在の呪文の並びに基づいて再生成する
    public void RebuildUI(List<SpellBase> fixedSequence, List<SpellBase> newSequence)
    {
        fixedSpellBasesCashe = fixedSequence;
        spellBasesCashe = newSequence;
        // 全てのUI要素をクリア
        foreach (var element in uiElements)
        {
            Destroy(element);
        }
        uiElements.Clear();

        // 1. 固定呪文を生成
        foreach (var spell in fixedSequence)
        {
            CreateFixedSpellUI(spell);
        }

        // 2. 通常の呪文とスペーシングを交互に生成
        for (int i = 0; i < newSequence.Count; i++)
        {
            CreateSpacingUI(i, false);
            CreateSpellUI(i, newSequence[i]);
        }

        // 最後のSpacingUIの前に空間を作るための空オブジェクトを挿入
        GameObject spacer = new GameObject("Spacer", typeof(RectTransform));
        spacer.transform.SetParent(spellFrame, false);
        if (spacer.TryGetComponent<RectTransform>(out var spacerRect))
        {
            spacerRect.sizeDelta = new Vector2(0, spacerRect.sizeDelta.y);
        }
        uiElements.Add(spacer);

        CreateSpacingUI(newSequence.Count, false);
    }
    public void RebuildUI()
    {
        RebuildUI(fixedSpellBasesCashe, spellBasesCashe);
    }

    /// <summary>
    /// SpacingUIがPointerEnterイベントを受け取ったことを通知する。
    /// この通知を受け取ったWandUIは、他のSpacingUIのハイライトを解除し、
    /// 挿入時に連鎖する呪文をハイライトする。
    /// </summary>
    /// <param name="enteredSpacing">Enterイベントが発生したSpacingUI。</param>
    /// <param name="draggedSpellUI">ドラッグ中のSpellUI。</param>
    public void NotifySpellEntered(SpacingUI enteredSpacing, SpellUI draggedSpellUI)
    {
        // 1. SpacingUIのハイライト排他制御
        foreach (var element in uiElements)
        {
            SpacingUI spacing = element.GetComponent<SpacingUI>();
            if (spacing != null && spacing != enteredSpacing)
            {
                spacing.StopHighlight();
            }
        }

        // 2. SpellUIのハイライトをリセット
        ResetAllSpellHighlights();

        // 3. 次に呼ばれる呪文の予測とハイライト
        HighlightNextSpells(enteredSpacing, draggedSpellUI);
    }

    /// <summary>
    /// SpacingUIからPointerExitイベントを受け取ったことを通知する。
    /// </summary>
    public void NotifySpellExited()
    {
        ResetAllSpellHighlights();
    }

    /// <summary>
    /// 全てのSpellUIのハイライトを解除する。
    /// </summary>
    private void ResetAllSpellHighlights()
    {
        foreach (var element in uiElements)
        {
            if (element != null && element.TryGetComponent<SpellUI>(out var spellUI))
            {
                spellUI.SetHighlight(false);
            }
        }
    }

    private void HighlightNextSpells(SpacingUI enteredSpacing, SpellUI draggedSpellUI)
    {
        if (enteredSpacing == null || draggedSpellUI == null) return;
        SpellBase draggedSpell = draggedSpellUI.GetSpellData();
        if (draggedSpell == null) return;

        int fixedCount = fixedSpellBasesCashe.Count;

        // 1. 全体のSpellBaseリストを構築
        List<SpellBase> fullList = new List<SpellBase>();
        fullList.AddRange(fixedSpellBasesCashe);
        fullList.AddRange(spellBasesCashe);

        // 2. ドラッグ中の呪文がこの杖のものか確認
        bool isMovingFromSelf = (draggedSpellUI.spellContainerUI as WandUI == this);
        int oldAbsIndex = isMovingFromSelf ? (fixedCount + draggedSpellUI.index) : -1;

        // 挿入位置（変数スペルリスト内での位置）
        int insertVarIndex = enteredSpacing.Index;
        int absInsertIndex = fixedCount + insertVarIndex;

        // シミュレーション用のリスト作成
        List<SpellBase> mockList = new List<SpellBase>(fullList);
        int finalAbsInsertIndex = absInsertIndex;

        if (isMovingFromSelf)
        {
            if (oldAbsIndex >= 0 && oldAbsIndex < mockList.Count)
            {
                mockList.RemoveAt(oldAbsIndex);
                if (oldAbsIndex < absInsertIndex)
                {
                    finalAbsInsertIndex = absInsertIndex - 1;
                }
            }
        }
        mockList.Insert(finalAbsInsertIndex, draggedSpell);

        // 3. 次に呼ばれる呪文のオフセットを取得
        int[] offsets = draggedSpell.GetNextSpellOffsets(mockList, finalAbsInsertIndex);
        if (offsets == null || offsets.Length == 0) return;

        // 4. ハイライト対象のUIを特定してハイライト
        foreach (int offset in offsets)
        {
            int mockIndex = finalAbsInsertIndex + offset;
            if (mockIndex < 0 || mockIndex >= mockList.Count) continue;

            if (mockIndex == finalAbsInsertIndex)
            {
                // ドラッグ中の呪文自身
                draggedSpellUI.SetHighlight(true);
                continue;
            }

            // mockIndexから現在の絶対インデックスに逆写像
            int tempIndex = mockIndex;
            // Undo Insert
            if (tempIndex > finalAbsInsertIndex) tempIndex--;
            // (tempIndex == finalAbsInsertIndex は既に自身として除外済み)

            // Undo Remove
            int origAbsIndex = tempIndex;
            if (isMovingFromSelf && origAbsIndex >= oldAbsIndex)
            {
                origAbsIndex++;
            }

            // UI要素の特定
            if (origAbsIndex < fixedCount)
            {
                // 固定呪文
                if (origAbsIndex >= 0 && origAbsIndex < uiElements.Count)
                {
                    var sUI = uiElements[origAbsIndex].GetComponent<SpellUI>();
                    if (sUI != null) sUI.SetHighlight(true);
                }
            }
            else
            {
                // 通常呪文
                int varIndex = origAbsIndex - fixedCount;
                int uiIndex = fixedCount + varIndex * 2 + 1;
                if (uiIndex >= 0 && uiIndex < uiElements.Count)
                {
                    var sUI = uiElements[uiIndex].GetComponent<SpellUI>();
                    if (sUI != null) sUI.SetHighlight(true);
                }
            }
        }
    }

    // --- SpellUIのドラッグ開始/終了を管理する機能の追加 ---

    /// <summary>
    /// SpellUIがドラッグを開始したことを通知する。
    /// </summary>
    public void NotifySpellDragBegan()
    {
        // 全てのSpacingUIの拡張トリガーをアクティブ化
        foreach (var element in uiElements)
        {
            SpacingUI spacing = element.GetComponent<SpacingUI>();
            if (spacing != null)
            {
                spacing.SetExtendedTriggerActive(true);
            }
        }

        // 最後のSpacingUIをハイライト（空きがある場合のみ）
        if (CanDropSpell(false))
        {
            for (int i = uiElements.Count - 1; i >= 0; i--)
            {
                SpacingUI lastSpacing = uiElements[i].GetComponent<SpacingUI>();
                if (lastSpacing != null)
                {
                    lastSpacing.SetAlwaysHighlight(true);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// SpellUIのドラッグが終了したことを通知する。
    /// </summary>
    public void NotifySpellDragEnded()
    {
        // 全てのSpacingUIの拡張トリガーを非アクティブ化し、ハイライトを強制解除
        foreach (var element in uiElements)
        {
            SpacingUI spacing = element.GetComponent<SpacingUI>();
            if (spacing != null)
            {
                spacing.SetExtendedTriggerActive(false);
                spacing.SetAlwaysHighlight(false);
            }
        }
        ResetAllSpellHighlights(); // 追加
    }




    [SerializeField] Image wandImage;

    /// <summary>
    /// 杖の見た目を変更します。
    /// </summary>
    /// <param name="wandSprite">設定するスプライト</param>
    public void ChangeAppearance(Sprite wandSprite)
    {
        wandImage.sprite = wandSprite;
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

    /// <summary>
    /// 指定された呪文をこの杖に追加できるかどうかを判定する。
    /// </summary>
    /// <param name="isMovingFromSelf">追加を試みている呪文が、もともとこの杖に属していたか (移動) どうか。</param>
    /// <returns>追加可能であればtrue、そうでなければfalse。</returns>
    bool CanAddSpell(bool isMovingFromSelf);
}