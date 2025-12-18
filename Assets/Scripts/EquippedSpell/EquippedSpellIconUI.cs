// EquippedSpellIconUI.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshProUGUIを使用
using UnityEngine.EventSystems; // ドラッグ操作インターフェース
using System.Collections;

/// <summary>
/// 持ち込み呪文選択画面で使用する、個々の呪文アイコンのUI。
/// 保持呪文リスト (Inventory) や持ち込みスロット (Equipped Slot) のどちらにも配置され、
/// ドラッグ操作の起点となり、選択・装備・解除・並び替えを担う。
/// </summary>
public class EquippedSpellIconUI : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerClickHandler, IDropHandler
{
    // --- インスペクター設定項目 ---

    [Header("UI コンポーネント")]
    [Tooltip("呪文アイコンを表示するImage")]
    [SerializeField] private Image iconImage;

    [Tooltip("UIの枠/背景を表示するImage")]
    [SerializeField] private Image frameImage;

    [Tooltip("所持数を表示するTextMeshProUGUI")]
    [SerializeField] private TextMeshProUGUI countText;

    [Tooltip("レイキャストターゲットとして機能するImage (非アクティブ時に制御)")]
    [SerializeField] private Image raycastTargetImage;


    [Header("非アクティブ設定")]
    [Tooltip("ロック時のフレームの色")]
    [SerializeField] private Color lockColor = Color.gray;

    [Tooltip("非所持のアイコンに適用するマテリアル")]
    [SerializeField] private Material disableMaterial;
    [SerializeField] private Sprite lockSprite;

    // --- 内部データ ---

    private int _slotIndex = -1; // 持ち込みスロットの番号。保持リストにある場合は -1
    private SpellBase _spellData; // このUIが表す呪文のデータ
    private Color _activeColor; // 通常時のフレームの色
    private bool _isEquippedSlot = false; // 持ち込みスロットとして機能しているか (保持リストのUIと区別)
    public bool IsEquippedSlot => _isEquippedSlot;

    // 外部通知用（登録できるのは1つのみ）
    private IEquippedSpellIconUIObserver _observer;

    // --- 公開メソッド ---

    /// <summary>
    /// 変更通知を受け取るオブザーバーをセットします。（1つのみ登録可能）
    /// </summary>
    public void SetObserver(IEquippedSpellIconUIObserver observer)
    {
        _observer = observer;
    }

    /// <summary>
    /// このUIが表す呪文のデータを設定し、アイコンを更新します。
    /// </summary>
    /// <param name="data">設定する呪文のScriptableObject</param>
    public void SetData(SpellBase data)
    {
        _spellData = data;
    }

    /// <summary>
    /// このUIが表す呪文のデータを取得します。
    /// </summary>
    public SpellBase GetSpellData()
    {
        return _spellData;
    }

    /// <summary>
    /// このUIが持ち込みスロットに配置された際のインデックスを設定します。
    /// </summary>
    /// <param name="newIndex">スロット番号。保持リストの場合は -1 を設定しても良い。</param>
    /// <param name="isEquipped">持ち込みスロットとして機能するかどうか</param>
    public void SetSlotIndex(int newIndex, bool isEquipped)
    {
        _slotIndex = newIndex;
        _isEquippedSlot = isEquipped;
    }

    public void SetIcon(bool active)
    {
        iconImage.sprite = active ? _spellData?.icon : lockSprite;
    }
    public void SetShowDescription(bool active)
    {
        showDescription = active;
    }

    bool frameActive = true; // デフォルトでアクティブな状態であることを想定
    public void SetFrameColor(bool active)
    {
        if (frameImage == null) return;
        if (active)
        {
            if (frameActive) return;
            frameActive = true;
            frameImage.color = _activeColor;
            iconImage.material = null;
        }
        else
        {
            if (!frameActive) return;
            frameActive = false;
            _activeColor = frameImage.color;
            frameImage.color = lockColor;
            iconImage.material = disableMaterial;
        }
    }


