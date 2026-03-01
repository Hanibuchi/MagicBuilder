// StageListConfig.cs
using System.Linq;
using UnityEngine;

/// <summary>
/// 全てのステージ設定 (StageConfig) のリストを保持するScriptableObject。
/// </summary>
[CreateAssetMenu(fileName = "StageList", menuName = "GameConfig/Stage List")]
public class StageListConfig : ScriptableObject
{
    [Tooltip("ゲーム内の全てのステージ設定の配列。インスペクタから設定します。")]
    public StageConfig[] stages;

    public StageConfig GetStageInfoByName(string stageName)
    {
        if (stages == null)
        {
            Debug.LogError("StageListConfig: stages 配列が設定されていません。");
            return null;
        }

        return stages.FirstOrDefault(config => config != null && config.stageName == stageName);
    }

    /// <summary>
    /// 指定されたステージ名の次のステージ設定 (StageConfig) を取得します。
    /// 最後のステージの場合は null を返します。
    /// </summary>
    /// <param name="stageName">現在のステージの識別名。</param>
    /// <returns>次のステージ設定。存在しない場合は null。</returns>
    public StageConfig GetNextStageInfoByName(string stageName)
    {
        if (stages == null || stages.Length == 0)
        {
            Debug.LogError("StageListConfig: stages 配列が設定されていないか、空です。");
            return null;
        }

        // 1. 現在のステージのインデックスを検索
        int currentIndex = -1;
        for (int i = 0; i < stages.Length; i++)
        {
            // Nullチェックを追加
            if (stages[i] != null && stages[i].stageName == stageName)
            {
                currentIndex = i;
                break;
            }
        }

        // 2. ステージが見つからなかった場合
        if (currentIndex == -1)
        {
            Debug.LogWarning($"StageListConfig: ステージ名 '{stageName}' が見つかりませんでした。");
            return null;
        }

        // 3. 次のステージのインデックスを計算
        int nextIndex = currentIndex + 1;

        // 4. 次のステージが存在するかチェック
        if (nextIndex < stages.Length)
        {
            // 次のステージ設定を返す (念のためnullチェック)
            return stages[nextIndex];
        }
        else
        {
            // 最後のステージであるため、次のステージは存在しない
            Debug.Log($"StageListConfig: ステージ名 '{stageName}' は最後のステージです。");
            return null;
        }
    }
}