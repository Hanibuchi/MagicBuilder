using UnityEngine;

/// <summary>
/// メガビームの予測表示用コンポーネント。
/// MegaBeamProjectileと似たロジックでLineRendererを更新しますが、
/// ダメージ判定や音声再生などの実機能は持ちません。
/// </summary>
public class MegaBeamTrajectory : MonoBehaviour
{
    [Header("Beam Settings")]
    [SerializeField] private float maxLength = 20f;
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform tipObject;
    [Tooltip("先端オブジェクトの位置オフセット")]
    [SerializeField] private Vector3 tipOffset;

    private float initialWidth;

    private void Awake()
    {
        if (lineRenderer != null) initialWidth = lineRenderer.widthMultiplier;
    }

    private void OnEnable()
    {
        // プールから復帰した際にスケールがリセットされる場合があるため
        // もしくは、初期化順序の都合でAwakeで取れなかった場合への保険
        if (lineRenderer != null && initialWidth == 0) initialWidth = lineRenderer.widthMultiplier;
    }

    /// <summary>
    /// ビームの状態を更新します。
    /// </summary>
    /// <param name="origin">開始点</param>
    /// <param name="direction">方向ベクトル</param>
    public void UpdateBeam(Vector3 origin, Vector2 direction)
    {
        transform.position = origin;
        transform.right = direction;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxLength, collisionLayers);
        Vector3 endPos;

        if (hit.collider != null)
        {
            endPos = hit.point;
        }
        else
        {
            endPos = origin + (Vector3)(direction * maxLength);
        }

        if (lineRenderer != null)
        {
            // 初期値にオブジェクトのスケールを掛け合わせて太さに適用
            lineRenderer.widthMultiplier = initialWidth * transform.localScale.y;

            if (lineRenderer.positionCount != 2)
                lineRenderer.positionCount = 2;

            if (lineRenderer.useWorldSpace)
            {
                lineRenderer.SetPosition(0, origin);
                lineRenderer.SetPosition(1, endPos);
            }
            else
            {
                lineRenderer.SetPosition(0, Vector3.zero);
                lineRenderer.SetPosition(1, transform.InverseTransformPoint(endPos));
            }
        }

        if (tipObject != null)
        {
            // 回転を考慮したオフセットの適用
            tipObject.position = endPos + transform.TransformDirection(tipOffset);
            tipObject.right = direction;
        }
    }
}
