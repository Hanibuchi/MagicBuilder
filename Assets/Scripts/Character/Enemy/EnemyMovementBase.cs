using UnityEngine;

/// <summary>
/// 敵の動きの基底クラス。すべての敵の動きはこれを継承する。
/// </summary>
public class EnemyMovementBase : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField]
    protected float defaultMoveSpeed = 1.0f; // 基準となる移動速度

    [Tooltip("目標速度への追従強度。高いほど物理的な干渉に強くなります。")]
    [SerializeField]
    protected float movementForceGain = 10.0f;

    protected Rigidbody2D rb;
    protected bool isMoving = false; // 現在移動中かどうかのフラグ

    public bool IsMoving => isMoving;

    // --- 速度制御用フィールド ---
    protected float speedRatio = 1.0f;        // 状態異常（スロウ等）による速度比率
    protected float spellSpeedMultiplier = 1.0f; // 呪文による追加の速度効果

    /// <summary>
    /// 呪文による追加の速度倍率。
    /// </summary>
    public float SpellSpeedMultiplier
    {
        get => spellSpeedMultiplier;
        set => spellSpeedMultiplier = value;
    }

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
    /// 現在の適用されるべき移動速度を計算して返します。
    /// </summary>
    public float GetCurrentMoveSpeed()
    {
        return defaultMoveSpeed * GetTotalSpeedMultiplier();
    }

    /// <summary>
    /// 状態異常比率と呪文倍率を掛け合わせた合計倍率を返します。
    /// </summary>
    public float GetTotalSpeedMultiplier()
    {
        return speedRatio * spellSpeedMultiplier;
    }

    /// <summary>
    /// 速度比率を設定します。1.0fを通常状態とします。
    /// </summary>
    public virtual void SetMovementSpeedRatio(float ratio)
    {
        speedRatio = ratio;
    }

    /// <summary>
    /// 呪文による速度倍率を設定します。
    /// </summary>
    public virtual void SetSpellSpeedMultiplier(float multiplier)
    {
        spellSpeedMultiplier = multiplier;
    }

    /// <summary>
    /// 移動速度を通常の速度に戻します。
    /// </summary>
    protected virtual void ResetMovementSpeed()
    {
        speedRatio = 1.0f;
    }

    /// <summary>
    /// 毎フレームの動きの処理を実装するメソッド。
    /// 継承先でオーバーライドして、独自の動きを実装する。
    /// </summary>
    protected virtual void HandleMovement()
    {
        // 直接速度を上書きせず、目標速度に近づくように力を加えることで
        // 外部からの AddForce（ノックバックや引き寄せなど）と共存させます。
        if (rb != null)
        {
            // キャラクターが向いている方向（localScale.x）の符号のみを取得
            // Unityのtransform.rightはスケール反転を無視するため、localScaleを直接参照します
            float targetVelocityX = Mathf.Sign(transform.localScale.x) * -GetCurrentMoveSpeed();
            float currentVelocityX = rb.linearVelocity.x;

            // 目標速度との差分を埋めるための力を計算
            float forceX = (targetVelocityX - currentVelocityX) * rb.mass * movementForceGain;
            rb.AddForce(new Vector2(forceX, 0));
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

        // 速度比率をデフォルトに戻す
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