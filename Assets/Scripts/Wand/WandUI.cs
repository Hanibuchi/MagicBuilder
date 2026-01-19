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
        // 呪文が一つもない場合のみ、唯一のSpacingUIを常にハイライトにする
        CreateSpacingUI(newSequence.Count, newSequence.Count == 0);
    }
    public void RebuildUI()
    {
        RebuildUI(fixedSpellBasesCashe, spellBasesCashe);
    }

    /// <summary>
    /// SpacingUIがPointerEnterイベントを受け取ったことを通知する。
    /// この通知を受け取ったWandUIは、他のSpacingUIのハイライトを解除する。
    /// </summary>
    /// <param name="enteredSpacing">Enterイベントが発生したSpacingUI。</param>
    /// <returns>呪文の追加が可能であればtrue。</returns>
    public void NotifySpellEntered(SpacingUI enteredSpacing)
    {
        // ハイライトの排他制御
        // ※ 追加可能でない場合でも、他のSpacingUIのハイライトは解除する必要があります。
        foreach (var element in uiElements)
        {
            SpacingUI spacing = element.GetComponent<SpacingUI>();
            if (spacing != null && spacing != enteredSpacing)
            {
                spacing.StopHighlight();
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
            element.GetComponent<SpacingUI>()?.SetExtendedTriggerActive(true);
        }
    }

    /// <summary>
    /// SpellUIのドラッグが終了したことを通知する。
    /// </summary>
    public void NotifySpellDragEnded()
    {
        // 全てのSpacingUIの拡張トリガーを非アクティブ化
        foreach (var element in uiElements)
        {
            element.GetComponent<SpacingUI>()?.SetExtendedTriggerActive(false);
        }
    }




    [SerializeField] Image wandImage;

    /// <summary>
    /// 指定されたWandTypeに基づいて杖の見た目を変更します。
    /// </summary>
    /// <param name="wandSprite">設定する杖のタイプ</param>
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