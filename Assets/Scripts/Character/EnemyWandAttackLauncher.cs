using UnityEngine;

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

    [Tooltip("この杖の呪文が想定している強さの倍率 (ExampleSpellなどのstrengthMultiplierに合わせる)")]
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
        float g = Physics2D.gravity.magnitude;
        Vector2 diff = target - start;
        float x = Mathf.Abs(diff.x);
        float y = diff.y;

        // 重力がほぼ無い場合は直接狙う
        if (g < 0.01f)
        {
            angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            strength = baseFireStrength;
            return;
        }

        float vMax = expectedStrengthMultiplier; // 最大強度(1.0)時の速度

        // 最小到達速度 v_min^2 = g * (y + sqrt(x^2 + y^2))
        float vMinSq = g * (y + Mathf.Sqrt(x * x + y * y));
        float vMin = Mathf.Sqrt(Mathf.Max(0, vMinSq));

        float v;
        if (vMin > vMax)
        {
            // 最大パワーでも届かない場合は最大で撃つ
            v = vMax;
            strength = 1.0f;
        }
        else
        {
            // baseFireStrengthで届くかチェック。届かないなら必要最小限まで上げる。
            float targetV = baseFireStrength * expectedStrengthMultiplier;
            if (targetV < vMin)
            {
                v = vMin;
                strength = v / expectedStrengthMultiplier;
            }
            else
            {
                v = targetV;
                strength = baseFireStrength;
            }
        }

        // 角度の計算: tan(theta) = (v^2 +/- sqrt(v^4 - g(gx^2 + 2yv^2))) / (gx)
        float v2 = v * v;
        float v4 = v2 * v2;
        float root = v4 - g * (g * x * x + 2 * y * v2);

        if (root < 0) root = 0; // 精度誤差対策
        root = Mathf.Sqrt(root);

        float tanTheta = (v2 + (useLowArc ? -root : root)) / (g * x);

        if (x < 0.001f)
        {
            angle = diff.y > 0 ? 90f : -90f;
        }
        else
        {
            angle = Mathf.Atan(tanTheta) * Mathf.Rad2Deg;
            // X方向（左右）の調整
            if (diff.x < 0)
            {
                angle = 180f - angle;
            }
        }
    }
}
