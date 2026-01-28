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
    protected bool isMoving = false; // 現在移動中かどうかのフラグ

    public bool IsMoving => isMoving;

    // --- 状態異常管理用フィールド ---
    protected int isStunned = 0; // 気絶（FireStun, FreezeStun）中かどうか
    public bool IsStunned => isStunned > 0;
    protected bool isSlowed = false;  // 減速（IceSlow）中かどうか

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

    private void Start()
    {
        isMoving = true;
    }

    protected virtual void FixedUpdate()
    {
        // 物理演算はFixedUpdateで行う
        if (isMoving && isStunned <= 0)
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
    protected virtual void ResetMovementSpeed()
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
        isMoving = false;
    }

    /// <summary>
    /// 敵の動きを再開させる。状態異常（気絶・減速）からも復帰する。
    /// </summary>
    public virtual void ResumeMovement()
    {
        isMoving = true;
    }

    public void ApplyStun()
    {
        isStunned++;
    }
    public void RemoveStun()
    {
        isStunned--;
    }

    public void RemoveSlow()
    {
        // 状態異常フラグをリセット
        isSlowed = false;

        // 速度をデフォルトに戻す（減速状態からの復帰）
        ResetMovementSpeed();
    }

    /// <summary>
    /// 氷による減速状態を適用する。速さが半減する。
    /// </summary>
    public virtual void ApplyIceSlow()
    {
        // 既に減速状態なら何もしない
        if (isSlowed) return;

        isSlowed = true;
        // 速度を半減させる
        SetMovementSpeedRatio(0.5f);
    }
}