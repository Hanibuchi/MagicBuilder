// StageConfig.cs
using UnityEngine;

/// <summary>
/// 一つのステージに関する情報をまとめたScriptableObject。
/// </summary>
[CreateAssetMenu(fileName = "NewStageConfig", menuName = "GameConfig/Stage Config")]
public class StageConfig : ScriptableObject
{
    [Header("ステージ基本情報")]
    [Tooltip("ステージの識別名。")]
    public string stageName = "New Stage";
    public string subStageName = "New SubStage";

    public string SceneName = "Stage_";

    [Tooltip("ステージの勝利条件。")]
    public StageClearCondition clearCondition = StageClearCondition.SpecificBossDefeated;

    [Header("敵の出現フェーズ")]
    [Tooltip("このステージで出現する敵のフェーズ設定リスト。")]
    public EnemyPhaseConfig[] enemyPhases;

    [Header("ステージの種類")]
    [Tooltip("ステージのプレイスタイルを指定。")]
    public StageType stageType = StageType.EndlessWave;
}

public enum StageType
{
    EndlessWave, // 敵が次々と攻めてくるステージ (例: 無限ウェーブ)
    Puzzle // 与えられた呪文で敵を全滅させるパズル的なステージ
}