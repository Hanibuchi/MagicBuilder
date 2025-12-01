// StageListConfig.cs
using UnityEngine;

/// <summary>
/// 全てのステージ設定 (StageConfig) のリストを保持するScriptableObject。
/// </summary>
[CreateAssetMenu(fileName = "StageList", menuName = "GameConfig/Stage List")]
public class StageListConfig : ScriptableObject
{
    [Tooltip("ゲーム内の全てのステージ設定の配列。インスペクタから設定します。")]
    public StageConfig[] stages;
}