using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// 全ての具体的な呪文クラスの抽象基底クラス。
/// ScriptableObjectを継承することで、Unity Inspectorで設定可能なデータアセットとして扱えます。
/// </summary>
public abstract class SpellBase : ScriptableObject
{
    [Header("基本設定")]
    [Tooltip("呪文が発動した際に追加されるクールタイム（秒）")]
    public float cooldown = 0.5f;

    [Tooltip("この呪文のゲーム内での表示名")]
    public string spellName = "未定義の呪文";

    [Header("補助線設定")]
    [Tooltip("軌道プレハブの生成間隔（秒）。小さいほど密になります。")]
    public float trajectoryPrefabInterval = 0.1f;
    [Tooltip("⚡ 軌道予測を行う最大の時間（秒）。この時間を超える軌道は計算しません。")]
    public float maxPredictionTime = 5.0f;

    [Tooltip("軌道予測に使用するプレハブ")]
    public GameObject trajectoryPrefab;

    // ----------------------------------------------------------------------------------
    // 抽象メソッド定義
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// 補助線（軌道予測）を表示するためのロジックを定義します。
    /// 処理内容: 発射角度と強さ、重力に基づいて軌道を計算し、プレハブを生成します。
    /// </summary>
    /// <param name="wandSpells">杖にセットされている全呪文の配列</param>
    /// <param name="currentSpellIndex">この呪文が杖の配列の何番目にあるか</param>
    /// <param name="rotationZ">発射角度（Z軸回転）</param>
    /// <param name="strength">発射の強さ。0~1の範囲</param>
    /// <param name="casterPosition">発射元となる位置</param>
    /// <param name="gravityMagnitude">重力の大きさ</param>
    public abstract void DisplayAimingLine(
        List<SpellBase> wandSpells,
        int currentSpellIndex,
        float rotationZ,
        float strength,
        Vector2 casterPosition,
        bool clearLine = false
    );

    /// <summary>
    /// 呪文の主要な効果を発射・実行するためのロジックを定義します。
    /// 処理内容: 例として、特定のプレハブを発射します。
    /// </summary>
    /// <param name="wandSpells">杖にセットされている全呪文の配列</param>
    /// <param name="currentSpellIndex">この呪文が杖の配列の何番目にあるか</param>
    /// <param name="rotationZ">発射角度（Z軸回転）</param>
    /// <param name="strength">発射の強さ</param>
    /// <param name="context">発射時の環境情報を持つインスタンス</param>
    public abstract void FireSpell(
        List<SpellBase> wandSpells,
        int currentSpellIndex,
        float rotationZ,
        float strength,
        SpellContext context
    );

    // ----------------------------------------------------------------------------------
    // 軌道計算のヘルパー（具体的な実装例として）
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// 指定されたパラメータに基づき、特定の時間における発射体の予測位置を計算します。
    /// </summary>
    /// <param name="initialPosition">初期位置</param>
    /// <param name="initialVelocity">初速ベクトル</param>
    /// <param name="gravity">重力の大きさ</param>
    /// <param name="time">発射後の時間</param>
    /// <returns>予測位置</returns>
    protected Vector2 CalculateTrajectoryPoint(
        Vector2 initialPosition,
        Vector2 initialVelocity,
        float gravity,
        float time)
    {
        // 運動の公式: P(t) = P0 + V0*t + 1/2 * G * t^2
        Vector2 position = initialPosition
                           + initialVelocity * time
                           + (Vector2.down * gravity * time * time * 0.5f);

        return position;
    }
}

/// <summary>
/// 呪文の発射・実行時に、環境や発射元の情報などを伝達するためのクラス。
/// </summary>
public class SpellContext
{
    public Vector2 CasterPosition;
}