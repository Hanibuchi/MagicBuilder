// EquippedSpellSelectionUIController.cs

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro; // TextMeshProUGUIを使用

/// <summary>
/// 持ち込み呪文選択画面のUI全体を管理するコントローラークラス。
/// EquippedSpellModelからデータを取得し、UIに反映します。
/// 呪文アイコンや空スロットからのドラッグ＆ドロップイベントを受け取り、コントローラークラスに変更を伝達します。
/// </summary>
public class EquippedSpellSelectionUIController : MonoBehaviour,
    IEquippedEmptySlotObserver,
    IEquippedSpellIconUIObserver
{
    // --- インスペクター設定項目 ---

    [Header("UI コンポーネント (親オブジェクト)")]
    [Tooltip("保持呪文リストのアイコンを格納する親オブジェクト (Content)")]
    [SerializeField] private RectTransform holdListContentParent;

    [Tooltip("持ち込み呪文スロットのアイコンを格納する親オブジェクト")]
    [SerializeField] private RectTransform equippedSlotParent;

    [Header("ページ切り替え")]
    [Tooltip("前のページへ移動するボタン")]
    [SerializeField] private Button prevPageButton;
    [Tooltip("次のページへ移動するボタン")]
    [SerializeField] private Button nextPageButton;
    [Tooltip("現在のページ/全ページ数を表示するTextMeshProUGUI (例: 1/4)")]
    [SerializeField] private TextMeshProUGUI pageStatusText;
    [Tooltip("1ページあたりに表示する呪文アイコンの最大数")]
    [SerializeField] private int spellsPerPage = 16;


    [Header("プレハブ")]
    [Tooltip("持ち込みスロットが空の場合に使用するUIプレハブ (EquippedEmptySlotUIがアタッチされている)")]
    [SerializeField] private EquippedEmptySlotUI emptySlotUIPrefab;

    // SpellBase.CreateEquippedIconUI() で生成されるアイコンUIは、このクラスではなくSpellBaseが生成するため、ここでは不要。
    // null/非アンロック時に使用するプレハブなど、EquippedSpellIconUI以外の特殊なUIが必要な場合はここに追加する。

    // --- 内部データ ---

    // EquippedSpellModelから受け取った最新の全所持呪文ステータス
    private IReadOnlyList<SpellHoldStatus> _allSpellStatuses = new List<SpellHoldStatus>();

    // EquippedSpellModelから受け取った最新の持ち込み呪文リスト
    private IReadOnlyList<SpellBase> _currentEquippedSpells = new List<SpellBase>();

    // 現在のページ番号 (1から開始)
    private int _currentPage = 1;
    // 全ページ数
    private int _totalPages = 1;

    // 現在生成されている保持リストのEquippedSpellIconUIのリスト
    private List<EquippedSpellIconUI> _holdListSpellUIs = new List<EquippedSpellIconUI>();

    // 現在生成されている持ち込みスロットのUI (EquippedSpellIconUIまたはEquippedEmptySlotUI) のリスト
    private List<Component> _equippedSlotUIs = new List<Component>();

    // ドラッグ中の呪文データ (ドロップ後の処理で使用)
    private SpellBase _draggedSpellData = null;
    // ドラッグ元のスロットインデックス (-1は保持リストから)
    private int _draggedFromSlotIndex = -1;

    // --- Unity イベント関数 ---

    private void Start()
    {
        // ページ切り替えボタンのリスナー登録
        prevPageButton?.onClick.AddListener(() => ChangePage(-1));
        nextPageButton?.onClick.AddListener(() => ChangePage(1));
    }


    // 依存コンポーネントの書き換え
    private IEquippedSpellUIProvider _provider;

    public void SetInterface(IEquippedSpellUIProvider provider)
    {
        _provider = provider;
    }








    public void OnAllSpellStatusesChanged(IReadOnlyList<SpellHoldStatus> allSpellStatuses)
    {
        // 呪文の所持状況、利用可能数が変わった
        _allSpellStatuses = allSpellStatuses;
        RebuildHoldList(); // 保持リストのUIを更新
        RebuildEquippedSlots(); // 利用可能数が変わった場合、装備スロットのUIも更新が必要
        Debug.Log("[Controller Proxy] AllSpellStatusesChanged. UI Rebuilded.");
    }

    public void OnEquippedSpellsChanged(IReadOnlyList<SpellBase> equippedSpells)
    {
        // 持ち込みスロットの内容が変わった
        _currentEquippedSpells = equippedSpells;
        RebuildEquippedSlots(); // 持ち込みスロットのUIを更新
                                // ※ RecalculateAllSpellStatusesAndNotify() の中で OnEquippedSpellsChanged が呼ばれるため、
                                //    このメソッドが呼ばれる時点では _allSpellStatuses の更新も同時に行われていることが期待される。
        Debug.Log("[Controller Proxy] EquippedSpellsChanged. Equipped Slots Rebuilded.");
    }









    // --- UI再構築ロジック ---

    /// <summary>
    /// 持ち込みスロットUIを現在の _currentEquippedSpells に基づいて再構築します。
    /// </summary>
    private void RebuildEquippedSlots()
    {
        // 既存のUIをすべて破棄
        foreach (var ui in _equippedSlotUIs)
        {
            if (ui != null && ui.gameObject != null)
            {
                Destroy(ui.gameObject);
            }
        }
        _equippedSlotUIs.Clear();

        // 最大スロット数に基づいて新しいUIを生成
        for (int i = 0; i < _currentEquippedSpells.Count; i++)
        {
            SpellBase spell = _currentEquippedSpells[i];

            if (spell != null)
            {
                // 呪文データが存在する場合: EquippedSpellIconUIを生成
                EquippedSpellIconUI iconUI = spell.CreateEquippedIconUI();
                if (iconUI != null)
                {
                    iconUI.transform.SetParent(equippedSlotParent, false);
                    iconUI.SetSlotIndex(i, true); // 持ち込みスロットとして設定
                    iconUI.SetObserver(this);

                    // ステータスを更新
                    var status = _allSpellStatuses.FirstOrDefault(s => s.Type == SpellDatabase.Instance.GetSpellType(spell));
                    if (status != null && !status.IsUnlocked)
                    {
                        // 装備されているが、Modelの最新情報ではアンロックされていない場合 (ありえないはずだが念のため)
                        iconUI.SetFrameColor(false); // グレーアウト
                        iconUI.SetIcon(false);       // ロックアイコン
                        iconUI.SetActive(false);     // 操作不可
                    }
                    else
                    {
                        iconUI.SetFrameColor(true); // 通常色
                        iconUI.SetIcon(true);       // 通常アイコン
                        iconUI.SetActive(true);     // 操作可能
                    }
                    iconUI.SetAvailableCount(status != null ? status.AvailableCount : -1); // 持ち込みスロットでは非表示になるが念のため

                    _equippedSlotUIs.Add(iconUI);
                }
            }
            else
            {
                // 呪文データが存在しない場合: EquippedEmptySlotUIを生成
                if (emptySlotUIPrefab != null)
                {
                    EquippedEmptySlotUI emptyUI = Instantiate(emptySlotUIPrefab, equippedSlotParent);
                    emptyUI.Initialize(i);
                    emptyUI.SetObserver(this);
                    _equippedSlotUIs.Add(emptyUI);
                }
            }
        }
        Debug.Log($"[Controller] Equipped Slots Rebuilt.");
    }

    /// <summary>
    /// 保持呪文リストのUIを現在の _allSpellStatuses に基づいて再構築し、現在のページを反映します。
    /// </summary>
    private void RebuildHoldList()
    {
        // 既存のUIをすべて破棄
        foreach (var ui in _holdListSpellUIs)
        {
            if (ui != null && ui.gameObject != null)
            {
                Destroy(ui.gameObject);
            }
        }
        _holdListSpellUIs.Clear();

        // フィルタリングとソート
        var sortedStatuses = _allSpellStatuses
            .Where(s => s.Type != SpellType.None) // Noneタイプを除外
            .OrderByDescending(s => s.IsUnlocked)
            .ThenByDescending(s => s.TotalCount)
            .ThenBy(s => s.Type.ToString())
            .ToList();

        // ページ数と表示範囲の計算
        _totalPages = Mathf.Max(1, Mathf.CeilToInt((float)sortedStatuses.Count / spellsPerPage));
        _currentPage = Mathf.Clamp(_currentPage, 1, _totalPages);
        int startIndex = (_currentPage - 1) * spellsPerPage;
        int endIndex = Mathf.Min(startIndex + spellsPerPage, sortedStatuses.Count);

        // ページ情報の更新
        UpdatePageStatusUI(sortedStatuses.Count);

        // ページ内の呪文UIを生成
        for (int i = startIndex; i < endIndex; i++)
        {
            var status = sortedStatuses[i];

            // SpellTypeからSpellBaseを取得（アンロック済みでなくてもアイコン取得のために必要）
            SpellBase spellAsset = SpellDatabase.Instance.GetSpellAsset(status.Type);
            if (spellAsset == null) { Debug.LogWarning("Spell asset not found for type: " + status.Type); continue; } // アセットが見つからない場合はスキップ

            EquippedSpellIconUI iconUI = spellAsset.CreateEquippedIconUI();
            if (iconUI == null) { Debug.LogWarning("Failed to create EquippedSpellIconUI for spell: " + spellAsset.spellName); continue; }

            iconUI.transform.SetParent(holdListContentParent, false);
            iconUI.SetSlotIndex(-1, false); // 保持リストとして設定
            iconUI.SetObserver(this);

            if (status.IsUnlocked)
            {
                // アンロック済み
                iconUI.SetIcon(true);       // 通常アイコン
                iconUI.SetActive(true);     // 操作可能
                iconUI.SetShowDescription(true); // 詳細表示可能

                if (status.TotalCount > 0)
                {
                    // 所持している
                    iconUI.SetFrameColor(true); // 通常色 (利用可能数の有無に関わらず所持していれば色付き)
                    iconUI.SetAvailableCount(status.AvailableCount);
                    if (status.AvailableCount <= 0)
                    {
                        // 在庫なし（装備スロットが埋まっているなど）
                        // アイコンは色付きのまま（所持はしているため）
                        iconUI.SetActive(false); // ドラッグ操作は不可
                    }
                }
                else
                {
                    // アンロック済みだが所持していない
                    iconUI.SetFrameColor(false); // グレーアウト
                    iconUI.SetAvailableCount(-1); // 所持数非表示
                    iconUI.SetActive(false); // ドラッグ操作不可
                }
            }
            else
            {
                // 非アンロック（ロック状態）
                iconUI.SetIcon(false);          // ロックアイコン
                iconUI.SetFrameColor(false);    // グレーアウト
                iconUI.SetAvailableCount(-1);   // 所持数非表示
                iconUI.SetActive(false);        // 操作不可
                iconUI.SetShowDescription(false); // 詳細表示不可
                // TODO: UIを「？」マークなどにする処理があれば追加
            }

            _holdListSpellUIs.Add(iconUI);
        }
        Debug.Log($"[Controller] Hold List Rebuilt. Page: {_currentPage}/{_totalPages}");
    }

    /// <summary>
    /// ページ切り替え処理を実行します。
    /// </summary>
    private void ChangePage(int direction)
    {
        int newPage = Mathf.Clamp(_currentPage + direction, 1, _totalPages);
        if (newPage != _currentPage)
        {
            _currentPage = newPage;
            RebuildHoldList();
        }
    }

    /// <summary>
    /// ページ切り替えUIの状態を更新します。
    /// </summary>
    private void UpdatePageStatusUI(int totalCount)
    {
        pageStatusText.text = totalCount > 0 ? $"{_currentPage}/{_totalPages}" : "0/0";
        prevPageButton.interactable = _currentPage > 1;
        nextPageButton.interactable = _currentPage < _totalPages;
    }


    // --- IEquippedEmptySlotObserver の実装 (空スロットへのドロップ) ---

    /// <summary>
    /// EquippedEmptySlotUIに呪文がドロップされたときの通知を受け取ります。
    /// </summary>
    public void NotifySpellDroppedOnEmptySlot(SpellBase droppedSpell, int targetSlotIndex)
    {
        _provider.SetSpell(targetSlotIndex, droppedSpell);

        // Modelから変更通知が来て、UIが再構築されるのを待つ
    }


    // --- IEquippedSpellIconUIObserver の実装 (アイコンUIからのドラッグ/ドロップ) ---

    public void NotifyEquippedDragBegin(SpellBase draggedSpell, int fromSlotIndex)
    {
        // 持ち込みスロットからのドラッグ開始（並び替え・解除）
        _draggedSpellData = draggedSpell;
        _draggedFromSlotIndex = fromSlotIndex;
        // ドラッグ開始と同時に、元のスロットを空スロットにする
        // _provider.RemoveSpell(fromSlotIndex);
    }

    public void NotifyHoldListDragBegin(SpellBase draggedSpell)
    {
        // 保持リストからのドラッグ開始（装備）
        _draggedSpellData = draggedSpell;
        _draggedFromSlotIndex = -1;
    }

    public void NotifySpellDroppedOnEquippedSlot(SpellBase droppedSpell, int targetSlotIndex)
    {
        // 持ち込みスロット上に別の呪文UIがドロップされたとき（並び替え、または上書き）
        _provider.SetSpell(targetSlotIndex, droppedSpell);
    }

    public void NotifyEquippedDragCanceled(int slotIndex)
    {
        // 持ち込みスロットからのドラッグがキャンセルされたとき
        // UIが元の状態に戻るように再構築を要求
        Debug.Log($"[Controller] Equipped Drag Canceled from slot {slotIndex}. Rebuilding Equipped Slots...");
        RebuildEquippedSlots();
        // **注意**: EquippedSpellIconUI.CheckDropResultAndCleanUp() で親を EquippedSlotParent に戻す処理がないため、
        // ここで再構築全体を行うことで、親の再設定とUIの復元を強制します。
    }

    public void NotifyHoldListDragCanceled(SpellBase draggedSpell)
    {
        // 保持リストからのドラッグがキャンセルされたとき
        // UIが元の状態に戻るように再構築を要求
        Debug.Log($"[Controller] Hold List Drag Canceled for {draggedSpell.spellName}. Rebuilding Hold List...");
        RebuildHoldList();
        // **注意**: EquippedSpellIconUI.CheckDropResultAndCleanUp() で親を HoldListContentParent に戻す処理がないため、
        // ここで再構築全体を行うことで、親の再設定とUIの復元を強制します。
    }

    public void NotifyEquippedSpellRemoved(int slotIndex)
    {
        // 呪文UIがドロップに成功し、持ち込みスロットから呪文が抜き取られたことを通知
        // (空スロット/別のスロットにドロップされた際、EquippedSpellIconUI側で呼ばれる)
        if (_draggedFromSlotIndex != slotIndex)
        {
            // 持ち込みリスト->持ち込みリストの移動のうち、自分以外の場所にドロップした場合、元いたスロットを空にしないと呪文が複製されてしまう。
            _provider.RemoveSpell(slotIndex);
        }

        // ドラッグ状態をリセット (重要)
        _draggedSpellData = null;
        _draggedFromSlotIndex = -1;
    }

    public void NotifyHoldListSpellRemoved(SpellBase removedSpell)
    {
        // 呪文UIがドロップに成功し、保持リストから呪文が抜き取られたことを通知
        // (空スロット/装備スロットにドロップされた際、EquippedSpellIconUI側で呼ばれる)
        // 保持リストの呪文は装備されてもそのまま残るため、ここでは特別な処理は不要。
        // EquippedSpellIconUIが親から切り離されて残っているため、破棄が必要だが、
        // RebuildHoldList()で一括破棄されるため、ここでは不要（ロジックをシンプルに保つ）

        // ドラッグ状態をリセット (重要)
        _draggedSpellData = null;
        _draggedFromSlotIndex = -1;
    }
}


/// <summary>
/// UIControllerが必要とするデータ操作と取得を抽象化するインターフェース
/// </summary>
public interface IEquippedSpellUIProvider
{
    // データの取得
    IReadOnlyList<SpellHoldStatus> GetAllSpellStatuses();
    IReadOnlyList<SpellBase> GetCurrentEquippedSpells();
    int GetMaxCapacity();

    // 操作の実行
    void SetSpell(int index, SpellBase spell);
    void RemoveSpell(int index);

    // UI側の再構築をキックするためのコールバック登録（必要に応じて）
    void SetUIController(EquippedSpellSelectionUIController controller);
}