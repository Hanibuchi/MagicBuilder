using UnityEngine;

/// <summary>
/// ランダムに飛び回る蜂の動きを実装するクラス。
/// 指定されたTransform（中心点）の半径以内のランダムな地点へ、指定時間をかけて移動を繰り返す。
/// </summary>
public class BeeMovement : EnemyMovementBase
{
    [Header("蜂の移動パス設定")]
    [SerializeField]
    private Transform centerTransform; // 移動の中心となるTransform

    [SerializeField]
    private float moveRadius = 3f;    // 中心からの移動可能半径

    [SerializeField]
    private float moveDuration = 0.5f; // 1回の移動にかける時間（秒）

    [SerializeField]
    private Transform modelTransform; // 実際に移動させるオブジェクト（見た目とコライダーを持つもの）

    private Vector2 startPosition;      // 移動開始地点
    private Vector2 targetPosition;     // 移動目標地点
    private float elapsedTime = 0f;     // 現在の移動経過時間
    private bool isInitialized = false; // 初期化済みフラグ

    protected override void Awake()
    {
        base.Awake();

        // modelTransformが未設定の場合は、自身のTransformを使用する
        if (modelTransform == null)
        {
            modelTransform = transform;
        }

        // 最初の目標地点を設定
        InitializeMovement();
    }

    private void InitializeMovement()
    {
        if (centerTransform == null)
        {
            Debug.LogWarning($"{name}: centerTransformが設定されていないため、現在の位置を移動の中心とします。");
            // centerTransformがない場合は、開始時の親の位置などを仮の中心にするために自身の初期位置を基準にする
            startPosition = modelTransform.position;
        }
        else
        {
            startPosition = modelTransform.position;
        }

        SetNextRandomTarget();
        isInitialized = true;
    }

    /// <summary>
    /// 次のランダムな目標地点を設定する。
    /// </summary>
    private void SetNextRandomTarget()
    {
        elapsedTime = 0f;
        startPosition = modelTransform.position;

        // centerTransformの現在位置を基準に半径内のランダムな地点を計算
        Vector2 center = centerTransform != null ? (Vector2)centerTransform.position : (Vector2)transform.position;
        targetPosition = center + Random.insideUnitCircle * moveRadius;
    }

    protected override void FixedUpdate()
    {
    }

    void LateUpdate()
    {
        base.FixedUpdate();
    }

    protected override void HandleMovement()
    {
        base.HandleMovement();
        if (!isInitialized) return;

        // 合計速度倍率（状態異常比率 * 呪文倍率）を移動経過時間に反映させる
        float currentMultiplier = GetTotalSpeedMultiplier();

        // 経過時間を加算（倍率を掛けることで、減速時に移動がゆっくりになるようにする）
        elapsedTime += Time.fixedDeltaTime * currentMultiplier;

        // 補間割合 (0.0 ～ 1.0)
        float t = Mathf.Clamp01(elapsedTime / moveDuration);

        // 線形補間で移動（ベジェ曲線などを使わないシンプルなジグザグ移動）
        Vector2 nextPos = Vector2.Lerp(startPosition, targetPosition, t);

        // if (rb != null)
        // {
        //     // Rigidbody2Dがある場合は物理演算に干渉しないようMovePositionを使用
        //     rb.MovePosition(nextPos);
        // }
        // else
        {
            // Rigidbodyがない場合はTransformを直接動かす
            modelTransform.position = nextPos;
        }

        // 到着したら次の地点へ
        if (t >= 1f)
        {
            SetNextRandomTarget();
        }
    }

    /// <summary>
    /// インスペクタでのデバッグ表示
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (centerTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(centerTransform.position, moveRadius);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawLine(modelTransform.position, targetPosition);
        Gizmos.DrawWireSphere(targetPosition, 0.2f);
    }
}
