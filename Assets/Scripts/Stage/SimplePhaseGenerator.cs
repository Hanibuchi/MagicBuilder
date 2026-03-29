using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 単一のEnemyPhaseConfigをそのままリストに追加するシンプルな設定クラス。
/// </summary>
[System.Serializable]
public class SimplePhaseGenerator : PhaseGeneratorBase
{
    public EnemyPhaseConfig phaseConfig;

    public override void GeneratePhases(List<EnemyPhaseConfig> phaseList)
    {
        phaseList.Add(phaseConfig);
    }
}
