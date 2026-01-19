// EquippedSpellManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 持ち込み呪文の管理ロジック。
/// シングルトンパターン（MonoBehaviour）。PlayerPrefsで永続化を行う。
/// 持ち込み呪文は固定配列として扱い、編集は上書きロジックで行われます。
/// </summary>
public class EquippedSpellManager : MonoBehaviour
{
    // --- 定数 ---

    private const string PLAYERPREFS_KEY_SPELLS = "EquippedSpells_Types";
    private const string PLAYERPREFS_KEY_CAPACITY = "EquippedSpells_Capacity";
    private const int DEFAULT_CAPACITY = 1; // 初期持ち込み可能数

    // --- シングルトン実装 ---

    private static EquippedSpellManager _instance;
    public static EquippedSpellManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject singletonObject = new GameObject(nameof(EquippedSpellManager));
                _instance = singletonObject.AddComponent<EquippedSpellManager>();
                // シーンを跨いでも破棄されないように設定
                DontDestroyOnLoad(singletonObject);
                _instance.LoadConfig();
                _instance.LoadCapacity();
                _instance.LoadEquippedSpells();
            }
            return _instance;
        }
    }

    // Awakeでインスタンスを設定し、2つ目が生成されないようにする
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
    }

    // --- 内部データとオブザーバー ---

    // 持ち込み呪文を格納する配列。SpellBaseはScriptableObjectなので直接参照を保持。
    // 固定長（最大スロット数）となり、空きスロットは null で表現します。
    private SpellBase[] _equippedSpells = new SpellBase[DEFAULT_CAPACITY];

    // 持ち込み可能な最大スロット数
    private int _maxCapacity = DEFAULT_CAPACITY;

    private EquippedSpellCapacityConfig _config;
    private InitialEquippedSpellsConfig _initialConfig;

    // 変更通知用オブザーバー（1つのみ登録可能）
    private IEquippedSpellsObserver _observer;

    // --- 永続化（PlayerPrefs） ---

    /// <summary>
    /// Resourcesから設定ファイルを読み込みます。
    /// </summary>
    private void LoadConfig()
    {
        _config = Resources.Load<EquippedSpellCapacityConfig>("EquippedSpellCapacityConfig");
        if (_config == null)
        {
            Debug.LogError("EquippedSpellCapacityConfig が Resources/EquippedSpellCapacityConfig に見つかりません。デフォルト設定を使用します。");
            // 必要に応じてデフォルト値を生成するか、エラーとして扱う
        }

        _initialConfig = Resources.Load<InitialEquippedSpellsConfig>("InitialEquippedSpellsConfig");
        if (_initialConfig == null)
        {
            Debug.Log("InitialEquippedSpellsConfig が Resources/InitialEquippedSpellsConfig に見つかりません。初期装備なしで開始します。");
        }
    }

    /// <summary>
    /// PlayerPrefsに現在の持ち込み呪文リストを保存します。
    /// SpellDatabaseを利用し、SpellTypeのカンマ区切り文字列として保存します。
    /// null要素は "NULL" または空文字として保存することを想定します。
    /// </summary>
    private void SaveEquippedSpells()
    {
        // null の場合は空文字 "" を、それ以外は SpellType の文字列を使用
        var spellTypeStrings = _equippedSpells
        .Select(spell => spell == null ? "" : SpellDatabase.Instance.GetSpellType(spell).ToString())
        .ToList();

        // 文字列の配列をカンマ区切り文字列に変換
        string dataToSave = string.Join(",", spellTypeStrings);
        PlayerPrefs.SetString(PLAYERPREFS_KEY_SPELLS, dataToSave);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// PlayerPrefsから持ち込み呪文リストを読み込み、_equippedSpellsを復元します。
    /// PlayerPrefsにはSpellTypeまたは空文字のカンマ区切り文字列が保存されている前提です。
    /// </summary>
    private void LoadEquippedSpells()
    {
        // _equippedSpells のサイズを現在の _maxCapacity に設定
        _equippedSpells = new SpellBase[_maxCapacity];

        // 1. 保存されたカンマ区切り文字列を取得
        if (!PlayerPrefs.HasKey(PLAYERPREFS_KEY_SPELLS))
        {
            Debug.Log("持ち込み呪文の保存データが見つかりませんでした。初期状態で開始します。");
            ApplyInitialSpells();
            return;
        }

        string dataToLoad = PlayerPrefs.GetString(PLAYERPREFS_KEY_SPELLS);

        if (string.IsNullOrEmpty(dataToLoad))
        {
            // データが空の場合は null で初期化（既に new で初期化済み）
            ApplyInitialSpells(); // 空の場合も初期装備を試みる
            return;
        }

        // 2. 文字列をSpellTypeのリストに変換
        var spellTypeStrings = dataToLoad.Split(',');

        for (int i = 0; i < spellTypeStrings.Length; i++)
        {
            // 最大容量を超過している要素は無視
            if (i >= _maxCapacity) break;

            var spellTypeString = spellTypeStrings[i];

            // 空文字（保存時の null 要素）の場合はスキップし、null のままにする
            if (string.IsNullOrEmpty(spellTypeString))
            {
                _equippedSpells[i] = null;
                continue;
            }

            // 文字列をSpellTypeのEnumにパース
            if (System.Enum.TryParse(spellTypeString, out SpellType type))
            {
                // SpellDatabaseを利用してSpellTypeからSpellBaseのインスタンスを取得
                var spellInstance = SpellDatabase.Instance.GetSpellAsset(type);

                if (spellInstance != null)
                {
                    _equippedSpells[i] = spellInstance;
                }
                else
                {
                    Debug.LogError($"SpellType: {type} に対応する呪文インスタンスがデータベースに見つかりませんでした。");
                }
            }
            else
            {
                Debug.LogError($"無効なSpellType文字列が保存されていました: {spellTypeString}");
            }
        }
    }

    /// <summary>
    /// 初期装備を適用し、所持情報も更新します。
    /// </summary>
    private void ApplyInitialSpells()
    {
        if (_initialConfig == null || _initialConfig.initialSpells == null) return;

        for (int i = 0; i < _initialConfig.initialSpells.Count; i++)
        {
            if (i >= _maxCapacity) break;

            var spell = _initialConfig.initialSpells[i];
            if (spell != null)
            {
                _equippedSpells[i] = spell;

                // 初期装備を所持・開放済みに設定（SpellHoldInfoManagerが初期化された後に実行される必要あり）
                SpellType type = SpellDatabase.Instance.GetSpellType(spell);
                if (SpellHoldInfoManager.Instance.GetSpellCount(type) <= 0)
                {
                    SpellHoldInfoManager.Instance.IncreaseSpellCount(type);
                }
            }
        }

        // 初期状態を保存
        SaveEquippedSpells();
    }

    /// <summary>
    /// PlayerPrefsに最大持ち込みスロット数を保存します。
    /// </summary>
    private void SaveCapacity()
    {
        PlayerPrefs.SetInt(PLAYERPREFS_KEY_CAPACITY, _maxCapacity);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// PlayerPrefsから最大持ち込みスロット数をロードします。
    /// </summary>
    private void LoadCapacity()
    {
        _maxCapacity = PlayerPrefs.GetInt(PLAYERPREFS_KEY_CAPACITY, DEFAULT_CAPACITY);
        NotifyMaxCapacityChanged();
    }

    // --- 外部アクセス用メソッド ---

    /// <summary>
    /// 現在の持ち込み呪文リストを取得します。
    /// 外部からの変更を防ぐため、読み取り専用のコピーを返します。
    /// (null要素を含むことがあります)
    /// </summary>
    public IReadOnlyList<SpellBase> GetEquippedSpells()
    {
        // 外部から配列が直接操作されないように、新しいリストとして返す
        return new List<SpellBase>(_equippedSpells);
    }

    /// <summary>
    /// 持ち込み可能な最大スロット数を取得します。
    /// </summary>
    public int GetMaxCapacity()
    {
        return _maxCapacity;
    }

    /// <summary>
    /// 現在の容量に基づいた拡張コストを取得します。
    /// </summary>
    public int GetCapacityUpgradeCost()
    {
        if (_config == null || _config.capacityUpgradeCosts == null || _config.capacityUpgradeCosts.Length == 0) return 0;

        int index = Mathf.Clamp(_maxCapacity, 0, _config.capacityUpgradeCosts.Length - 1);
        return _config.capacityUpgradeCosts[index];
    }

    /// <summary>
    /// インデックスを指定して持ち込み呪文を削除します。
    /// 実際には、そのスロットを null で上書きします。
    /// </summary>
    /// <param name="index">削除したい呪文のインデックス</param>
    public void RemoveSpell(int index)
    {
        if (index >= 0 && index < _maxCapacity)
        {
            _equippedSpells[index] = null; // nullで上書き

            SaveEquippedSpells();
            NotifyEquippedSpellsChanged();
        }
        else
        {
            Debug.LogError($"無効なインデックス {index} で呪文を削除しようとしました。現在の最大スロット数: {_maxCapacity}");
        }
    }

    /// <summary>
    /// インデックスを指定して持ち込み呪文を置き換えます。
    /// 既に呪文がセットされていても上書きされます。
    /// </summary>
    /// <param name="index">セット/置き換えたいインデックス</param>
    /// <param name="spellAsset">セットするSpellBaseアセット</param>
    /// <returns>セットに成功した場合は true、インデックスが無効な場合は false</returns>
    public bool SetSpell(int index, SpellBase spellAsset)
    {
        if (spellAsset == null)
        {
            // nullをセットしたい場合は RemoveSpell を利用
            Debug.LogError("追加しようとした呪文アセットがnullです。スロットを空ける場合は RemoveSpell(index) を利用してください。");
            return false;
        }

        if (index < 0 || index >= _maxCapacity)
        {
            Debug.LogError($"インデックス {index} が無効か、最大スロット数 {_maxCapacity} を超えています。");
            return false;
        }

        _equippedSpells[index] = spellAsset;

        SaveEquippedSpells();
        NotifyEquippedSpellsChanged();
        return true;
    }

    /// <summary>
    /// 持ち込める呪文の最大数を増やします。
    /// 配列のサイズ変更に伴い、既存の呪文は保持しつつ配列を再構築します。
    /// </summary>
    /// <param name="amount">増加させる数</param>
    public void IncreaseCapacity(int amount = 1)
    {
        if (amount <= 0) return;

        int newCapacity = _maxCapacity + amount;

        // 新しい配列を作成し、既存の呪文をコピー
        SpellBase[] newSpells = new SpellBase[newCapacity];
        for (int i = 0; i < _equippedSpells.Length; i++)
        {
            newSpells[i] = _equippedSpells[i];
        }

        _equippedSpells = newSpells;
        _maxCapacity = newCapacity;

        SaveCapacity();
        NotifyMaxCapacityChanged();
        NotifyEquippedSpellsChanged();
    }

    // --- オブザーバー管理と通知 ---

    /// <summary>
    /// 持ち込み呪文変更通知のオブザーバーを登録します。（単一登録）
    /// </summary>
    public void RegisterObserver(IEquippedSpellsObserver observer)
    {
        if (_observer != null)
        {
            Debug.LogWarning("既にオブザーバーが登録されています。上書きします。");
        }
        _observer = observer;

        // 登録直後に最新の情報を通知する
        NotifyEquippedSpellsChanged();
    }

    /// <summary>
    /// 持ち込み呪文変更通知のオブザーバーを解除します。
    /// </summary>
    public void UnregisterObserver()
    {
        _observer = null;
    }

    private void NotifyEquippedSpellsChanged()
    {
        // 配列の内容を新しいリストとしてオブザーバーに通知
        _observer?.OnEquippedSpellsChanged(new List<SpellBase>(_equippedSpells));
    }

    private void NotifyMaxCapacityChanged()
    {
        _observer?.OnMaxCapacityChanged(_maxCapacity);
    }

    // [SerializeField] int test_index;
    // [SerializeField] SpellBase test_spellBase;
    // public void Test_Remove()
    // {
    //     RemoveSpell(test_index);
    // }
    // public void Test_Insert()
    // {
    //     SetSpell(test_index, test_spellBase);
    // }
    // [SerializeField] List<SpellBase> test_getSpells;
    // public void Test_Get()
    // {
    //     test_getSpells = new List<SpellBase>(GetEquippedSpells());
    // }
    // public void Test_AddCapacity()
    // {
    //     IncreaseCapacity();
    // }
}

/// <summary>
/// 持ち込み呪文リストが変更された際に通知を受け取るためのインターフェース。
/// </summary>
public interface IEquippedSpellsObserver
{
    /// <summary>
    /// 持ち込み呪文リストが変更されたときに呼び出されます。
    /// (固定配列の内容がリストとして渡されます。null要素を含むことがあります)
    /// </summary>
    /// <param name="currentSpells">変更後の持ち込み呪文のリスト</param>
    void OnEquippedSpellsChanged(List<SpellBase> currentSpells);

    /// <summary>
    /// 持ち込み可能な最大スロット数が変更されたときに呼び出されます。
    /// </summary>
    /// <param name="newCapacity">新しい最大スロット数</param>
    void OnMaxCapacityChanged(int newCapacity);
}