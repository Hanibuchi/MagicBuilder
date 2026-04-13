using System.Collections.Generic;

/// <summary>
/// EnemyPhaseConfigのリストを受け取り、要素を動的に追加するための基底クラス。
/// SerializeReferenceを使用して、派生クラスをInspectorで設定可能にします。
/// </summary>
[System.Serializable]
public abstract class PhaseGeneratorBase
{
    public abstract void GeneratePhases(List<EnemyPhaseConfig> phaseList);
}
