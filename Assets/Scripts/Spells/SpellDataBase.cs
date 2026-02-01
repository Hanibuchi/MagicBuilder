using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// SpellType.cs と SpellBase.cs、SpellDataEntry は別途定義が必要です。
// （ここではSpellDataEntryをこのファイル内に保持します）

/// <summary>
/// 呪文データアセットへのアクセスを管理するシングルトン（ScriptableObject版）。
/// Resourcesフォルダからロードされ、シーンに依存せず使用できます。
/// </summary>
[CreateAssetMenu(fileName = "SpellDatabase", menuName = "Wand System/Spell Database")]
public class SpellDatabase : ScriptableObject
{
    // --- シングルトン実装（Resourcesロードによる遅延初期化） ---

    // Resourcesフォルダ内のこのアセットのパス。
    // Unityの慣例により、Resources直下に配置する場合はフォルダ名を含めません。
    private const string DATABASE_RESOURCE_PATH = "SpellDatabase";

    private static SpellDatabase _instance;
    /// <summary>
    /// SpellDatabaseのシングルトンインスタンスを取得します。
    /// 初回アクセス時にResourcesフォルダからアセットをロードします。
    /// </summary>
    public static SpellDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                // Resourcesフォルダからアセットをロード
                _instance = Resources.Load<SpellDatabase>(DATABASE_RESOURCE_PATH);

                if (_instance == null)
                {
                    Debug.LogError($"SpellDatabaseアセットが 'Resources/{DATABASE_RESOURCE_PATH}.asset' に見つかりません！" +
                                   $"プロジェクト内にScriptableObjectアセットを作成し、Resourcesフォルダに配置してください。");
                }
                else
                {
                    // ロード成功後、辞書を初期化
                    _instance.InitializeDictionary();
                }
            }
            return _instance;
        }
    }

    // --- データと初期化 ---

    [Header("全ての呪文データのリスト (自動ロード)")]
    [Tooltip("実行時にResourcesフォルダ内のSpellBaseアセットから自動構築されます。")]
    public List<SpellDataEntry> allSpells = new List<SpellDataEntry>();

    /// <summary>
    /// データベースに登録されている、有効な全ての呪文のSpellTypeリストを取得します。
    /// </summary>
    public List<SpellType> GetAllRegisteredSpellTypes()
    {
        if (_spellDictionary == null) InitializeDictionary();
        
        return _spellDictionary.Keys.ToList();
    }

    // 高速検索のための辞書。一度初期化すれば再構築は不要。
    private Dictionary<SpellType, SpellBase> _spellDictionary;
    // 辞書のキーを SpellBase に変更し、値として SpellType を保持する（逆引き用）
    private Dictionary<SpellBase, SpellType> _reverseSpellDictionary;

    /// <summary>
    /// ResourcesからSpellBaseアセットを自動ロードし、辞書を構築します。
    /// </summary>
    private void InitializeDictionary()
    {
        if (_spellDictionary != null) return; // 既に初期化済みなら何もしない

        _spellDictionary = new Dictionary<SpellType, SpellBase>();
        _reverseSpellDictionary = new Dictionary<SpellBase, SpellType>();

        // Resourcesフォルダ以下の全てのSpellBase（およびその継承クラス）アセットをロード
        SpellBase[] allSpellAssets = Resources.LoadAll<SpellBase>("");

        foreach (var spellAsset in allSpellAssets)
        {
            if (spellAsset == null) continue;

            // SpellType.None のものは登録対象外とする
            if (spellAsset.spellType == SpellType.None)
            {
                // 必要に応じて警告を出す
                // Debug.LogWarning($"SpellBase '{spellAsset.name}' の SpellType が None のため、データベースに登録されません。");
                continue;
            }

            if (_spellDictionary.ContainsKey(spellAsset.spellType))
            {
                Debug.LogError($"SpellDatabase: SpellType.{spellAsset.spellType} が重複しています！ " +
                               $"既存: {_spellDictionary[spellAsset.spellType].name}, 新規: {spellAsset.name}");
                continue;
            }

            _spellDictionary.Add(spellAsset.spellType, spellAsset);
            _reverseSpellDictionary.Add(spellAsset, spellAsset.spellType);
        }

        // インスペクター表示用に allSpells リストを更新
        allSpells = _spellDictionary
            .Select(kvp => new SpellDataEntry { type = kvp.Key, spellAsset = kvp.Value })
            .OrderBy(e => e.type.ToString())
            .ToList();

        Debug.Log($"SpellDatabase: {_spellDictionary.Count} 個の呪文を Resources からロードしました。");
    }

    // --- 外部アクセス用メソッド ---

    /// <summary>
    /// Enumで指定した種類のSpellBaseアセットを高速で取得します。
    /// </summary>
    /// <param name="type">取得したい呪文の種類</param>
    /// <returns>対応するSpellBaseアセット</returns>
    public SpellBase GetSpellAsset(SpellType type)
    {
        // Instanceプロパティ経由でアクセスすれば辞書は初期化されているはず
        if (_spellDictionary == null)
        {
            Debug.LogError("SpellDatabaseが正しく初期化されていません。Instanceプロパティ経由でアクセスしてください。");
            return null;
        }

        if (_spellDictionary.TryGetValue(type, out SpellBase spell))
        {
            return spell;
        }

        Debug.LogError($"SpellType.{type} に対応する呪文アセットがデータベースに見つかりません！");
        return null;
    }

    /// <summary>
    /// SpellBaseアセットから対応するSpellTypeを取得します。（EquippedSpellManagerが使用）
    /// </summary>
    public SpellType GetSpellType(SpellBase asset)
    {
        if (_reverseSpellDictionary.TryGetValue(asset, out SpellType type))
        {
            return type;
        }
        Debug.LogError($"アセット {asset.name} に対応する SpellType がデータベースに見つかりません！");
        return SpellType.None;
    }

    // --- 補助クラス（必要に応じて別途ファイルに分割しても良い） ---

    [System.Serializable]
    public class SpellDataEntry
    {
        [Tooltip("対応する呪文の種類")]
        public SpellType type;

        [Tooltip("対応するSpellBaseアセットへの参照")]
        public SpellBase spellAsset;
    }
}