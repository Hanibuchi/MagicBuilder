using UnityEngine;

/// <summary>
/// コウモリなどの動きを実装するクラス。
/// 基本的な前進移動に加え、個別のモデルオブジェクトを上下にサイン波で揺らします。
/// BeeMovementを参考に、FixedUpdateではなくLateUpdateを使用して自身の更新処理を行います。
/// </summary>
public class BatMovement : EnemyMovementBase
{
    [Header("コウモリの移動オプション")]
    [SerializeField, Tooltip("上下にサイン波で移動するかどうか")]
    private bool enableVerticalMovement = false;

    [SerializeField, Tooltip("上下移動の範囲（振幅）")]
    private float verticalAmplitude = 3.0f;

    [SerializeField, Tooltip("上下移動の速さ（周波数）")]
    private float verticalFrequency = 2.0f;

    [SerializeField, Tooltip("実際に上下に揺らすオブジェクト（見た目とコライダーを持つもの）")]
    private Transform modelTransform;

    private float elapsedTime = 0f;
    private float initialLocalY;
    private bool isInitialized = false;

    protected override void Awake()
    {
        base.Awake();
        
        // modelTransformが未設定の場合は、自身のTransformを使用する
        if (modelTransform == null)
        {
            modelTransform = transform;
        }

        initialLocalY = modelTransform.localPosition.y;
        isInitialized = true;
    }

    void LateUpdate()
    {
        if (!isMoving || isStunned > 0) return;
        if (!isInitialized) return;

        if (enableVerticalMovement && modelTransform != null)
        {
            float currentMultiplier = GetTotalSpeedMultiplier();
            // LateUpdateなのでTime.deltaTimeを使用
            elapsedTime += Time.deltaTime * currentMultiplier;

            // サイン波を用いてオフセットを計算
            float yOffset = Mathf.Sin(elapsedTime * verticalFrequency) * verticalAmplitude;
            
            // localPositionを操作して上下に揺らす
            Vector3 newLocalPos = modelTransform.localPosition;
            newLocalPos.y = initialLocalY + yOffset;
            modelTransform.localPosition = newLocalPos;
        }
    }
}
