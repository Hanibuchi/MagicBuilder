using UnityEngine;

/// <summary>
/// Rigidbody2Dの速度ベクトルに基づき、オブジェクトを進行方向に回転させます。
/// </summary>
public class ProjectileController : MonoBehaviour
{
    private Rigidbody2D rb;

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
        }
    }
}