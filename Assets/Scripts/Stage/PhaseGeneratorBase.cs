using System.Collections.Generic;

/// <summary>
/// EnemyPhaseConfigのリストを受け取り、要素を動的に追加するための基底クラス。
/// SerializeReferenceを使用して、派生クラスをInspectorで設定可能にします。
/// </summary>
[System.Serializable]
public abstract class PhaseGeneratorBase
{
    // 既存のリスト追加方式（後方互換性のため残す）
    public abstract void GeneratePhases(List<EnemyPhaseConfig> phaseList);

    /// <summary>
    /// 遅延評価でフェーズを生成するためのメソッド。
    /// 無限に続くフェーズ（エンドレスモード）などを実装する場合はこちらをオーバーライドして yield return します。
    /// </summary>
    public virtual IEnumerable<EnemyPhaseConfig> GeneratePhasesEnumerable()
    {
        var list = new List<EnemyPhaseConfig>();
        GeneratePhases(list);
        foreach (var phase in list)
        {
            yield return phase;
        }
    }
}
