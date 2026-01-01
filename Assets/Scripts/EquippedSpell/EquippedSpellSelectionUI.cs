// EquippedSpellSelectionUIController.cs

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System;

/// <summary>
/// 持ち込み呪文選択画面のUI全体を管理するコントローラークラス。
/// EquippedSpellModelからデータを取得し、UIに反映します。
/// 呪文アイコンや空スロットからのドラッグ＆ドロップイベントを受け取り、コントローラークラスに変更を伝達します。
/// </summary>
public class EquippedSpellSelectionUI : MonoBehaviour,
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

    [Header("容量拡張UI")]
    [SerializeField] private Button increaseCapacityButton;

    [Header("装備解除（ゴミ箱）設定")]
    [SerializeField] private EquippedTrashAreaUI trashArea;
    void SetTrashArea(bool active)
    {
        if (trashArea != null)
            trashArea.gameObject.SetActive(active);
        else
            Debug.LogError("trashArea is not assigned.");
    }

    // --- Unity イベント関数 ---

    // --- シングルトン実装 ---
    public static EquippedSpellSelectionUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // ページ切り替えボタンのリスナー登録
        prevPageButton?.onClick.AddListener(() => ChangePage(-1));
        nextPageButton?.onClick.AddListener(() => ChangePage(1));
        increaseCapacityButton?.onClick.AddListener(() =>
        {
            _provider.RequestIncreaseCapacity();
        });
    }


    // 依存コンポーネントの書き換え
    private IEquippedSpellUIProvider _provider;

    public void Init(IEquippedSpellUIProvider provider)
    {
        _provider = provider;
        // 初期データの反映
        _allSpellStatuses = _provider.GetAllSpellStatuses();
        _currentEquippedSpells = _provider.GetCurrentEquippedSpells();

        SetTrashArea(false);
        RebuildHoldList();
        RebuildEquippedSlots();
    }




    [Header("アニメーション設定")]
    [SerializeField] private Animator animator;
    static string OPEN_PARAM = "Open";
    static string CLOSE_PARAM = "Close";
    // UIが表示中かどうか
    public bool IsVisible { get; private set; } = false;

    private bool _wasCurrencyUIShowing;

    [SerializeField] AudioClip openClip;
    Action closeCallback;
    /// <summary>
    /// UIを表示するアニメーションを開始します
    /// </summary>
    public void Open(Action callback)
    {
        if (animator == null) return;
        closeCallback = callback;

        if (IsVisible) return;
        IsVisible = true;

        // CurrencyUIの状態を記録して表示
        if (CurrencyUI.Instance != null)
        {
            _wasCurrencyUIShowing = CurrencyUI.Instance.IsShowing;
            CurrencyUI.Instance.Show();
        }

        this.gameObject.SetActive(true); // オブジェクト自体をアクティブに
        animator.SetTrigger(OPEN_PARAM);
        animator.ResetTrigger(CLOSE_PARAM);
        SoundManager.Instance?.PlaySE(openClip);

        // UIが開く際の最新データを反映
        if (_provider != null)
        {
            SetHoldSpells(_provider.GetAllSpellStatuses());
            SetEquippedSpells(_provider.GetCurrentEquippedSpells());
        }
    }

    [SerializeField] AudioClip closeClip;
    /// <summary>
    /// UIを閉じるアニメーションを開始します
    /// </summary>
    public void Close()
    {
        if (animator == null) return;
        if (!IsVisible) return;

        IsVisible = false;

        // CurrencyUIの状態を元に戻す
        if (CurrencyUI.Instance != null)
        {
            if (!_wasCurrencyUIShowing)
            {
                CurrencyUI.Instance.Hide();
            }
        }

        animator.SetTrigger(CLOSE_PARAM);
        animator.ResetTrigger(OPEN_PARAM);

        SoundManager.Instance?.PlaySE(closeClip);
    }

    /// <summary>
    /// アニメーションイベントから呼ばれることを想定した非アクティブ化メソッド
    /// </summary>
    public void OnCloseAnimationComplete()
    {
        this.gameObject.SetActive(false);
        closeCallback?.Invoke();
    }



    public void SetHoldSpells(IReadOnlyList<SpellHoldStatus> allSpellStatuses)
    {
        // 呪文の所持状況、利用可能数が変わった
        _allSpellStatuses = allSpellStatuses;
        RebuildHoldList(); // 保持リストのUIを更新
        Debug.Log("[Controller Proxy] AllSpellStatusesChanged. UI Rebuilded.");
    }

    public void SetEquippedSpells(IReadOnlyList<SpellBase> equippedSpells)
    {
        // 持ち込みスロットの内容が変わった
        _currentEquippedSpells = equippedSpells;
        RebuildEquippedSlots();
        Debug.Log("[Controller Proxy] EquippedSpellsChanged. Equipped Slots Rebuilded.");
    }









    // --- UI再構築ロジック ---

    /// <summary>
    /// 持ち込みスロットUIを現在の _currentEquippedSpells に基づいて再構築します。
    /// </summary>
    private void RebuildEquippedSlots(bool drag = false)
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

        if (!drag)
            DestroyIconUI();

        // 最大スロット数に基づいて新しいUIを生成
        for (int i = 0; i < _currentEquippedSpells.Count; i++)
        {
            SpellBase spell = _currentEquippedSpells[i];

            UnityEngine.Component component;
            if (spell != null)
            {
                // 呪文データが存在する場合: EquippedSpellIconUIを生成
                EquippedSpellIconUI iconUI = spell.CreateEquippedIconUI();
                if (iconUI == null)
                {
                    Debug.LogError("Failed to create EquippedSpellIconUI for spell: " + spell.spellName);
                    continue;
                }
                iconUI.transform.SetParent(equippedSlotParent, false);
                iconUI.SetSlotIndex(i, true); // 持ち込みスロットとして設定
                iconUI.SetObserver(this);
                iconUI.SetFrameColor(true); // 通常色
                iconUI.SetIcon(true);       // 通常アイコン
                iconUI.SetDrag(true);     // 操作可能
                iconUI.SetAvailableCount(-1);
                component = iconUI;
            }
            else
            {
                if (emptySlotUIPrefab == null)
                {
                    Debug.LogError("EmptySlotUIPrefab is not assigned.");
                    continue;
                }

                // 呪文データが存在しない場合: EquippedEmptySlotUIを生成
                EquippedEmptySlotUI emptyUI = Instantiate(emptySlotUIPrefab, equippedSlotParent);
                emptyUI.Initialize(i);
                emptyUI.SetObserver(this);
                component = emptyUI;
            }
            component.transform.SetSiblingIndex(i);
            _equippedSlotUIs.Add(component);
        }
        Debug.Log($"[Controller] Equipped Slots Rebuilt.");
    }

    void DestroyIconUI()
    {
        if (draggingIconUI != null)
        {
            Destroy(draggingIconUI.gameObject);
            draggingIconUI = null;
        }
    }

    /// <summary>
    /// 保持呪文リストのUIを現在の _allSpellStatuses に基づいて再構築し、現在のページを反映します。
    /// </summary>
    private void RebuildHoldList(bool drag = false)
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

        if (!drag)
            DestroyIconUI();


        UpdateSpellsPerPage();

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
                iconUI.SetShowDescription(true); // 詳細表示可能

                if (status.TotalCount > 0)
                {
                    // 所持している
                    iconUI.SetFrameColor(true); // 通常色 (利用可能数の有無に関わらず所持していれば色付き)
                    iconUI.SetAvailableCount(status.AvailableCount);
                    iconUI.SetDrag(true);     // 操作可能
                    if (status.AvailableCount <= 0)
                    {
                        // 在庫なし（装備スロットが埋まっているなど）
                        // アイコンは色付きのまま（所持はしているため）
                        iconUI.SetDrag(false); // ドラッグ操作は不可
                    }
                }
                else
                {
                    // アンロック済みだが所持していない
                    iconUI.SetFrameColor(false); // グレーアウト
                    iconUI.SetAvailableCount(-1); // 所持数非表示
                    iconUI.SetDrag(false); // ドラッグ操作不可
                }
            }
            else
            {
                // 非アンロック（ロック状態）
                iconUI.SetIcon(false);          // ロックアイコン
                iconUI.SetFrameColor(false);    // グレーアウト
                iconUI.SetAvailableCount(-1);   // 所持数非表示
                iconUI.SetDrag(false);        // 操作不可
                iconUI.SetShowDescription(false); // 詳細表示不可
                // TODO: UIを「？」マークなどにする処理があれば追加
            }

            _holdListSpellUIs.Add(iconUI);
        }
        Debug.Log($"[Controller] Hold List Rebuilt. Page: {_currentPage}/{_totalPages}");
    }


    [Header("Layout Reference")]
    [Tooltip("保持リストのレイアウト設定を取得するためのGridLayoutGroup")]
    [SerializeField] private GridLayoutGroup holdListGridLayout;

    // --- 内部メソッドの追加 ---

    /// <summary>
    /// GridLayoutGroupの設定とRectTransformのサイズから、1ページに収まる要素数を計算します。
    /// </summary>
    private void UpdateSpellsPerPage()
    {
        if (holdListGridLayout == null || holdListContentParent == null)
        {
            Debug.LogWarning("GridLayoutGroup or Parent RectTransform is not assigned.");
            return;
        }

        // 親（Content）のサイズを取得
        // CanvasUpdateを待たないとサイズが0の場合があるため、正確を期すならLayoutRebuilder等が必要な場合があります
        Rect parentRect = holdListContentParent.rect;

        // セルサイズと間隔を取得
        Vector2 cellSize = holdListGridLayout.cellSize;
        Vector2 spacing = holdListGridLayout.spacing;
        RectOffset padding = holdListGridLayout.padding;

        // 利用可能な幅と高さ（パディングを除く）
        float availableWidth = parentRect.width - padding.left - padding.right;
        float availableHeight = parentRect.height - padding.top - padding.bottom;

        // 横に何個並ぶか
        // (Width + spacing.x) / (cellSize.x + spacing.x) で計算
        int columns = Mathf.FloorToInt((availableWidth + spacing.x) / (cellSize.x + spacing.x));

        // 縦に何個並ぶか
        int rows = Mathf.FloorToInt((availableHeight + spacing.y) / (cellSize.y + spacing.y));

        // 0にならないようガード
        columns = Mathf.Max(1, columns);
        rows = Mathf.Max(1, rows);

        // 新しいページあたりの表示数を設定
        spellsPerPage = columns * rows;

        Debug.Log($"[DynamicLayout] Columns: {columns}, Rows: {rows}, SpellsPerPage: {spellsPerPage}");
    }



    [Header("ページ切り替えアニメーション")]
    [SerializeField] private Animator pageAnimator; // ページ専用のアニメーター
    [SerializeField] private string nextTriggerName = "Next"; // 次へ移動する時のトリガー
    [SerializeField] private string prevTriggerName = "Prev"; // 前へ移動する時のトリガー
    /// <summary>
    /// ページ切り替え処理を実行します。
    /// </summary>
    private void ChangePage(int direction)
    {
        int newPage = Mathf.Clamp(_currentPage + direction, 1, _totalPages);
        if (newPage != _currentPage)
        {
            _currentPage = newPage;

            PlayChangePageSound();
            // アニメーションの実行
            PlayPageAnimation(direction);

            // UIの再構築
            RebuildHoldList();
        }
    }

    [SerializeField] AudioClip pageMoveInventorySound;
    void PlayChangePageSound()
    {
        if (SoundManager.Instance != null && pageMoveInventorySound != null)
            SoundManager.Instance.PlaySE(pageMoveInventorySound);
    }

    /// <summary>
    /// ページ移動方向に合わせたアニメーションを再生します
    /// </summary>
    /// <param name="direction">1なら次へ(Next), -1なら前へ(Prev)</param>
    private void PlayPageAnimation(int direction)
    {
        if (pageAnimator == null) return;

        if (direction > 0)
        {
            pageAnimator.SetTrigger(nextTriggerName);
        }
        else if (direction < 0)
        {
            pageAnimator.SetTrigger(prevTriggerName);
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
        PlaySpellSetSound(droppedSpell);
        _provider.SetSpell(targetSlotIndex, droppedSpell);
        // Modelから変更通知が来て、UIが再構築されるのを待つ
    }


    // --- IEquippedSpellIconUIObserver の実装 (アイコンUIからのドラッグ/ドロップ) ---
    EquippedSpellIconUI draggingIconUI;

    public void NotifyEquippedDragBegin(SpellBase draggedSpell, int fromSlotIndex)
    {
        draggingIconUI = (EquippedSpellIconUI)_equippedSlotUIs[fromSlotIndex];
        _equippedSlotUIs[fromSlotIndex] = null;

        RebuildEquippedSlots(true);
        ReplaceEquippedSlotWithEmpty(fromSlotIndex);

        // 持ち込みスロットからのドラッグ開始（並び替え・解除）
        _draggedSpellData = draggedSpell;
        _draggedFromSlotIndex = fromSlotIndex;

        SetTrashArea(true);
    }

    // --- 指定したインデックスのスロットを空にするメソッド ---
    /// <summary>
    /// 指定されたインデックスの持ち込みスロットUIを削除し、空スロットUIに差し替えます。
    /// </summary>
    /// <param name="index">対象のスロットインデックス</param>
    public void ReplaceEquippedSlotWithEmpty(int index)
    {
        if (index < 0 || index >= _equippedSlotUIs.Count) return;

        // 1. 既存のUI（アイコン等）を破棄
        var oldUI = _equippedSlotUIs[index];
        if (oldUI != null && oldUI.gameObject != null)
        {
            Destroy(oldUI.gameObject);
        }

        // 2. 空スロットUIを生成
        if (emptySlotUIPrefab != null)
        {
            EquippedEmptySlotUI emptyUI = Instantiate(emptySlotUIPrefab, equippedSlotParent);
            emptyUI.Initialize(index);
            emptyUI.SetObserver(this);

            // 階層順序を元のインデックスに合わせる
            emptyUI.transform.SetSiblingIndex(index);

            // リスト内の参照を更新
            _equippedSlotUIs[index] = emptyUI;
        }

        Debug.Log($"[UI Update] Slot {index} replaced with empty.");
    }

    public void NotifyHoldListDragBegin(SpellBase draggedSpell)
    {
        // 1. ドラッグが開始されたUIを特定
        // 現在表示されているUIリストの中から、該当する呪文を持つものを探す
        int index = _holdListSpellUIs.FindIndex(ui => ui != null && ui.GetSpellData() == draggedSpell);
        if (index == -1)
        {
            Debug.LogError($"[UI] Drag Begin: {draggedSpell.spellName} not found in hold list.");
            return;
        }

        // 2. ドラッグ中のUIとして保持し、リストから参照を外す
        // これにより、直後の RebuildHoldList(true) 内の Destroy ループから除外される
        draggingIconUI = _holdListSpellUIs[index];
        _holdListSpellUIs[index] = null;

        // 3. 保持リストのUIを再構築
        // 引数 true により、draggingIconUI が破棄されるのを防ぐ
        RebuildHoldList(true);

        // 5. 新しく生成された保持リスト側のUI表示を調整
        // 手に持っている分（ドラッグ中）の1つを差し引いた個数を表示させる
        DecrementHoldListCount(draggedSpell);

        Debug.Log($"[UI] Hold List Drag Begin: {draggedSpell.spellName}. UI rebuilt and count adjusted.");

        // 4. ドラッグ情報をセット
        _draggedSpellData = draggedSpell;
        _draggedFromSlotIndex = -1;
    }

    // --- 保持リストの特定呪文の数を減らすメソッド ---
    /// <summary>
    /// 指定された呪文に対応する保持リスト内のUIの表示個数を1減らします。
    /// 0になった場合はドラッグ不可に設定します。
    /// </summary>
    /// <param name="spell">対象の呪文データ</param>
    public void DecrementHoldListCount(SpellBase spell)
    {
        if (spell == null) return;

        // 現在のページに表示されているUIの中から、該当する呪文を探す
        var targetUI = _holdListSpellUIs.Find(ui => ui.GetSpellData() == spell);

        if (targetUI != null)
        {
            // 現在の残り数を取得して減らす（AvailableCountはUI側で保持されている想定）
            int newCount = targetUI.AvailableCount - 1;

            // UI上の数値を更新
            targetUI.SetAvailableCount(newCount);

            // 0以下になったらドラッグを禁止する
            if (newCount <= 0)
            {
                targetUI.SetDrag(false);
                // 必要に応じてグレーアウト処理などを追加
                // targetUI.SetFrameColor(false); 
            }

            Debug.Log($"[UI Update] {spell.spellName} count decremented to {newCount}.");
        }
    }

    public void NotifySpellDroppedOnEquippedSlot(SpellBase droppedSpell, int targetSlotIndex)
    {
        PlaySpellSetSound(droppedSpell);
        // 持ち込みスロット上に別の呪文UIがドロップされたとき（並び替え、または上書き）
        _provider.SetSpell(targetSlotIndex, droppedSpell);
    }

    void PlaySpellSetSound(SpellBase spellToAdd)
    {
        if (SoundManager.Instance != null && spellToAdd != null)
        {
            spellToAdd.GetDropSound(out AudioClip clip, out float volume);
            if (clip != null)
            {
                SoundManager.Instance.PlaySE(clip, volume);
            }
        }
    }

    public void NotifyEquippedDragCanceled(int slotIndex)
    {
        // 持ち込みスロットからのドラッグがキャンセルされたとき
        // UIが元の状態に戻るように再構築を要求
        Debug.Log($"[Controller] Equipped Drag Canceled from slot {slotIndex}. Rebuilding Equipped Slots...");
        RebuildEquippedSlots();

        SetTrashArea(false);
    }

    public void NotifyHoldListDragCanceled(SpellBase draggedSpell)
    {
        // 保持リストからのドラッグがキャンセルされたとき
        // UIが元の状態に戻るように再構築を要求
        Debug.Log($"[Controller] Hold List Drag Canceled for {draggedSpell.spellName}. Rebuilding Hold List...");
        RebuildHoldList();
    }

    public void NotifyEquippedSpellRemoved(int slotIndex)
    {
        // 呪文UIがドロップに成功し、持ち込みスロットから呪文が抜き取られたことを通知
        // (空スロット/別のスロットにドロップされた際、EquippedSpellIconUI側で呼ばれる)
        _provider.RemoveSpell(slotIndex);

        SetTrashArea(false);

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






    // --- デバッグ・テスト用設定項目 ---

    [Header("--- Test Settings ---")]
    [SerializeField] private List<SpellBase> test_manualEquippedSpells = new List<SpellBase>();

    [Tooltip("テスト用の所持状況データ。インスペクタで要素を追加して試せます")]
    [SerializeField] private List<TestHoldStatusData> test_manualHoldStatuses = new List<TestHoldStatusData>();

    [System.Serializable]
    public struct TestHoldStatusData
    {
        public SpellType type;
        public bool isUnlocked;
        public int totalCount;
        public int equippedCount;
    }

    // --- テスト用公開メソッド (Buttonから呼び出し可能) ---

    /// <summary>
    /// インスペクタの test_manualHoldStatuses リストの内容を UI に反映します。
    /// </summary>
    public void Test_ApplyHoldSpells()
    {
        List<SpellHoldStatus> statusList = new List<SpellHoldStatus>();
        foreach (var data in test_manualHoldStatuses)
        {
            statusList.Add(new SpellHoldStatus(data.type, data.isUnlocked, data.totalCount, data.equippedCount));
        }

        SetHoldSpells(statusList.AsReadOnly());
        Debug.Log("<color=cyan>[Test]</color> 保持リストのテストデータを適用しました。");
    }

    /// <summary>
    /// インスペクタの test_manualEquippedSpells リストの内容を UI に反映します。
    /// </summary>
    public void Test_ApplyEquippedSpells()
    {
        SetEquippedSpells(test_manualEquippedSpells.AsReadOnly());
        Debug.Log("<color=lime>[Test]</color> 持ち込みスロットのテストデータを適用しました。");
    }

    /// <summary>
    /// 全てのテスト用データを一括で適用します。
    /// </summary>
    public void Test_ApplyAll()
    {
        Test_ApplyHoldSpells();
        Test_ApplyEquippedSpells();
    }

    /// <summary>
    /// 現在のページを強制的にリロードします（UIの再描画チェック用）。
    /// </summary>
    public void Test_ForceRebuild()
    {
        RebuildHoldList();
        RebuildEquippedSlots();
        Debug.Log("<color=yellow>[Test]</color> UIの再構築を強制実行しました。");
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

    // 操作の実行
    void SetSpell(int index, SpellBase spell);
    void RemoveSpell(int index);
    void RequestIncreaseCapacity();
}