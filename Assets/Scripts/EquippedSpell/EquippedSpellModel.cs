// EquippedSpellModel.cs

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// 持ち込み呪文の選択画面でUIに表示するための情報を提供するモデル。
/// SpellHoldInfoManagerとEquippedSpellManagerの情報を統合・加工してコントローラーに通知する。
/// シングルトンであり、両Managerのオブザーバーとして機能する。
/// </summary>
public class EquippedSpellModel : MonoBehaviour, ISpellHoldInfoObserver, IEquippedSpellsObserver
{
    // --- シングルトン実装 ---
    private static EquippedSpellModel _instance;
    public static EquippedSpellModel Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject singletonObject = new GameObject(nameof(EquippedSpellModel));
                _instance = singletonObject.AddComponent<EquippedSpellModel>();
                // シーンを跨いでも破棄されないように設定
                DontDestroyOnLoad(singletonObject);
                _instance.Initialize();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            // シーンを跨いでも破棄されないように設定
            DontDestroyOnLoad(gameObject);
            _instance = this;
            _instance.Initialize();
        }
        else if (_instance != this)
        {
            // 既にインスタンスが存在する場合、自分自身を破棄
            Destroy(gameObject);
        }
    }

    // --- 内部データとオブザーバー ---

    // 全呪文タイプの所持情報を格納
    private Dictionary<SpellType, SpellHoldStatus> _allSpellStatuses = new Dictionary<SpellType, SpellHoldStatus>();

    // 現在の持ち込み呪文リスト（EquippedSpellManagerから取得した最新情報）
    private IReadOnlyList<SpellBase> _currentEquippedSpells = new List<SpellBase>();

    // 外部通知用（登録できるのは1つのみ）
    private IEquippedSpellModelObserver _observer;


    /// <summary>
    /// 初期化処理。Managerへの登録と初期データの取得を行います。
    /// </summary>
    private void Initialize()
    {
        // Managerへのオブザーバー登録
        if (SpellHoldInfoManager.Instance != null)
        {
            SpellHoldInfoManager.Instance.SetObserver(this);
        }
        else
        {
            Debug.LogError("SpellHoldInfoManager が見つかりません。");
        }

        if (EquippedSpellManager.Instance != null)
        {
            EquippedSpellManager.Instance.RegisterObserver(this);
            // RegisterObserver内で Load と Notify が行われるため、ここで初期データ取得は不要
        }
        else
        {
            Debug.LogError("EquippedSpellManager が見つかりません。");
        }

        // 初回データ構築
        // SpellHoldInfoManagerのロードは Awake で既に行われている前提
        RecalculateAllSpellStatusesAndNotify();
    }


    // --- 公開メソッド ---

    /// <summary>
    /// 変更通知を受け取るオブザーバーをセットします。（1つのみ登録可能）
    /// </summary>
    public void SetObserver(IEquippedSpellModelObserver observer)
    {
        _observer = observer;
        // 登録直後に現在の全情報を通知
        NotifyAllCurrentData();
    }

    /// <summary>
    /// 現在の全所持呪文ステータスの読み取り専用リストを取得します。
    /// SpellDatabase で設定された順番（allSpells）に従ったリストを返します。
    /// </summary>
    public IReadOnlyList<SpellHoldStatus> GetAllSpellStatuses()
    {
        if (SpellDatabase.Instance == null) return new List<SpellHoldStatus>().AsReadOnly();

        // Databaseの登録順（allSpellsの並び順）に従ってリストを構築する
        var allTypes = SpellDatabase.Instance.GetAllRegisteredSpellTypes().Where(t => t != SpellType.None);
        var orderedStatuses = new List<SpellHoldStatus>();

        foreach (var type in allTypes)
        {
            if (_allSpellStatuses.TryGetValue(type, out var status))
            {
                orderedStatuses.Add(status);
            }
        }

        return orderedStatuses.AsReadOnly();
    }

    /// <summary>
    /// 現在の持ち込み呪文リスト（スロットの内容）を取得します。
    /// </summary>
    public IReadOnlyList<SpellBase> GetEquippedSpells()
    {
        return _currentEquippedSpells;
    }

    /// <summary>
    /// 現在の最大持ち込みスロット数を取得します。
    /// </summary>
    public int GetMaxCapacity()
    {
        return EquippedSpellManager.Instance.GetMaxCapacity();
    }


    // --- データ加工ロジック ---

    /// <summary>
    /// 全呪文のSpellHoldStatusを再計算し、オブザーバーに通知します。
    /// </summary>
    private void RecalculateAllSpellStatusesAndNotify()
    {
        // SpellDatabaseに登録されている全SpellTypeを取得
        if (SpellDatabase.Instance == null) return;
        var allTypes = SpellDatabase.Instance.GetAllRegisteredSpellTypes().Where(t => t != SpellType.None);

        // 持ち込みスロットでの使用数を計算
        var equippedCounts = _currentEquippedSpells
            .Where(spell => spell != null)
            .Select(spell => SpellDatabase.Instance.GetSpellType(spell))
            .GroupBy(type => type)
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var type in allTypes)
        {
            // SpellHoldInfoManagerから基本情報を取得
            int totalCount = SpellHoldInfoManager.Instance.GetSpellCount(type);
            bool isUnlocked = SpellHoldInfoManager.Instance.IsSpellUnlocked(type);
            int equippedCount = equippedCounts.TryGetValue(type, out int count) ? count : 0;

            if (_allSpellStatuses.ContainsKey(type))
            {
                // 既存のエントリを更新
                _allSpellStatuses[type].UpdateStatus(isUnlocked, totalCount, equippedCount);
            }
            else
            {
                // 新規エントリを作成
                _allSpellStatuses[type] = new SpellHoldStatus(type, isUnlocked, totalCount, equippedCount);
            }
        }

        // 通知
        _observer?.OnAllSpellStatusesChanged(GetAllSpellStatuses());
    }

    /// <summary>
    /// オブザーバー登録時などに、全種類の情報を一度に通知します。
    /// </summary>
    private void NotifyAllCurrentData()
    {
        // 持ち込み呪文と最大容量は EquippedSpellManager の RegisterObserver で通知されているはずですが、
        // Model の SetObserver でも念のため通知します。

        // 所持呪文情報
        _observer?.OnAllSpellStatusesChanged(GetAllSpellStatuses());

        // 持ち込み呪文リスト
        _observer?.OnEquippedSpellsChanged(_currentEquippedSpells);

        // 最大容量
        _observer?.OnMaxCapacityChanged(EquippedSpellManager.Instance.GetMaxCapacity());
    }


    // --- ISpellHoldInfoObserver の実装 ---

    // 呪文がアンロックされたとき
    public void OnSpellUnlocked(SpellType type)
    {
        // アンロック状態が変わったため、全所持呪文ステータスを再計算して通知
        RecalculateAllSpellStatusesAndNotify();
        Debug.Log($"[Model] SpellUnlocked: {type}. 全所持ステータスを更新し通知しました。");
    }

    // 呪文の保持数が変更されたとき
    public void OnSpellCountChanged(SpellType type, int newCount)
    {
        // 保持数が変わったため、全所持呪文ステータスを再計算して通知
        RecalculateAllSpellStatusesAndNotify();
        Debug.Log($"[Model] SpellCountChanged: {type} -> {newCount}. 全所持ステータスを更新し通知しました。");
    }


    // --- IEquippedSpellsObserver の実装 ---

    // 持ち込み呪文リストが変更されたとき
    public void OnEquippedSpellsChanged(List<SpellBase> currentSpells)
    {
        // 持ち込みリストを更新
        _currentEquippedSpells = currentSpells;

        // 持ち込みリストが変わると、各呪文の「AvailableCount」が変わるため、全所持ステータスの再計算が必要
        RecalculateAllSpellStatusesAndNotify();

        // 持ち込み呪文のリスト自体もコントローラーに通知
        _observer?.OnEquippedSpellsChanged(_currentEquippedSpells);
        Debug.Log($"[Model] EquippedSpellsChanged. 持ち込みリストと全所持ステータスを更新し通知しました。");
    }

    // 持ち込み可能な最大スロット数が変更されたとき
    public void OnMaxCapacityChanged(int newCapacity)
    {
        // 最大容量をコントローラーに通知
        _observer?.OnMaxCapacityChanged(newCapacity);
        // EquippedSpellManagerの配列サイズも変わっているため、EquippedSpellsChangedも続いて呼ばれることが期待されます
        Debug.Log($"[Model] MaxCapacityChanged: {newCapacity}. 通知しました。");
    }
}


