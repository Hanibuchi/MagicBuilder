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
    public ISpellContainer spellContainerUI;
    private SpellBase spellData;

    // UIパーツ
    public Image iconImage;

    public void Initialize(ISpellContainer parentWandUI)
    {
        activeColor = frame.color;
        this.spellContainerUI = parentWandUI;
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
    [SerializeField]
    private AudioClip dragStartClip; // ドラッグ開始時に再生するAudioClip
    [SerializeField] float dragStartClipVolume = 1.0f;
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (SoundManager.Instance != null && dragStartClip != null)
            SoundManager.Instance.PlaySE(dragStartClip, dragStartClipVolume);

        dropSuccess = false;
        spellContainerUI.NotifyDragBegin(index);
        // 1. ドラッグ開始時に、自身をCanvasの最前面に移動
        RectTransform root = DraggingSpellRootProvider.Instance.GetRootTransform();
        if (root != null)
        {
            // 現在の親から切り離し、ルートCanvasの子にする
            transform.SetParent(root, true);
        }
        WandUIManager.Instance?.NotifySpellDragBeganToAll();
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // このメソッドはDragが成功した場合はなぜか呼ばれない。ただ、失敗した場合は呼ばれる必要がある。
        Debug.Log("OnEndDrag called");
        WandUIManager.Instance?.NotifySpellDragEndedToAll();
        StartCoroutine(CheckDropResultAndCleanUp());
    }
    private IEnumerator CheckDropResultAndCleanUp()
    {
        // 1フレーム待機することで、IDropHandler.OnDropの実行を保証する
        yield return null;

        if (!dropSuccess)
            spellContainerUI.RebuildUI();
    }
    private bool dropSuccess = false;    // ★ 追加: ドロップが成功したか
    public void NotifyDropSuccess()
    {
        WandUIManager.Instance?.NotifySpellDragEndedToAll();
        spellContainerUI.NotifySpellRemoved(index);
        dropSuccess = true;
    }

    /// <summary>
    /// 非アクティブのとき色を変えるFrame
    /// </summary>
    [SerializeField] Image frame;
    [SerializeField] Color disableColor;
    [SerializeField] Material disableMaterial;
    [SerializeField] Image raycastTargetImage;
    Color activeColor;
    public void SetActive(bool active)
    {
        if (active)
        {
            frame.color = activeColor;
            iconImage.material = null;
            raycastTargetImage.raycastTarget = true;
        }
        else
        {
            frame.color = disableColor;
            iconImage.material = disableMaterial;
            raycastTargetImage.raycastTarget = false;
        }
    }
}

// ファイル名: ISpellContainer.cs

/// <summary>
/// SpellUIを格納し、その操作結果（例：削除）を受け取るためのコンテナインターフェース。
/// WandUIやInventoryUIなどがこれを実装します。
/// </summary>
public interface ISpellContainer
{
    /// <summary>
    /// ドラッグが開始されたことを通知。SpellIndex用。
    /// </summary>
    /// <param name="index"></param>
    void NotifyDragBegin(int index) { }

    /// <summary>
    /// SpellUIがドラッグ＆ドロップによってコンテナから削除されたことを通知します。
    /// </summary>
    /// <param name="removedIndex">削除された呪文が元々持っていたインデックス。</param>
    void NotifySpellRemoved(int removedIndex);

    /// <summary>
    /// 呪文リストが変更された（削除・並び替え・追加など）後に、UIを再構築するよう要求します。
    /// </summary>
    void RebuildUI();
}