    private bool _canDrag = true; // ドラッグ可能フラグを追加
    /// <summary>
    /// このアイコンのドラッグ操作の有効/無効を切り替えます。
    /// クリック判定（詳細表示など）には影響しません。
    /// </summary>
    /// <param name="active">ドラッグを許可するかどうか</param>
    public void SetDrag(bool active)
    {
        _canDrag = active;
    }

    int availableCount = -1;
    public int AvailableCount => availableCount;
    /// <summary>
    /// 持ち込みに使用可能な残り所持数を表示します。-1で非表示
    /// </summary>
    /// <param name="count">表示する残り所持数</param>
    public void SetAvailableCount(int count)
    {
        availableCount = count;
        if (countText != null)
        {
            // 持ち込みスロットの場合は所持数を表示しないか、空にする
            if (_isEquippedSlot)
            {
                countText.text = "";
                return;
            }

            // 保持リストの場合は所持数を表示
            countText.text = count >= 2 || count == 0 ? count.ToString() : "";
        }
    }

    // --- ドラッグ処理の実装 ---

    [SerializeField]
    private AudioClip dragStartClip; // ドラッグ開始時に再生するAudioClip
    private bool _dropSuccess = false; // ドロップが成功したか

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_spellData == null) return; // 呪文データがない場合はドラッグ不可

        if (!_canDrag)
        {
            // ドラッグ不可なら、このイベント自体を「なかったこと」にする
            eventData.pointerDrag = null;
            return;
        }

        if (SoundManager.Instance != null && dragStartClip != null)
            SoundManager.Instance.PlaySE(dragStartClip);

        _dropSuccess = false;

        // 1. ドラッグ開始通知
        if (_isEquippedSlot)
        {
            // 持ち込みスロットからドラッグ開始 (並び替え・解除)
            _observer?.NotifyEquippedDragBegin(_spellData, _slotIndex);
        }
        else
        {
            // 保持リストからドラッグ開始 (装備)
            _observer?.NotifyHoldListDragBegin(_spellData);
        }

        SetAvailableCount(-1);
        raycastTargetImage.raycastTarget = false;

        // 2. 自身をCanvasの最前面に移動
        RectTransform root = DraggingSpellRootProvider.Instance?.GetRootTransform();
        if (root != null)
        {
            // 現在の親から切り離し、ルートCanvasの子にする（位置を維持したまま）
            transform.SetParent(root, true);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 3. マウス/タッチの位置に追従
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 4. ドロップ結果の確認とクリーンアップ
        // OnDropの実行を待つため、1フレーム待機
        StartCoroutine(CheckDropResultAndCleanUp());
    }

    private IEnumerator CheckDropResultAndCleanUp()
    {
        // 1フレーム待機することで、IDropHandler.OnDropの実行を保証する
        yield return null;

        // ドロップが成功しなかった場合
        if (!_dropSuccess)
        {
            // ★ 修正点：ドロップ失敗通知を呼び出し、親コンテナ側で再構成を指示
            if (_isEquippedSlot)
            {
                // 持ち込みスロットからドラッグしたがドロップに失敗した (スロットUIを再構成)
                _observer?.NotifyEquippedDragCanceled(_slotIndex);
            }
            else
            {
                // 保持リストからドラッグしたがドロップに失敗した (保持リストUIを再構成)
                _observer?.NotifyHoldListDragCanceled(_spellData);
            }
        }
    }

    /// <summary>
    /// ドロップ先のコンテナから、ドロップが成功したことを通知を受け取るためのメソッド。
    /// </summary>
    public void NotifyDropSuccess()
    {
        _dropSuccess = true;
        if (_isEquippedSlot)
        {
            // 持ち込みスロットから抜けた
            _observer?.NotifyEquippedSpellRemoved(_slotIndex);
        }
        else
        {
            // 保持リストから抜けた
            _observer?.NotifyHoldListSpellRemoved(_spellData);
        }
    }

    // --- ポインタークリック処理 ---

    bool showDescription = true;
    /// <summary>
    /// クリック（タップ）されたときに呪文の詳細説明を表示する。
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // SpellBaseデータがない、またはドラッグ操作の場合は何もしない
        if (_spellData == null || eventData.dragging || !showDescription) return;

        // シングルトン経由で詳細パネルの表示を開始
        if (SpellDescriptionUI.Instance != null)
        {
            // 自身の持つ呪文データを渡して表示アニメーションを開始
            Debug.Log($"[EquippedSpellIconUI on {gameObject.name}] Clicked. Show description for {_spellData.spellName}.");
            SpellDescriptionUI.Instance.StartShowAnimation(_spellData);
        }
    }

    // --- ドロップイベント通知 (IDropHandlerの実装は外部コンテナが行う) ---

    public void OnDrop(PointerEventData eventData)
    {
        if (_isEquippedSlot)
        {
            if (eventData.pointerDrag.TryGetComponent(out EquippedSpellIconUI droppedSpellUI))
            {
                droppedSpellUI.NotifyDropSuccess();
                // 持ち込みスロットの場合、ドロップされた呪文情報と自身のスロット番号を通知
                _observer?.NotifySpellDroppedOnEquippedSlot(droppedSpellUI.GetSpellData(), _slotIndex);
            }
        }
    }
}


