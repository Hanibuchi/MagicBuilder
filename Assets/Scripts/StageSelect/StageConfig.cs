// StageConfig.cs
using UnityEngine;
using System.Collections.Generic;

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
    [Tooltip("このステージで出現する敵のフェーズ設定リスト。先頭につけられる。")]
    public EnemyPhaseConfig[] enemyPhases;

    [Header("動的フェーズ生成設定")]
    [Tooltip("敵の出現フェーズを動的に生成するジェネレータのリスト。enemyPhasesの後に追加される。")]
    [SerializeReference] public List<PhaseGeneratorBase> phaseGenerators = new List<PhaseGeneratorBase>();

    [ContextMenu("Add Simple Phase Generator")]
    void AddSimplePhaseGenerator() => phaseGenerators.Add(new SimplePhaseGenerator());

    [ContextMenu("Add Random Spawns Phase Generator")]
    void AddRandomSpawnsPhaseGenerator() => phaseGenerators.Add(new RandomSpawnsPhaseGenerator());

    [Header("パスルステージ設定")]
    [Tooltip("パズルステージで使用する固定呪文のリスト。")]
    public SpellBase[] puzzleSpells;
    [Tooltip("パズルステージで使用できる杖の種類。")]
    public WandType[] puzzleWands;

    [Header("ステージの種類")]
    [Tooltip("ステージのプレイスタイルを指定。")]
    public StageType stageType = StageType.Rush;
}

public enum StageType
{
    Rush, // 敵が次々と攻めてくるステージ (例: 無限ウェーブ)
    Puzzle // 与えられた呪文で敵を全滅させるパズル的なステージ
}