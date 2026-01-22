// EnemyPhaseConfig.cs
using UnityEngine;

/// <summary>
/// 敵の出現フェーズと次のフェーズへの移行条件を定義するセリアライズ可能なクラス。
/// </summary>
[System.Serializable]
public class EnemyPhaseConfig
{
    [Header("フェーズの条件")]
    [Tooltip("このフェーズを開始するための条件タイプ。現在はTimeElapsedのみを想定。")]
    public PhaseConditionType conditionType = PhaseConditionType.TimeElapsed;

    [Tooltip("条件を満たすために必要な値 (例: TimeElapsedの場合は秒数)")]
    public float conditionValue = 1.0f; // 例: 5秒経過

    [Header("このフェーズで実行する処理")]
    [Tooltip("このフェーズで生成する敵の設定")]
    [SerializeReference]
    public EnemySpawnerConfig spawnerConfig = new EnemySpawnerConfig();

    [Tooltip("このフェーズの処理後に実行する次のフェーズ")]
    public EnemyPhaseConfig[] nextPhases;

    public enum PhaseConditionType
    {
        None, // 条件なし（即時実行）
        TimeElapsed, // 時間経過
        AllEnemiesDefeated, // すべての敵を倒した
        // 今後の拡張性: PlayerEnteredArea, SpecificEventTriggered, etc.
    }
}