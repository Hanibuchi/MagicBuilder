using UnityEngine;

/// <summary>
/// 衛星呪文の弾丸に付与され、OrbitCenterに向かって引力を発生させるコンポーネント。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class OrbitalSatellite : MonoBehaviour
{
    private Transform _center;
    private Rigidbody2D _rb;
    private float _radius;
    private float _speed;
    
    [Header("軌道設定")]
    [Tooltip("中心に向かう力の倍率")]
    public float attractionForceMagnitude = 20f;

    public void Init(Transform center, float radius, float speed)
    {
        _center = center;
        _radius = radius;
        _speed = speed;
        _rb = GetComponent<Rigidbody2D>();

        if (_rb != null && _center != null)
        {
            // 中心への方向
            Vector2 toCenter = (_center.position - transform.position).normalized;
            // 接線方向（回転方向）
            Vector2 tangent = new Vector2(toCenter.y, -toCenter.x);

            // 中心の速度を取得
            Rigidbody2D centerRb = _center.GetComponent<Rigidbody2D>();
            Vector2 centerVel = centerRb != null ? centerRb.linearVelocity : Vector2.zero;

            // 初期速度を設定: 中心の速度 + 接線方向の速度（回転用）
            // 速度の大きさは呪文本来のスピードを考慮
            _rb.linearVelocity = centerVel + tangent * (_speed * 0.8f);
        }
    }

    private void FixedUpdate()
    {
        if (_center == null || _rb == null) return;

        // 中心へのベクトル
        Vector2 offset = (Vector2)_center.position - _rb.position;
        float distance = offset.magnitude;
        Vector2 direction = offset.normalized;

        // 1. 中心の速度に追従するための力（あるいは速度の同期）
        // 弾丸が置いていかれないように、中心のRigidbodyがあればその速度をベースに考える
        Rigidbody2D centerRb = _center.GetComponent<Rigidbody2D>();
        if (centerRb != null)
        {
            // 2. 向心力を与える (F = m * v^2 / r)
            // 実際は魔法なので少し強めに引き寄せる
            float forceScale = attractionForceMagnitude;
            
            // 距離が離れすぎている場合は強く引き、近い場合は弱める
            float diff = distance - _radius;
            float extraForce = diff * 10f;

            _rb.AddForce(direction * (forceScale + extraForce), ForceMode2D.Force);
            
            // 3. 進行方向（中心の移動方向）への補正
            // 中心の速度成分を維持するように力を加える
            Vector2 relativeVel = _rb.linearVelocity - centerRb.linearVelocity;
            // 接線方向の速度を維持
            // (ここでは引力だけで回転が維持される想定だが、減衰対策として少し接線方向に加速してもよい)
        }
        else
        {
            // 中心が消失した場合は、そのまま直進するように設定、あるいは消滅
            Destroy(this);
        }
    }
}
