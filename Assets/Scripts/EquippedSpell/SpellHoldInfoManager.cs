using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// 呪文の保持数と開放（使ったことがあるか）情報を管理するシングルトンクラス。
/// PlayerPrefsでデータを永続化します。
/// </summary>
public class SpellHoldInfoManager : MonoBehaviour
{
    // --- シングルトン実装 ---
    private static SpellHoldInfoManager _instance;
    public static SpellHoldInfoManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject singletonObject = new GameObject(nameof(SpellHoldInfoManager));
                _instance = singletonObject.AddComponent<SpellHoldInfoManager>();
                // シーンを跨いでも破棄されないように設定
                DontDestroyOnLoad(singletonObject);
                _instance.InitializeData();
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
            _instance.InitializeData();
        }
        else if (_instance != this)
        {
            // 既にインスタンスが存在する場合、自分自身を破棄
            Destroy(gameObject);
        }
    }

    // --- データ構造と永続化キー ---

    private const string PREF_KEY_COUNT_PREFIX = "SpellCount_";
    private const string PREF_KEY_UNLOCKED_PREFIX = "SpellUnlocked_";

    // 呪文の保持数: Key=SpellType, Value=保持数
    private Dictionary<SpellType, int> _spellCounts = new Dictionary<SpellType, int>();
    // 呪文の開放情報: Key=SpellType, Value=開放済みか
    private Dictionary<SpellType, bool> _spellUnlockedStatus = new Dictionary<SpellType, bool>();

    // 外部通知用（登録できるのは1つのみ）
    private ISpellHoldInfoObserver _observer;

    // --- 初期化処理 ---

    /// <summary>
    /// SpellDatabaseから全SpellTypeを取得し、PlayerPrefsから対応する情報を読み込みます。
    /// </summary>
    private void InitializeData()
    {
        if (SpellDatabase.Instance == null)
        {
            Debug.LogError("SpellDatabase.Instance が null です。先にSpellDatabaseがロードされていることを確認してください。");
            return;
        }

        var allRegisteredTypes = SpellDatabase.Instance.GetAllRegisteredSpellTypes();

        if (allRegisteredTypes.Count == 0 && SpellDatabase.Instance.allSpells.Count > 0)
        {
            Debug.LogWarning("SpellDatabaseに登録された呪文のアセットがすべてnullの可能性があります。");
        }

        foreach (SpellType type in allRegisteredTypes)
        {
            // Noneタイプを除外
            if (type == SpellType.None) continue;

            // 保持数の読み込み (デフォルトは 0)
            string countKey = PREF_KEY_COUNT_PREFIX + type.ToString();
            int count = PlayerPrefs.GetInt(countKey, 0);
            _spellCounts[type] = count;

            // 開放情報の読み込み (デフォルトは false)
            string unlockedKey = PREF_KEY_UNLOCKED_PREFIX + type.ToString();
            // PlayerPrefs.GetIntで0=false, 1=trueとして読み込む
            bool unlocked = PlayerPrefs.GetInt(unlockedKey, 0) == 1;
            _spellUnlockedStatus[type] = unlocked;
        }

        // 初期状態で保持数が1以上の呪文は「開放済み」と見なす
        foreach (var pair in _spellCounts)
        {
            if (pair.Value > 0)
            {
                _spellUnlockedStatus[pair.Key] = true;
            }
        }
    }

    /// <summary>
    /// 現在の保持数と開放情報をPlayerPrefsに保存します。
    /// </summary>
    private void SaveAllData()
    {
        foreach (var pair in _spellCounts)
        {
            string countKey = PREF_KEY_COUNT_PREFIX + pair.Key.ToString();
            PlayerPrefs.SetInt(countKey, pair.Value);
        }

        foreach (var pair in _spellUnlockedStatus)
        {
            string unlockedKey = PREF_KEY_UNLOCKED_PREFIX + pair.Key.ToString();
            // boolを int (0 or 1) に変換して保存
            PlayerPrefs.SetInt(unlockedKey, pair.Value ? 1 : 0);
        }

        PlayerPrefs.Save(); // 変更をディスクに書き込み
    }

    // --- 外部アクセス用メソッド ---

    /// <summary>
    /// 変更通知を受け取るオブザーバーをセットします。（1つのみ登録可能）
    /// </summary>
    public void SetObserver(ISpellHoldInfoObserver observer)
    {
        _observer = observer;
    }

    /// <summary>
    /// 特定の呪文を使ったことがある（＝開放済み）状態にします。
    /// </summary>
    /// <param name="type">対象の呪文のSpellType</param>
    public void UnlockSpell(SpellType type)
    {
        if (_spellUnlockedStatus.ContainsKey(type) && _spellUnlockedStatus[type])
        {
            return; // 既に開放済み
        }

        _spellUnlockedStatus[type] = true;

        // 通知
        _observer?.OnSpellUnlocked(type);

        SaveAllData();
    }

    /// <summary>
    /// 特定の呪文の保持数を1増やします。
    /// </summary>
    /// <param name="type">対象の呪文のSpellType</param>
    public void IncreaseSpellCount(SpellType type)
    {
        if (!_spellCounts.ContainsKey(type))
        {
            // 新しい呪文の場合は初期化
            _spellCounts[type] = 0;
            _spellUnlockedStatus[type] = false; // 保持数増加はドロップ時なので、基本的に開放済みになる
        }

        _spellCounts[type]++;
        int newCount = _spellCounts[type];

        // 保持数が1以上になった場合、自動的に開放済みと見なす
        if (!_spellUnlockedStatus[type])
        {
            UnlockSpell(type); // UnlockSpell内でSaveAllDataが呼ばれる
        }
        else
        {
            SaveAllData();
        }

        // 通知
        _observer?.OnSpellCountChanged(type, newCount);
    }

    /// <summary>
    /// 特定の呪文の保持数を1減らします。
    /// </summary>
    /// <param name="type">対象の呪文のSpellType</param>
    /// <returns>減少が成功したかどうか</returns>
    public bool DecreaseSpellCount(SpellType type)
    {
        if (!_spellCounts.ContainsKey(type) || _spellCounts[type] <= 0)
        {
            Debug.LogWarning($"SpellType.{type} の保持数が0以下のため減少できませんでした。");
            return false;
        }

        _spellCounts[type]--;
        int newCount = _spellCounts[type];

        // 通知
        _observer?.OnSpellCountChanged(type, newCount);

        SaveAllData();
        return true;
    }


    /// <summary>
    /// 特定の呪文の現在の保持数を取得します。
    /// </summary>
    /// <param name="type">対象の呪文のSpellType</param>
    /// <returns>現在の保持数</returns>
    public int GetSpellCount(SpellType type)
    {
        if (_spellCounts.TryGetValue(type, out int count))
        {
            return count;
        }
        return 0; // データがない場合は0
    }

    /// <summary>
    /// 特定の呪文が開放されている（使ったことがある）かを取得します。
    /// </summary>
    /// <param name="type">対象の呪文のSpellType</param>
    /// <returns>開放済みであれば true</returns>
    public bool IsSpellUnlocked(SpellType type)
    {
        if (_spellUnlockedStatus.TryGetValue(type, out bool unlocked))
        {
            return unlocked;
        }
        return false; // データがない場合は未開放
    }


    // SpellHoldInfoManager.cs 内に追記

    // [Header("--- デバッグ/テスト用設定 ---")]
    // [Tooltip("テスト対象とする呪文の SpellType")]
    // public SpellType test_targetSpellType = SpellType.ExampleSpell;

    // [Tooltip("保持数を増減させる量")]
    // public int test_countChangeAmount = 1;


    // ----------------------------------------------------------------------------------
    // デバッグ/テスト用メソッド (Buttonから呼び出し可能)
    // ----------------------------------------------------------------------------------

    // /// <summary>
    // /// 現在の保持数と開放状態をログに出力します。
    // /// </summary>
    // public void Test_LogCurrentStatus()
    // {
    //     int count = GetSpellCount(test_targetSpellType);
    //     bool unlocked = IsSpellUnlocked(test_targetSpellType);

    //     Debug.Log($"<color=cyan>[SpellHoldInfoManager Test]</color> " +
    //               $"Target: **{test_targetSpellType}**\n" +
    //               $"- 保持数: {count}\n" +
    //               $"- 開放済み: {unlocked}");
    // }

    // /// <summary>
    // /// test_targetSpellType の保持数を test_countChangeAmount だけ増やします。
    // /// </summary>
    // public void Test_IncreaseSpellCount()
    // {
    //     for (int i = 0; i < test_countChangeAmount; i++)
    //     {
    //         IncreaseSpellCount(test_targetSpellType);
    //     }
    //     Debug.Log($"<color=lime>[SpellHoldInfoManager Test]</color> " +
    //               $"**{test_targetSpellType}** の保持数を {test_countChangeAmount} 増やしました。現在の数: {GetSpellCount(test_targetSpellType)}");
    // }

    // /// <summary>
    // /// test_targetSpellType の保持数を test_countChangeAmount だけ減らします。
    // /// </summary>
    // public void Test_DecreaseSpellCount()
    // {
    //     int successfulDecrements = 0;
    //     for (int i = 0; i < test_countChangeAmount; i++)
    //     {
    //         if (DecreaseSpellCount(test_targetSpellType))
    //         {
    //             successfulDecrements++;
    //         }
    //     }
    //     Debug.Log($"<color=yellow>[SpellHoldInfoManager Test]</color> " +
    //               $"**{test_targetSpellType}** の保持数を {successfulDecrements} 減らしました。現在の数: {GetSpellCount(test_targetSpellType)}");
    // }

    // /// <summary>
    // /// test_targetSpellType を開放済みにします。
    // /// </summary>
    // public void Test_UnlockSpell()
    // {
    //     UnlockSpell(test_targetSpellType);
    //     Debug.Log($"<color=lime>[SpellHoldInfoManager Test]</color> " +
    //               $"**{test_targetSpellType}** を強制的に開放済みにしました。状態: {IsSpellUnlocked(test_targetSpellType)}");
    // }

    // /// <summary>
    // /// PlayerPrefs の全データを削除し、Managerをリロードします。
    // /// </summary>
    // public void Test_ResetAllDataAndReload()
    // {
    //     PlayerPrefs.DeleteAll();
    //     // Unityエディタでテストする場合、再初期化のために一度オブジェクトを破棄してからインスタンスを再取得するのが確実です。
    //     // シーン内に SpellHoldInfoManager の GameObject があることを前提とします。
    //     Destroy(gameObject);

    //     // 次にアクセスしたときにシングルトンが再構築されます（ゲーム実行中のみ有効）
    //     Debug.Log("<color=red>[SpellHoldInfoManager Test]</color> **全データ削除**しました。ゲームを再起動するか、手動で次のアクセスをトリガーしてください。");
    // }
}

/// <summary>
/// SpellHoldInfoManagerで呪文情報が変更された際に通知を受け取るためのインターフェース。
/// </summary>
public interface ISpellHoldInfoObserver
{
    /// <summary>
    /// 特定の呪文が初めて開放された（使われた）ときに呼び出されます。
    /// </summary>
    /// <param name="type">開放された呪文のSpellType</param>
    void OnSpellUnlocked(SpellType type);

    /// <summary>
    /// 特定の呪文の保持数が変更されたときに呼び出されます。
    /// </summary>
    /// <param name="type">保持数が変更された呪文のSpellType</param>
    /// <param name="newCount">変更後の新しい保持数</param>
    void OnSpellCountChanged(SpellType type, int newCount);
}