using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// SpellType.cs と SpellDataEntry は前回の定義をそのまま使用します。

/// <summary>
/// 全ての呪文データアセットへのアクセスを管理するシングルトン（MonoBehaviour版）。
/// シーンロード時に一度だけ初期化されます。
/// </summary>
public class SpellDatabase : MonoBehaviour
{
    // --- シングルトン実装 ---
    private static SpellDatabase instance;
    public static SpellDatabase Instance => instance;

    // --- データと初期化 ---

    [Header("全ての呪文データのリスト")]
    [Tooltip("InspectorでSpellTypeとSpellBaseアセットを設定します。")]
    public List<SpellDataEntry> allSpells = new List<SpellDataEntry>();

    // 高速検索のための辞書。一度初期化すれば再構築は不要。
    private Dictionary<SpellType, SpellBase> _spellDictionary;

    private void Awake()
    {
        // シングルトンの重複防止
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        // シーンを跨いで保持したい場合は DontDestroyOnLoad(gameObject); を追加

        // 初回アクセス時またはシーン開始時に一度だけ辞書を初期化（高パフォーマンス化）
        InitializeDatabase();
    }

    /// <summary>
    /// SpellDataEntryリストから辞書を構築します。
    /// </summary>
    private void InitializeDatabase()
    {
        if (_spellDictionary == null)
        {
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
    }

    // --- 外部アクセス用メソッド ---

    /// <summary>
    /// Enumで指定した種類のSpellBaseアセットを高速で取得します。
    /// </summary>
    /// <param name="type">取得したい呪文の種類</param>
    /// <returns>対応するSpellBaseアセット</returns>
    public SpellBase GetSpellAsset(SpellType type)
    {
        // 辞書が未初期化の場合、フォールバックとして初期化を試みる
        if (_spellDictionary == null)
        {
            InitializeDatabase();
        }

        if (_spellDictionary.TryGetValue(type, out SpellBase spell))
        {
            return spell;
        }

        Debug.LogError($"SpellType.{type} に対応する呪文アセットがデータベースに見つかりません！");
        return null;
    }
}

// SpellDatabase.cs と同じファイルで定義
[System.Serializable]
public class SpellDataEntry
{
    [Tooltip("対応する呪文の種類")]
    public SpellType type;

    [Tooltip("対応するSpellBaseアセットへの参照")]
    public SpellBase spellAsset;
}
