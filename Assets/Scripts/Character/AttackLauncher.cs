using UnityEngine;

/// <summary>
/// 攻撃の発射地点、プレハブを設定し、指定されたターゲットに向かって攻撃を発射するクラス
/// </summary>
public class AttackLauncher : MonoBehaviour
{
    // --- インスペクタから設定するフィールド ---

    [Header("設定")]
    [Tooltip("攻撃を発射するワールド座標のTransform")]
    [SerializeField] protected Transform launchPoint;

    [Tooltip("発射する攻撃のプレハブ (GameObject)")]
    [SerializeField] protected GameObject attackPrefab;

    [Header("攻撃詳細設定")]
    [Tooltip("攻撃のダメージ情報")]
    [SerializeField] protected Damage attackDamage;

    [Tooltip("攻撃オブジェクトが消滅するまでの時間 (秒)")]
    [SerializeField] protected float destroyTime = 1.0f;

    // --- 公開メソッド ---

    /// <summary>
    /// 指定されたワールド座標に向かって攻撃を発射する。
    /// </summary>
    /// <param name="targetPosition">狙う地点のワールド座標 (Vector2)</param>
    public virtual void LaunchAttack(Vector2 targetPosition)
    {

        if (launchPoint == null || attackPrefab == null)
        {
            Debug.LogError("LaunchPointまたはAttackPrefabが設定されていません！");
            return;
        }

        // 1. 発射地点の取得
        Vector2 launchPosition = launchPoint.position;

        // 2. 狙う地点への方向を計算 (target - current)
        Vector2 direction = (targetPosition - launchPosition).normalized;

        // 3. 角度の計算 (ラジアンから度に変換)
        // atan2を使用して、yとxから正確な角度を計算
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 4. プレハブのインスタンス化
        // 角度をZ軸の回転としてQuaternionに変換
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
        GameObject attackInstance = Instantiate(attackPrefab, launchPosition, rotation);

        // --- 追加: ProjectileDamageSourceへの値のセット ---
        if (attackInstance.TryGetComponent<ProjectileDamageSource>(out var damageSource))
        {
            damageSource.SetDamage(attackDamage);
            damageSource.SetDestroyTime(destroyTime);
        }

        // 5. 発射した攻撃をこのオブジェクトの子にする (オプション: シーンの整理のため)
        attackInstance.transform.SetParent(transform);

        // rootのスケールを適用して、生成されたオブジェクトを親（root）の大きさに拡大する
        attackInstance.transform.localScale = Vector3.Scale(attackInstance.transform.localScale, transform.root.localScale);

        // 6. X軸負の方向を向く場合の調整
        // Unityの2Dスプライトは通常、X軸正の方向を「前」とする。
        // 発射角度が180度に近い（左向き）場合、Yスケールを反転させて左右反転させる。
        // 例: angleが90度～-90度（右向き）なら通常、90度～270度（左向き）なら反転。
        // X方向の速度 (または方向ベクトルx成分) が負の場合にYスケールを-1にする。


        // direction.xが負（左向き）の場合にYスケールを-1倍にする
        // (これにより、スプライトが左を向くように反転する)
        if (direction.x < 0)
        {
            // 攻撃オブジェクトのローカルスケールを取得
            Vector3 localScale = attackInstance.transform.localScale;
            // Yスケールを反転
            localScale.y *= -1f;
            // ローカルスケールを適用
            attackInstance.transform.localScale = localScale;
        }
    }

    // public Transform test_targetPosition;
    // public void Test()
    // {
    //     LaunchAttack(test_targetPosition.position);
    // }
}