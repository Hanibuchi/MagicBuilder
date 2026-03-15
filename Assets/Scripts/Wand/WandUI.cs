// ファイル名: WandUI.cs

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
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
    private SpellUI currentlyDraggedSpellUI = null;

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
    /// UIに登録されている全呪文のISpellCastListenerのリストを取得します。
    /// 固定呪文が先頭に配置され、その後にスロット内の通常呪文が順番に並びます。
    /// </summary>
    /// <returns>ISpellCastListenerのリスト</returns>
    public List<ISpellCastListener> GetSpellCastListeners()
    {
        List<ISpellCastListener> listeners = new List<ISpellCastListener>();
        foreach (var element in uiElements)
        {
            if (element != null && element.TryGetComponent<ISpellCastListener>(out var listener))
            {
                listeners.Add(listener);
            }
        }
        return listeners;
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

        // ホバー解除時は現在の総クールタイム表示に戻す
        List<SpellBase> currentSpells = new List<SpellBase>(fixedSpellBasesCashe);
        currentSpells.AddRange(spellBasesCashe);
        
        // ドラッグ中の呪文がこの杖から移動している場合は総クールタイムから除外
        if (currentlyDraggedSpellUI != null && currentlyDraggedSpellUI.spellContainerUI as WandUI == this)
        {
            currentSpells.Remove(currentlyDraggedSpellUI.GetSpellData());
        }

        float totalCooldown = Wand.CalculateTotalCooldown(currentSpells);
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.SetDisplayCooldownGradual(totalCooldown);
        }
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

    /// <summary>
    /// シミュレーション用の呪文情報保持クラス
    /// </summary>
    private class TmpSpell
    {
        public SpellBase Data;
        public SpellUI UI;
    }

    private void HighlightNextSpells(SpacingUI enteredSpacing, SpellUI draggedSpellUI)
    {
        if (enteredSpacing == null || draggedSpellUI == null) return;
        SpellBase draggedSpell = draggedSpellUI.GetSpellData();
        if (draggedSpell == null) return;

        // 1. 現在の全ての呪文を TmpSpell のリスト（mockList）に変換
        List<TmpSpell> mockList = new List<TmpSpell>();
        int fixedCount = fixedSpellBasesCashe.Count;

        // 固定呪文
        for (int i = 0; i < fixedSpellBasesCashe.Count; i++)
        {
            mockList.Add(new TmpSpell { Data = fixedSpellBasesCashe[i], UI = uiElements[i].GetComponent<SpellUI>() });
        }

        // 通常呪文（SpacingUIと交互に並んでいるため index*2+1 でアクセス）
        for (int i = 0; i < spellBasesCashe.Count; i++)
        {
            int uiIndex = fixedCount + i * 2 + 1;
            if (uiIndex < uiElements.Count)
            {
                mockList.Add(new TmpSpell { Data = spellBasesCashe[i], UI = uiElements[uiIndex].GetComponent<SpellUI>() });
            }
        }

        // 2. シミュレーション: 移動元がこの杖自身なら、一旦元の位置から削除
        bool isMovingFromSelf = (draggedSpellUI.spellContainerUI as WandUI == this);
        int finalAbsInsertIndex = fixedCount + enteredSpacing.Index;

        if (isMovingFromSelf)
        {
            int oldAbsIndex = fixedCount + draggedSpellUI.index;
            if (oldAbsIndex >= 0 && oldAbsIndex < mockList.Count)
            {
                mockList.RemoveAt(oldAbsIndex);
                // 削除によって挿入予定の絶対インデックスがずれる場合の補正
                if (oldAbsIndex < finalAbsInsertIndex)
                {
                    finalAbsInsertIndex--;
                }
            }
        }

        // 3. 予定位置にドラッグ中の呪文を挿入
        TmpSpell draggedTmp = new TmpSpell { Data = draggedSpell, UI = draggedSpellUI };
        mockList.Insert(finalAbsInsertIndex, draggedTmp);

        // 4. PreProcessに対応させる
        List<SpellBase> simulatedSequence = mockList.Select(x => x.Data).ToList();
        List<SpellBase> processedSequence = AttackManager.Instance != null
            ? AttackManager.Instance.ProcessWandSpellsBeforeFire(simulatedSequence)
            : simulatedSequence;

        // 5. 加工後のリストを TmpSpell のリストに再変換（対応するUIを紐付け直す）
        List<TmpSpell> remainingSource = new List<TmpSpell>(mockList);
        List<TmpSpell> processedMockList = new List<TmpSpell>();

        for (int i = processedSequence.Count - 1; i >= 0; i--)
        {
            var sData = processedSequence[i];
            TmpSpell match = null;
            if (remainingSource.Count >= 1 && remainingSource[^1]?.Data == sData)
                match = remainingSource[^1];
            if (match != null)
            {
                processedMockList.Insert(0, match);
                remainingSource.RemoveAt(remainingSource.Count - 1); // 1対1の対応を維持するため、一度使ったソースは除外
            }
            else
            {
                processedMockList.Insert(0, new TmpSpell { Data = sData, UI = null });
            }
        }

        // 6. 加工後のリスト内でドラッグ中の呪文がどこに移動したかを探す
        int processedIndex = processedMockList.IndexOf(draggedTmp);
        if (processedIndex == -1) return;

        // 7. GetNextSpellOffsets を使用して次に呼び出される呪文の相対インデックスを取得
        int[] offsets = draggedSpell.GetNextSpellOffsets(processedSequence, processedIndex);

        if (offsets == null) return;

        // 8. 取得したオフセットに基づき、対応する SpellUI をハイライト
        foreach (int offset in offsets)
        {
            int targetIndex = processedIndex + offset;
            if (targetIndex >= 0 && targetIndex < processedMockList.Count)
            {
                processedMockList[targetIndex].UI?.SetHighlight(true);
            }
        }

        // 9. シミュレーション構成による総クールタイムをUIに表示
        if (CooldownManager.Instance != null && simulatedSequence != null)
        {
            float simulatedTotalCooldown = Wand.CalculateTotalCooldown(simulatedSequence);
            CooldownManager.Instance.SetDisplayCooldownGradual(simulatedTotalCooldown);
        }
    }

    // --- SpellUIのドラッグ開始/終了を管理する機能の追加 ---

    /// <summary>
    /// SpellUIがドラッグを開始したことを通知する。
    /// </summary>
    public void NotifySpellDragBegan(SpellUI draggedSpellUI)
    {
        this.currentlyDraggedSpellUI = draggedSpellUI;

        // 現在の構成による総クールタイムを計算し、一瞬で表示する
        if (CooldownManager.Instance != null && AttackManager.Instance != null)
        {
            var wand = AttackManager.Instance.GetCurrentWand();
            if (wand != null)
            {
                List<SpellBase> simulatedSequence = new List<SpellBase>(wand.AllSpells);
                if (draggedSpellUI != null && draggedSpellUI.spellContainerUI as WandUI == this)
                {
                    simulatedSequence.Remove(draggedSpellUI.GetSpellData());
                }
                float totalCooldown = Wand.CalculateTotalCooldown(simulatedSequence);

                // ドラッグ開始直前までの実際のクールタイムから、総クールタイムの表示に向かって徐々に変化（あるいはInstant）
                CooldownManager.Instance.SetDisplayCooldownInstant(totalCooldown);
            }
        }

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
        this.currentlyDraggedSpellUI = null;

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

        // ドラッグ終了時は内部的な元のクールタイム表示に戻す
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.ResetDisplayToActualCooldown();
        }
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