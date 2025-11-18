// ファイル名: SpellUI.cs

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// 杖に組み込まれている呪文のUI。ドラッグによる削除と並び替えの起点となる。
/// </summary>
public class SpellUI : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public int index;
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
        dropSuccess = false;
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
        StartCoroutine(CheckDropResultAndCleanUp());
    }
    private IEnumerator CheckDropResultAndCleanUp()
    {
        // 1フレーム待機することで、IDropHandler.OnDropの実行を保証する
        yield return null;

        if (!dropSuccess)
            wandUI.RebuildUI();
    }
    private bool dropSuccess = false;    // ★ 追加: ドロップが成功したか
    public void NotifyDropSuccess()
    {
        wandUI.NotifySpellRemoved(index);
        dropSuccess = true;
    }
}