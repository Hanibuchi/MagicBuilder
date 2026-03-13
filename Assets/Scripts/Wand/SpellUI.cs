// ファイル名: SpellUI.cs

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// 杖に組み込まれている呪文のUI。ドラッグによる削除と並び替えの起点となる。
/// </summary>
public class SpellUI : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerClickHandler, ISpellCastListener
{
    public int index;
    public ISpellContainer spellContainerUI;
    private SpellBase spellData;
    private bool isActive = true;
    public bool IsUIActive => isActive;

    // UIパーツ
    public Image iconImage;
    [SerializeField] private GameObject newBadge;
    [SerializeField] private Animator animator;

    private static readonly int IsHighlightedHash = Animator.StringToHash("IsHighlighted");
    private static readonly int InvokeHash = Animator.StringToHash("Invoke");

    public void PlayCastAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(InvokeHash);
        }
    }

    public void Initialize(ISpellContainer parentWandUI)
    {
        activeColor = frame.color;
        this.spellContainerUI = parentWandUI;
        SetNewBadgeActive(false);
    }

    public void SetData(SpellBase data)
    {
        this.spellData = data;
        if (iconImage != null && data != null)
        {
            if (data.icon != null) iconImage.sprite = data.icon;
            iconImage.color = data.iconColor;
            iconImage.material = data.iconMaterial;
        }

        if (data != null)
        {
            SetColor(SpellCommonData.Instance.GetCategoryColor(data.category));
        }
    }

    /// <summary>
    /// ハイライト表示を切り替えます。
    /// Animatorの"IsHighlighted"パラメータ(bool)を操作します。
    /// </summary>
    /// <param name="highlight">ハイライトするかどうか</param>
    public void SetHighlight(bool highlight)
    {
        if (animator != null)
        {
            animator.SetBool(IsHighlightedHash, highlight);
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

    /// <summary>
    /// 新規取得バッジの表示・非表示を切り替える
    /// </summary>
    public void SetNewBadgeActive(bool active)
    {
        if (newBadge != null)
        {
            newBadge.SetActive(active);
        }
    }

    // --- ドラッグ処理 ---
    [SerializeField]
    private AudioClip dragStartClip; // ドラッグ開始時に再生するAudioClip
    [SerializeField] float dragStartClipVolume = 1.0f;
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isActive)
        {
            // EventSystemに対して、このオブジェクトはドラッグ不可能であることを伝える
            eventData.pointerDrag = null;
            return;
        }

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
        if (!isActive) return;
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isActive) return;
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
    /// ベースの色を設定します。
    /// </summary>
    /// <param name="color">設定する色</param>
    public void SetColor(Color color)
    {
        activeColor = color;
        if (isActive && frame != null)
        {
            frame.color = color;
        }
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
        this.isActive = active;
        if (active)
        {
            frame.color = activeColor;
            if (iconImage != null && spellData != null)
            {
                iconImage.color = spellData.iconColor;
                iconImage.material = spellData.iconMaterial;
            }
        }
        else
        {
            frame.color = disableColor;
            if (iconImage != null)
            {
                iconImage.color = Color.white;
                iconImage.material = disableMaterial;
            }
        }

        // ドラッグは制限するが、クリック（詳細表示）はできるようにRaycastTargetは常にtrueにする
        if (raycastTargetImage != null)
        {
            raycastTargetImage.raycastTarget = true;
        }
    }

    /// <summary>
    /// クリック（タップ）されたときに呪文の詳細説明を表示する。
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // SpellBaseデータがない場合は何もしない
        if (spellData == null) return;

        // 通常はドラッグ操作中はクリック判定を無視するが、
        // isActiveがfalseの時はドラッグ機能を無効化しているため、移動があってもクリックとして受け付ける
        if (eventData.dragging && isActive) return;

        // 新規取得フラグをクリア
        spellContainerUI?.NotifyPointerClick(index);
        SetNewBadgeActive(false);

        // シングルトン経由で詳細パネルの表示を開始
        if (SpellDescriptionUI.Instance != null)
        {
            // 自身の持つ呪文データを渡して表示アニメーションを開始
            SpellDescriptionUI.Instance.StartShowAnimation(spellData);
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
    /// クリックされたことを通知。
    /// </summary>
    /// <param name="index"></param>
    void NotifyPointerClick(int index) { }

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