using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ゲーム開始時の初期持ち込み呪文を設定するための ScriptableObject。
/// Resources/InitialEquippedSpellsConfig として配置することを想定しています。
/// </summary>
[CreateAssetMenu(fileName = "InitialEquippedSpellsConfig", menuName = "MagicBuilder/InitialEquippedSpellsConfig")]
public class InitialEquippedSpellsConfig : ScriptableObject
{
    [Tooltip("初期状態でスロットにセットされている呪文のリスト")]
    public List<SpellBase> initialSpells = new List<SpellBase>();
}
