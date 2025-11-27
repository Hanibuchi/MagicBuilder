using UnityEngine;

/// <summary>
/// 敵の動きの基底クラス。すべての敵の動きはこれを継承する。
/// </summary>
public class EnemyMovementBase : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField]
    protected float moveSpeed = 1.0f; // 単位時間あたりの移動速度
    
    // 通常の移動速度を保持するフィールドを追加
    protected float defaultMoveSpeed; 

    protected Rigidbody2D rb;
    protected bool isMoving = true; // 現在移動中かどうかのフラグ

    // --- Unity ライフサイクルメソッド ---

    protected virtual void Awake()
    {
        // Rigidbody2Dを取得。継承先でも利用できるようprotectedにする。
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2Dが見つかりません。敵モブにはRigidbody2Dが必要です。", this);
        }
        
        // 速度比率制御のために、Awake時に現在のmoveSpeedをデフォルト値として保持する
        defaultMoveSpeed = moveSpeed;
    }

    protected virtual void FixedUpdate()
    {
        // 物理演算はFixedUpdateで行う
        if (isMoving)
        {
            // 毎フレーム呼ばれる動きの処理を実行
            HandleMovement();
        }
    }

    // --- 動きの制御メソッド ---
    
    /// <summary>
    /// 0.0fから1.0fのfloat型を取り、1.0fを通常状態として移動速度をその比率にする。
    /// 例: ratioが0.5fなら、速度は通常の半分になる。
    /// </summary>
    /// <param name="ratio">移動速度の比率 (0.0f - 1.0f)。</param>
    public virtual void SetMovementSpeedRatio(float ratio)
    {
        // 比率を0.0fから1.0fの間にクランプ（制限）する
        float clampedRatio = Mathf.Clamp01(ratio);
        
        // moveSpeedをデフォルトの速度に比率を掛けた値に設定
        moveSpeed = defaultMoveSpeed * clampedRatio;
    }

    /// <summary>
    /// 移動速度を通常の速度に戻す (SetMovementSpeedRatio(1.0f) と同じ)。
    /// </summary>
    public virtual void ResetMovementSpeed()
    {
        moveSpeed = defaultMoveSpeed;
    }

    /// <summary>
    /// 毎フレームの動きの処理を実装するメソッド。
    /// 継承先でオーバーライドして、独自の動きを実装する。
    /// </summary>
    protected virtual void HandleMovement()
    {
        // デフォルトの動き: 左に移動
        // Velocityを直接操作
        if (rb != null)
        {
            // 更新された moveSpeed を使用
            rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);
        }
    }

    /// <summary>
    /// 敵の動きを**一時的に**停止させる。
    /// </summary>
    public virtual void StopMovement()
    {
        if (isMoving)
        {
            isMoving = false;
            if (rb != null)
            {
                // 停止時に現在の速度をゼロにする
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    /// <summary>
    /// 敵の動きを再開させる。
    /// </summary>
    public virtual void ResumeMovement()
    {
        if (!isMoving)
        {
            isMoving = true;
            // 再開時の速度設定はHandleMovement()に任せる（次のFixedUpdateで実行される）
        }
    }
}