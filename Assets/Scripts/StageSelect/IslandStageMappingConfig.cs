// IslandStageMappingConfig.cs

using System.Linq;
using UnityEngine;

/// <summary>
/// 島と、その島に属するステージのリストを紐づけるScriptableObject。
/// </summary>
[CreateAssetMenu(fileName = "IslandStageMap", menuName = "GameConfig/Island Stage Mapping")]
public class IslandStageMappingConfig : ScriptableObject
{
    [System.Serializable]
    public class IslandStageData
    {
        [Tooltip("IslandSelectorに設定された島の識別子。")]
        public string islandID;
        public string islandName;

        [Tooltip("この島をクリアしたときに解放される杖の種類。")]
        public WandType wandToUnlock;

        [Tooltip("この島に属するステージのリスト。")]
        public string[] stages;
    }

    [Tooltip("全ての島のステージ情報の配列。")]
    public IslandStageData[] islandStageMap;

    public string GetIslandNameByID(string id)
    {
        foreach (var data in islandStageMap)
        {
            if (data.islandID == id)
            {
                return data.islandName;
            }
        }
        return null; // 見つからない場合
    }

    /// <summary>
    /// 指定された島IDに一致するステージリストを取得します。
    /// </summary>
    /// <param name="id">島の識別子。</param>
    /// <returns>その島に属するステージ情報の配列。見つからない場合はnull。</returns>
    public string[] GetStagesForIsland(string id)
    {
        foreach (var data in islandStageMap)
        {
            if (data.islandID == id)
            {
                return data.stages;
            }
        }
        return null; // 見つからない場合
    }


    /// <summary>
    /// ステージ名から、それが属する島のIDを IslandStageMappingConfig を使って検索します。
    /// </summary>
    /// <param name="stageName">検索したいステージ名。</param>
    /// <returns>対応する島のID。見つからない場合はnull。</returns>
    public string GetIslandIDForStage(string stageName)
    {
        if (islandStageMap == null) return null;

        foreach (var islandData in islandStageMap)
        {
            if (islandData != null && islandData.stages != null && islandData.stages.Contains(stageName))
            {
                return islandData.islandID;
            }
        }
        return null;
    }

    /// <summary>
    /// 指定されたステージがその島の最後のステージかどうかを判定し、
    /// 最後のステージであればその島に関連付けられた解放対象の杖の種類を返します。
    /// </summary>
    /// <param name="stageName">判定対象のステージ名。</param>
    /// <param name="wandType">解放対象の杖の種類（最後のステージでない場合はNoneなどは返せないので、bool戻り値とoutパラメータで対応）。</param>
    /// <returns>最後のステージであればtrue。</returns>
    public bool IsLastStageOfIsland(string stageName, out WandType wandType)
    {
        wandType = WandType.Default;
        if (islandStageMap == null) return false;

        foreach (var islandData in islandStageMap)
        {
            if (islandData != null && islandData.stages != null && islandData.stages.Length > 0)
            {
                // 最後のステージ名を取得
                string lastStageInIsland = islandData.stages[^1];
                if (lastStageInIsland == stageName)
                {
                    wandType = islandData.wandToUnlock;
                    return true;
                }
            }
        }
        return false;
    }
}