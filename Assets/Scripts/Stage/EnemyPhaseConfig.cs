// EnemyPhaseConfig.cs
using UnityEngine;

/// <summary>
/// 敵の出現フェーズと次のフェーズへの移行条件を定義する構造体。
/// </summary>
[System.Serializable]
public struct EnemyPhaseConfig
{
    [Header("フェーズの条件")]
    [Tooltip("このフェーズを開始するための条件タイプ。現在はTimeElapsedのみを想定。")]
    public PhaseConditionType conditionType;

    [Tooltip("条件を満たすために必要な値 (例: TimeElapsedの場合は秒数)")]
    public float conditionValue;

    [Header("このフェーズで実行する処理")]
    [Tooltip("このフェーズで生成する敵の設定")]
    [SerializeField]
    public EnemySpawnerConfig spawnerConfig;

    [Tooltip("このフェーズの処理後に実行する次のフェーズ")]
    public EnemyPhaseConfig[] nextPhases;

    public enum PhaseConditionType
    {
        TimeElapsed, // 時間経過
        AllEnemiesDefeated, // すべての敵を倒した
        // 今後の拡張性: PlayerEnteredArea, SpecificEventTriggered, etc.
    }
}