using UnityEngine;

/// <summary>
/// 敵の動きの基底クラス。すべての敵の動きはこれを継承する。
/// </summary>
public class EnemyMovementBase : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField]
    protected float moveSpeed = 1.0f; // 単位時間あたりの移動速度

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
    /// 毎フレームの動きの処理を実装するメソッド。
    /// 継承先でオーバーライドして、独自の動きを実装する。
    /// </summary>
    public virtual void HandleMovement()
    {
        // デフォルトの動き: 左に移動
        // Velocityを直接操作
        if (rb != null)
        {
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