using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 全ての呪文で共通利用されるデータ（アイコン、エフェクトのプリセットなど）を管理するクラス。
/// Singletonパターンでアセットとして管理すると便利。
/// </summary>
[CreateAssetMenu(fileName = "SpellCommonData", menuName = "Wand System/Spell Common Data")]
public class SpellCommonData : ScriptableObject
{
    public static SpellCommonData Instance { get; private set; } // Static Instance

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

    // ランタイムでInstanceを設定するためのヘルパーメソッド
    private void OnEnable()
    {
        // プロジェクト起動時などに一度だけ設定されることを想定
        if (Instance == null)
        {
            Instance = this;
        }
    }
}