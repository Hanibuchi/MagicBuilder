using UnityEngine;

/// <summary>
/// Rigidbody2Dの速度ベクトルに基づき、オブジェクトを進行方向に回転させます。
/// </summary>
public class ProjectileController : MonoBehaviour
{
    [SerializeField, Tooltip("左方向移動時にスプライトを上下反転させ、見た目上の上下を維持するためのSpriteRenderer")]
    private SpriteRenderer spriteRenderer;

    [SerializeField, Tooltip("左方向移動時にSpriteRendererを上下反転（FlipY）して、見た目の上下を維持するかどうか")]
    private bool flipYToMaintainOrientation = false;

    private Rigidbody2D rb;
    void Start()
    {
        Initialize(GetComponent<Rigidbody2D>());
    }

    public void Initialize(Rigidbody2D body)
    {
        rb = body;
    }

    void Update()
    {
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.001f)
        {
            // 速度ベクトルからZ軸の回転角度を計算
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;

            // 自身のTransformに回転を適用
            transform.rotation = Quaternion.Euler(0, 0, angle);

            if (flipYToMaintainOrientation && spriteRenderer != null)
            {
                // X方向が正ならそのまま(false)、負ならY反転(true)
                spriteRenderer.flipY = rb.linearVelocity.x < 0f;
            }
        }
    }
}