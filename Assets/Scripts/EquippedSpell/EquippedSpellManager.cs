// EquippedSpellManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 持ち込み呪文の管理ロジック。
/// シングルトンパターン（MonoBehaviour）。PlayerPrefsで永続化を行う。
/// </summary>
public class EquippedSpellManager : MonoBehaviour
{
    // --- 定数 ---

    private const string PLAYERPREFS_KEY_SPELLS = "EquippedSpells_Types";
    private const string PLAYERPREFS_KEY_CAPACITY = "EquippedSpells_Capacity";
    private const int DEFAULT_CAPACITY = 1; // 初期持ち込み可能数

    // --- シングルトン実装 ---

    public static EquippedSpellManager Instance { get; private set; }

    // Awakeでインスタンスを設定し、2つ目が生成されないようにする
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadEquippedSpells(); // Awakeでもロードを実行
            LoadCapacity();
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // 既に存在するインスタンスがあれば破棄
        }
    }

    // --- 内部データとオブザーバー ---

    // 持ち込み呪文を格納するリスト。SpellBaseはScriptableObjectなので直接参照を保持。
    List<SpellBase> _equippedSpells = new List<SpellBase>();

    // 持ち込み可能な最大スロット数
    private int _maxCapacity = DEFAULT_CAPACITY;

    // 変更通知用オブザーバー（1つのみ登録可能）
    private IEquippedSpellsObserver _observer;

    // --- 永続化（PlayerPrefs） ---

    /// <summary>
    /// PlayerPrefsに現在の持ち込み呪文リストを保存します。
    /// SpellDatabaseを利用し、SpellTypeのカンマ区切り文字列として保存します。
    /// </summary>
    private void SaveEquippedSpells()
    {
        var spellTypes = _equippedSpells
        .Select(spell => SpellDatabase.Instance.GetSpellType(spell).ToString())
        .ToList();

        // intのリストをカンマ区切り文字列に変換
        string dataToSave = string.Join(",", spellTypes);
        PlayerPrefs.SetString(PLAYERPREFS_KEY_SPELLS, dataToSave);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// PlayerPrefsから持ち込み呪文リストを読み込み、_equippedSpellsを復元します。
    /// PlayerPrefsにはSpellTypeのカンマ区切り文字列が保存されている前提です。
    /// </summary>
    private void LoadEquippedSpells()
    {
        // 1. 保存されたカンマ区切り文字列を取得
        if (!PlayerPrefs.HasKey(PLAYERPREFS_KEY_SPELLS))
        {
            // キーが存在しない場合は、初期化処理を行うか、何もしない
            Debug.Log("持ち込み呪文の保存データが見つかりませんでした。");
            return;
        }

        string dataToLoad = PlayerPrefs.GetString(PLAYERPREFS_KEY_SPELLS);

        if (string.IsNullOrEmpty(dataToLoad))
        {
            // データが空の場合はリストをクリアして終了
            _equippedSpells.Clear();
            return;
        }

        // 2. 文字列をSpellTypeのリストに変換
        var spellTypeStrings = dataToLoad.Split(',');

        var loadedSpells = new List<SpellBase>(); // 戻り値として_equippedSpellsに設定するリスト

        foreach (var spellTypeString in spellTypeStrings)
        {
            // 文字列をSpellTypeのEnumにパース
            // ここでエラー処理としてTryParseを使用するとより堅牢になります
            if (System.Enum.TryParse(spellTypeString, out SpellType type))
            {
                // SpellDatabaseを利用してSpellTypeからSpellBaseのインスタンスを取得
                // ここで、SpellDatabaseにGetSpellAsset(SpellType type)のようなメソッドがある前提
                var spellInstance = SpellDatabase.Instance.GetSpellAsset(type);

                if (spellInstance != null)
                {
                    loadedSpells.Add(spellInstance);
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

        // 3. 復元したリストをメンバ変数に設定
        _equippedSpells = loadedSpells;
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
    /// </summary>
    public IReadOnlyList<SpellBase> GetEquippedSpells()
    {
        // 外部からリストが直接操作されないように、新しいリストとして返す
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
    /// インデックスを指定して持ち込み呪文を削除します。
    /// </summary>
    /// <param name="index">削除したい呪文のインデックス</param>
    public void RemoveSpell(int index)
    {
        if (index >= 0 && index < _equippedSpells.Count)
        {
            _equippedSpells.RemoveAt(index);

            SaveEquippedSpells();
            NotifyEquippedSpellsChanged();
        }
        else
        {
            Debug.LogError($"無効なインデックス {index} で呪文を削除しようとしました。現在のスロット数: {_equippedSpells.Count}");
        }
    }

    /// <summary>
    /// インデックスを指定して持ち込み呪文を追加または置き換えます。
    /// </summary>
    /// <param name="index">追加/置き換えたいインデックス</param>
    /// <param name="spellAsset">追加するSpellBaseアセット</param>
    /// <returns>追加に成功した場合は true、最大数を超えているなどの理由で失敗した場合は false</returns>
    public bool InsertSpell(int index, SpellBase spellAsset)
    {
        if (spellAsset == null)
        {
            Debug.LogError("追加しようとした呪文アセットがnullです。");
            return false;
        }

        if (index < 0 || index >= _maxCapacity)
        {
            Debug.LogError($"インデックス {index} が無効か、最大スロット数 {_maxCapacity} を超えています。");
            return false;
        }

        if (_equippedSpells.Count < _maxCapacity)
        {
            _equippedSpells.Insert(index, spellAsset);
        }
        else
        {
            // 最大数を超えて追加しようとした
            Debug.LogWarning($"持ち込みスロットは既に満杯です (最大: {_maxCapacity})。");
            return false;
        }

        SaveEquippedSpells();
        NotifyEquippedSpellsChanged();
        return true;
    }

    /// <summary>
    /// 持ち込める呪文の最大数を増やします。
    /// </summary>
    /// <param name="amount">増加させる数</param>
    public void IncreaseCapacity(int amount = 1)
    {
        if (amount <= 0) return;

        _maxCapacity += amount;

        SaveCapacity();
        NotifyMaxCapacityChanged();
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
        NotifyMaxCapacityChanged();
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
    //     InsertSpell(test_index, test_spellBase);
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
    /// </summary>
    /// <param name="currentSpells">変更後の持ち込み呪文のリスト</param>
    void OnEquippedSpellsChanged(List<SpellBase> currentSpells);

    /// <summary>
    /// 持ち込み可能な最大スロット数が変更されたときに呼び出されます。
    /// </summary>
    /// <param name="newCapacity">新しい最大スロット数</param>
    void OnMaxCapacityChanged(int newCapacity);
}