/// <summary>
/// EquippedSpellModelから変更通知を受け取るためのインターフェース。
/// </summary>
public interface IEquippedSpellModelObserver
{
    /// <summary>
    /// 所持呪文に関する情報（在庫、アンロック状態）の全体が変更されたときに呼び出されます。
    /// </summary>
    /// <param name="allSpellStatuses">全SpellTypeの所持ステータスの配列</param>
    void OnAllSpellStatusesChanged(IReadOnlyList<SpellHoldStatus> allSpellStatuses);

    /// <summary>
    /// 持ち込み呪文リスト（スロットの内容）が変更されたときに呼び出されます。
    /// </summary>
    /// <param name="equippedSpells">変更後の持ち込み呪文のリスト（null要素を含む）</param>
    void OnEquippedSpellsChanged(IReadOnlyList<SpellBase> equippedSpells);

    /// <summary>
    /// 持ち込み可能な最大スロット数が変更されたときに呼び出されます。
    /// </summary>
    /// <param name="newCapacity">新しい最大スロット数</param>
    void OnMaxCapacityChanged(int newCapacity);
}

/// <summary>
/// 持ち込み呪文選択UIに表示するために必要な、個々の呪文のステータス情報。
/// </summary>
[Serializable]
public class SpellHoldStatus
{
    public SpellType Type { get; private set; }
    public bool IsUnlocked { get; private set; } // 開放済みか（使ったことがあるか）
    public int TotalCount { get; private set; } // 全所持数（インベントリ全体）
    public int EquippedCount { get; private set; } // 持ち込みスロットで使用中の数
    public int AvailableCount => TotalCount - EquippedCount; // 持ち込みに使用可能な残り数

    public SpellHoldStatus(SpellType type, bool isUnlocked, int totalCount, int equippedCount)
    {
        Type = type;
        IsUnlocked = isUnlocked;
        TotalCount = totalCount;
        EquippedCount = equippedCount;
    }

    /// <summary>
    /// ステータス情報を更新します。
    /// </summary>
    public void UpdateStatus(bool isUnlocked, int totalCount, int equippedCount)
    {
        IsUnlocked = isUnlocked;
        TotalCount = totalCount;
        EquippedCount = equippedCount;
    }
}