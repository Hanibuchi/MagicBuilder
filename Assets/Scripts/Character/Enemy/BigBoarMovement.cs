using UnityEngine;
using System.Collections;

/// <summary>
/// BigBoarの動き。
/// 勢いよく突進し、相手(センサー範囲内)にぶつかったら大きくノックバックして一時的に止まる。
/// その後再び突進する動作を繰り返す。
/// </summary>
public class BigBoarMovement : EnemyMovementBase
{
    [Header("BigBoar 突進設定")]
    [Tooltip("突進中の移動速度の倍率。")]
    [SerializeField] private float dashSpeedMultiplier = 3.0f;
    
    [Tooltip("ぶつかった際にノックバックする力")]
    [SerializeField] private float bounceForce = 15.0f;//
    
    [Tooltip("ぶつかった後のスタン（硬直）時間")]
    [SerializeField] private float stunDurationAfterHit = 2.0f;
    
    [Tooltip("ノックバック後に突進を再開するまでの準備（チャージ）時間")]
    [SerializeField] private float chargeDurationBeforeDash = 1.0f;

    [Tooltip("ノックバックが発動するLayerSensorのID（EnemyControllerに設定したもの）")]
    [SerializeField] private string targetTriggerID = "attack1";

    private bool isRecovering = false; // ノックバックから次の突進までの待機状態か

    protected override void HandleMovement()
    {
        // 回復中（スタン・チャージ中）は移動処理を行わない
        if (isRecovering)
        {
            return;
        }

        // 突進処理
        // 突進中は移動速度を倍率分上げる
        float originalSpeedRatio = speedRatio;
        speedRatio *= dashSpeedMultiplier;
        
        // 基底クラスの移動処理を呼んで力を加える
        base.HandleMovement();
        
        // 速度比率を元に戻す
        speedRatio = originalSpeedRatio;
    }

    public override void OnTargetSensed(string triggerID, Vector2 targetPos)
    {
        base.OnTargetSensed(triggerID, targetPos);

        // 指定のセンサーに当たった＆回復中でない場合にノックバックを発動
        if (triggerID == targetTriggerID && !isRecovering && isStunned <= 0)
        {
            StartCoroutine(KnockbackAndRecoverRoutine(targetPos));
        }
    }

    private IEnumerator KnockbackAndRecoverRoutine(Vector2 targetPos)
    {
        isRecovering = true;
        // 突進の一時停止
        StopMovement();

        // 相手の方向から逆向きへノックバック
        if (rb != null)
        {
            // 速度をリセット
            rb.linearVelocity = Vector2.zero;

            // ぶつかった相手に対して逆方向へ弾かれる力を計算
            float diffX = transform.position.x - targetPos.x;
            // 相手が右側(diffX < 0)なら左上(-1, 1)、相手が左側(diffX >= 0)なら右上(1, 1)へ弾く
            Vector2 direction = new Vector2(diffX < 0 ? -1f : 1f, 1f).normalized;
            
            rb.AddForce(direction * bounceForce, ForceMode2D.Impulse);
        }

        // 1. スタン（硬直）時間待機
        yield return new WaitForSeconds(stunDurationAfterHit);

        // 2. 突進再開の準備（チャージ）時間待機
        // ここでチャージモーション（アニメーションなど）を行うことを想定
        yield return new WaitForSeconds(chargeDurationBeforeDash);

        // 突進再開
        isRecovering = false;
        ResumeMovement();
    }

    public override void ApplyIceSlow()
    {
        base.ApplyIceSlow();
        // 氷状態などで追加の処理が必要であればここに記述
    }
}
