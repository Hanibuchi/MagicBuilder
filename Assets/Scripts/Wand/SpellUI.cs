// ファイル名: SpellUI.cs

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 杖に組み込まれている呪文のUI。ドラッグによる削除と並び替えの起点となる。
/// </summary>
public class SpellUI : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private int index;
    private WandUI wandUI;
    private SpellBase spellData;

    // UIパーツ
    public Image iconImage;

    public void Initialize(WandUI parentWandUI)
    {
        this.wandUI = parentWandUI;
    }

    public void SetData(SpellBase data)
    {
        this.spellData = data;
        if (iconImage != null && data.icon != null)
        {
            iconImage.sprite = data.icon;
        }
    }

    public SpellBase GetSpellData()
    {
        return spellData;
    }

    public void SetIndex(int newIndex)
    {
        this.index = newIndex;
        this.gameObject.name = $"SpellUI_{index}";
    }

    // --- ドラッグ処理 ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 1. ドラッグ開始時に、自身をCanvasの最前面に移動
        RectTransform root = DraggingSpellRootProvider.Instance.GetRootTransform();
        if (root != null)
        {
            // 現在の親から切り離し、ルートCanvasの子にする
            transform.SetParent(root, true);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        wandUI.NotifySpellRemoved(index);
        // 2. **WandUIにRebuildを促す**
        // ドロップに成功した場合はSpacingUIがNotifySpellAddedを呼んでいるため、
        // 失敗した場合のみRebuildを再度実行して、元の呪文を再配置する必要がある。
        // （ここではシンプルに、ドロップ先に移動しなかった場合は元の位置に戻す処理を
        // IWandEditor/WandUI側で実装することを想定し、一旦処理を省略）
    }
}