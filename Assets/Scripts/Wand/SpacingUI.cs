// ファイル名: SpacingUI.cs

using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 呪文と呪文の間にあるスペース。呪文のドロップ（追加）を受け付ける。
/// </summary>
public class SpacingUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    private int index;
    private WandUI wandUI;
    private Animator animator;

    // Animatorのトリガー名
    private const string HighlightTrigger = "Highlight";
    private const string NormalTrigger = "Normal";

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Initialize(WandUI parentWandUI)
    {
        this.wandUI = parentWandUI;
    }

    public void SetIndex(int newIndex)
    {
        this.index = newIndex;
        this.gameObject.name = $"SpacingUI_{index}";
    }

    // --- ドロップ処理 ---

    public void OnDrop(PointerEventData eventData)
    {
        // ドロップされたオブジェクトがSpellUIであるかを確認
        SpellUI droppedSpellUI = eventData.pointerDrag.GetComponent<SpellUI>();

        if (droppedSpellUI != null)
        {
            SpellBase spellToAdd = droppedSpellUI.GetSpellData();
            droppedSpellUI.NotifyDropSuccess();
            if (droppedSpellUI.index < index && droppedSpellUI.spellContainerUI is WandUI spellWandUI && wandUI == spellWandUI) index--; // ドラッグ完了と同時に要素の削除と追加を行うとずれる。ドロップした時点でどの要素に追加するのかは決定されてしまい、その状態で要素が削除されるため、本来追加したい場所に追加されなくなる場合がある。この問題を回避するために、ここで補正をかけている。

            // 2. **アニメーションをリセット**
            if (animator != null)
            {
                animator.SetTrigger(NormalTrigger);
            }

            // 1. **WandUIに呪文の追加を通知**
            // NotifySpellAdded内でwandEditor.AddSpell(index, spellToAdd)が呼ばれ、
            // その後WandUIはRebuildUI()を実行し、UIを更新する。
            wandUI.NotifySpellAdded(index, spellToAdd);
        }
    }

    // --- ドロップオーバー時のアニメーション処理 ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        // ドラッグ中のオブジェクトがある場合のみアニメーション再生
        if (eventData.pointerDrag != null && eventData.pointerDrag.GetComponent<SpellUI>() != null)
        {
            if (animator != null)
            {
                animator.ResetTrigger(NormalTrigger);
                animator.SetTrigger(HighlightTrigger);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (animator != null)
        {
            animator.SetTrigger(NormalTrigger);
            animator.ResetTrigger(HighlightTrigger);
        }
    }
}