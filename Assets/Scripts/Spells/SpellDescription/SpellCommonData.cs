using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 全ての呪文で共通利用されるデータ（アイコン、エフェクトのプリセットなど）を管理するクラス。
/// Singletonパターンでアセットとして管理すると便利。
/// </summary>
[CreateAssetMenu(fileName = "SpellCommonData", menuName = "Wand System/Spell Common Data")]
public class SpellCommonData : ScriptableObject
{
    // 静的インスタンスのプライベートフィールド
    private static SpellCommonData _instance;

    /// <summary>
    /// SpellCommonDataのシングルトンインスタンスを取得します。
    /// 初めてアクセスされた時に、"Resources"フォルダからアセットをロードします。
    /// </summary>
    public static SpellCommonData Instance
    {
        get
        {
            // インスタンスがまだ設定されていない場合
            if (_instance == null)
            {
                // Resourcesフォルダからアセットをロードします。
                // アセットの名前は "SpellCommonData" とします。
                _instance = Resources.Load<SpellCommonData>("SpellCommonData");

                // ロードに失敗した場合（アセットが見つからない場合など）
                if (_instance == null)
                {
                    Debug.LogError("SpellCommonDataアセットが見つかりません。" +
                                   "アセットが 'Resources' フォルダ内に 'SpellCommonData.asset' という名前で存在するか確認してください。");
                }
            }
            return _instance;
        }
    }

    [Header("共通アイコン")]
    [Tooltip("クールタイム項目に使用するアイコン")]
    public Sprite coolDownIcon;

    [Tooltip("HPに使用するアイコン")]
    public Sprite HPIcon;

    [Tooltip("スケールに使用するアイコン")]
    public Sprite scaleIcon;

    [Tooltip("誤差角度に使用するアイコン")]
    public Sprite errorDegreeIcon;

    [Tooltip("通常ダメージに使用するアイコン")]
    public Sprite damageIcon;

    [Tooltip("炎ダメージに使用するアイコン")]
    public Sprite fireDamageIcon;
    [Tooltip("氷ダメージに使用するアイコン")]
    public Sprite iceDamageIcon;
    [Tooltip("水ダメージに使用するアイコン")]
    public Sprite waterDamageIcon;
    [Tooltip("木ダメージに使用するアイコン")]
    public Sprite woodDamageIcon;
    [Tooltip("ノックバックに使用するアイコン")]
    public Sprite knockbackIcon;

    [Tooltip("呪文UIプレハブ")] // ランクや種類別でUIを変えるってなったときは、これを複数作ってSpellBaseの子クラスのCreateUIから参照する。
    public GameObject spellUIPrefab;

    [Tooltip("ドロップUIプレハブ")]
    public GameObject dropUIPrefab;

    [Tooltip("保持情報UIプレハブ")]
    public GameObject holdInfoUIPrefab;
}