using UnityEngine;

/// <summary>
/// 持ち込み呪文の容量拡張に関する設定を保持する ScriptableObject。
/// </summary>
[CreateAssetMenu(fileName = "EquippedSpellCapacityConfig", menuName = "MagicBuilder/EquippedSpellCapacityConfig")]
public class EquippedSpellCapacityConfig : ScriptableObject
{
    [Tooltip("現在の最大容量ごとの拡張コスト。インデックスが現在の最大容量に対応します。")]
    public int[] capacityUpgradeCosts = new int[] { 500, 1000, 2000, 4000 };
}
