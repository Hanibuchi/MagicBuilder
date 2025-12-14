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

    [Header("全ての呪文データのリスト")]
    [Tooltip("InspectorでSpellTypeとSpellBaseアセットを設定します。")]
    public List<SpellDataEntry> allSpells = new List<SpellDataEntry>();

    // 高速検索のための辞書。一度初期化すれば再構築は不要。
    private Dictionary<SpellType, SpellBase> _spellDictionary;

    /// <summary>
    /// SpellDataEntryリストから辞書を構築します。（ScriptableObjectロード後に実行）
    /// </summary>
    private void InitializeDictionary()
    {
        if (_spellDictionary != null) return; // 既に初期化済みなら何もしない

        try
        {
            // ToDictionaryを使って、allSpellsリストから一発で辞書を作成
            _spellDictionary = allSpells.ToDictionary(e => e.type, e => e.spellAsset);
        }
        catch (System.ArgumentException e)
        {
            Debug.LogError($"SpellDatabaseの初期化中に重複したSpellTypeが見つかりました: {e.Message}");
            _spellDictionary = new Dictionary<SpellType, SpellBase>();
        }
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