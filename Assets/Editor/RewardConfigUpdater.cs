using UnityEngine;
using UnityEditor;

public class RewardConfigUpdater : EditorWindow
{
    [MenuItem("Tools/Update Stage Rewards")]
    public static void UpdateRewards()
    {
        string[] worldDirs = { "1_forest", "2_desert", "3_volcanic", "4_ice", "5_sky", "6_magic" };

        for (int w = 1; w <= 6; w++)
        {
            string worldFolder = worldDirs[w - 1];

            for (int s = 1; s <= 6; s++)
            {
                bool isPuzzle = (s % 2 != 0);

                // ワールド内のインデックス (0, 1, 2)
                int localIndex = isPuzzle ? (s - 1) / 2 : (s - 2) / 2;
                // 全体でのインデックス (0 ～ 17)
                int globalIndex = (w - 1) * 3 + localIndex;

                int firstClearReward = 0;
                if (isPuzzle)
                {
                    // パズル: 1-1(200) ～ 6-5(400)
                    firstClearReward = Mathf.RoundToInt(Mathf.Lerp(200, 370, globalIndex / 17f));
                }
                else
                {
                    // ラッシュ: 1-2(200) ～ 6-6(800)
                    firstClearReward = Mathf.RoundToInt(Mathf.Lerp(200, 710, globalIndex / 17f));
                }

                // キリよく10の倍数に丸める等の指定はないため、四捨五入した整数値をそのまま使用
                int repeatClearReward = firstClearReward / 2;

                string stageName = $"{w}-{s}";
                string targetPath = $"Assets/Stage/{worldFolder}/{stageName}_StageConfig.asset";

                var stageConfig = AssetDatabase.LoadAssetAtPath<StageConfig>(targetPath);
                if (stageConfig != null)
                {
                    stageConfig.firstClearReward = firstClearReward;
                    stageConfig.repeatClearReward = repeatClearReward;
                    EditorUtility.SetDirty(stageConfig);
                    Debug.Log($"Updated: {stageName} (First: {firstClearReward}, Repeat: {repeatClearReward})");
                }
                else
                {
                    Debug.LogWarning($"StageConfig not found: {targetPath}");
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("All Stage Rewards Successfully Updated!");
    }
}
