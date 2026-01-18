using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// WandTypeとWandのデータを紐づけるScriptableObject。
/// </summary>
[CreateAssetMenu(fileName = "WandDataAsset", menuName = "Wand System/WandDataAsset")]
public class WandDataAsset : ScriptableObject
{
    [System.Serializable]
    public class WandEntry
    {
        public WandType type;
        public Wand wand;
    }

    [Tooltip("杖の種類とテンプレートのリスト。")]
    public List<WandEntry> wands = new List<WandEntry>();

    /// <summary>
    /// 指定された種類のWandデータを取得します。
    /// </summary>
    public Wand GetWand(WandType type)
    {
        var entry = wands.Find(e => e.type == type);
        return entry?.wand;
    }
}