// --- インターフェース定義 ---

/// <summary>
/// EquippedSpellIconUIの操作結果をコントローラー/マネージャーに通知するためのインターフェース。
/// </summary>
public interface IEquippedSpellIconUIObserver
{
    /// <summary>
    /// 持ち込みスロットの呪文がドラッグ開始されたことを通知します。（並び替え、解除操作の開始）
    /// </summary>
    /// <param name="draggedSpell">ドラッグされた呪文のデータ</param>
    /// <param name="fromSlotIndex">ドラッグ元スロットのインデックス</param>
    void NotifyEquippedDragBegin(SpellBase draggedSpell, int fromSlotIndex);

    /// <summary>
    /// 保持リストの呪文がドラッグ開始されたことを通知します。（装備操作の開始）
    /// </summary>
    /// <param name="draggedSpell">ドラッグされた呪文のデータ</param>
    void NotifyHoldListDragBegin(SpellBase draggedSpell);

    /// <summary>
    /// 持ち込みスロット上に別の呪文UIがドロップされたことを通知します。
    /// </summary>
    /// <param name="droppedSpell">ドロップされた呪文のデータ</param>
    /// <param name="targetSlotIndex">ドロップ先の持ち込みスロットのインデックス</param>
    void NotifySpellDroppedOnEquippedSlot(SpellBase droppedSpell, int targetSlotIndex);

    /// <summary>
    /// 持ち込みスロットからのドラッグがキャンセル/失敗した場合に、元のUIの再構成を要求します。
    /// </summary>
    /// <param name="slotIndex">ドラッグ元スロットのインデックス</param>
    void NotifyEquippedDragCanceled(int slotIndex);

    /// <summary>
    /// 保持リストからのドラッグがキャンセル/失敗した場合に、元のUIの再構成を要求します。
    /// </summary>
    /// <param name="draggedSpell">ドラッグがキャンセルされた呪文のデータ</param>
    void NotifyHoldListDragCanceled(SpellBase draggedSpell);

    /// <summary>
    /// 呪文UIがドロップに成功し、持ち込みスロットから呪文が抜き取られたことを通知します。（UIの破棄・非アクティブ化処理が必要）
    /// </summary>
    /// <param name="slotIndex">呪文が抜けたスロットのインデックス</param>
    void NotifyEquippedSpellRemoved(int slotIndex);

    /// <summary>
    /// 呪文UIがドロップに成功し、保持リストから呪文が抜き取られたことを通知します。（UIの破棄・非アクティブ化処理が必要）
    /// </summary>
    /// <param name="removedSpell">抜き取られた呪文のデータ</param>
    void NotifyHoldListSpellRemoved(SpellBase removedSpell);
}