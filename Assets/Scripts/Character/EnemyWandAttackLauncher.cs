using UnityEngine;
using System.Linq;

/// <summary>
/// AttackLauncherを継承し、杖(Wand)を使用して攻撃を発射するクラス。
/// 敵の魔術師などにアタッチして使用することを想定しています。
/// </summary>
public class EnemyWandAttackLauncher : AttackLauncher
{
    [Header("敵の杖設定")]
    [Tooltip("発射に使用する杖")]
    [SerializeField] private Wand enemyWand;

    [Tooltip("発射の強さの基本値 (0.0f ～ 1.0f)。計算結果がこれを超える場合は調整されます。")]
    [SerializeField] private float baseFireStrength = 1.0f;

    [Tooltip("この杖の呪文が想定している強さの倍率 (ExampleSpellなどのstrengthMultiplierに合わせる。自動調整機能付き)")]
    [SerializeField] private float expectedStrengthMultiplier = 20f;

    [Tooltip("低い弾道を選択するか、高い弾道を選択するか")]
    [SerializeField] private bool useLowArc = true;

    /// <summary>
    /// 指定されたターゲット位置に向かって、杖から魔法を発射します。
    /// 角度と強度を計算して命中するように調整します。
    /// </summary>
    /// <param name="targetPosition">狙う地点のワールド座標</param>
    public override void LaunchAttack(Vector2 targetPosition)
    {
        if (enemyWand == null || enemyWand.AllSpells.Count == 0)
        {
            Debug.LogWarning($"{gameObject.name}: EnemyWandが設定されていないか、呪文が空です。ベースの攻撃を実行します。");
            base.LaunchAttack(targetPosition);
            return;
        }

        if (AttackManager.Instance == null)
        {
            Debug.LogError("AttackManagerのインスタンスが見つかりません！");
            return;
        }

        // 杖に入っている最初の ExampleSpell を探し、その倍率を自動取得する
        var exampleSpell = enemyWand.AllSpells.OfType<ExampleSpell>().FirstOrDefault();
        if (exampleSpell != null)
        {
            expectedStrengthMultiplier = exampleSpell.StrengthMultiplier;
        }

        // 1. 発射地点の取得
        Vector2 launchPosition = launchPoint != null ? (Vector2)launchPoint.position : (Vector2)transform.position;

        // 2. 角度と強さを計算
        CalculateShootParameters(launchPosition, targetPosition, out float angle, out float strength);

        // 3. AttackManagerを通じて杖を発射
        // 敵の魔術師による攻撃なので、レイヤーは Attack_Enemy (味方=プレイヤーをターゲット) を指定します。
        AttackManager.Instance.FireWand(
            enemyWand,
            launchPosition,
            angle,
            strength,
            SpellLayer.Attack_Enemy
        );

        Debug.Log($"{gameObject.name} が '{enemyWand.wandName}' を発射。ターゲット={targetPosition}, 角度={angle:F1}, 強度={strength:F2}");
    }

    /// <summary>
    /// ターゲットに命中させるための角度と強度を計算します。
    /// </summary>
    private void CalculateShootParameters(Vector2 start, Vector2 target, out float angle, out float strength)
    {
        Vector2 diff = target - start;
        float x = Mathf.Abs(diff.x);
        float y = diff.y;
        float g = Physics2D.gravity.magnitude;

        // 1. 強度(strength)の計算: ターゲットに届く最小速度を考慮
        // v_min^2 = g * (y + sqrt(x^2 + y^2))
        float minVSq = g * (y + Mathf.Sqrt(x * x + y * y));
        float minV = Mathf.Sqrt(Mathf.Max(0, minVSq));
        float targetV = baseFireStrength * expectedStrengthMultiplier;

        strength = Mathf.Clamp01(Mathf.Max(targetV, minV) / expectedStrengthMultiplier);
        float v = strength * expectedStrengthMultiplier;

        // 2. 角度(angle)の計算: 物理公式 tan(θ) = (v^2 ± sqrt(v^4 - g(gx^2 + 2yv^2))) / gx
        if (g < 0.01f || x < 0.01f) // 重力がほぼない、またはほぼ垂直の場合
        {
            angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            return;
        }

        float v2 = v * v;
        float det = v2 * v2 - g * (g * x * x + 2 * y * v2);
        float root = Mathf.Sqrt(Mathf.Max(0, det)); // 届かない(det < 0)場合は最大飛距離の角度に収束
        float tanTheta = (v2 + (useLowArc ? -root : root)) / (g * x);

        angle = Mathf.Atan(tanTheta) * Mathf.Rad2Deg;
        if (diff.x < 0) angle = 180f - angle; // ターゲットが左側の場合は角度を反転
    }
}
