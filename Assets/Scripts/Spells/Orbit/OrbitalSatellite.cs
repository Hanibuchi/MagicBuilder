using UnityEngine;

/// <summary>
/// 衛星呪文の弾丸に付与され、OrbitCenterに向かって引力を発生させるコンポーネント。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class OrbitalSatellite : MonoBehaviour
{
    private Transform _center;
    private Rigidbody2D _rb;
    private Rigidbody2D centerRb;
    private float _radius;
    private bool _isUpper;
    private bool _isInitialized = false;

    [Header("軌道設定")]
    [Tooltip("最小の初期速度。これ未満の場合はこの速度まで加速します。")]
    float minInitSpeed = 3f;
    [Tooltip("中心に向かう力の倍率")]
    float omega2;


    public void Init(Transform center, bool isUpper, float radius, float minInitSpeed)
    {
        _center = center;
        _isUpper = isUpper;
        this._radius = radius;
        this.minInitSpeed = minInitSpeed;
        _rb = GetComponent<Rigidbody2D>();
        if (_center != null) centerRb = _center.GetComponent<Rigidbody2D>();

        StartCoroutine(SyncVelocityDelayed());
    }

    private System.Collections.IEnumerator SyncVelocityDelayed()
    {
        yield return null; // 1フレーム待機

        if (_rb != null && centerRb != null)
        {
            float initSpeed = _rb.linearVelocity.magnitude;

            // 速度が一定値未満なら、接線方向（正しい方向）へ加速させる
            if (initSpeed < minInitSpeed)
            {
                // 中心からの相対位置
                Vector2 relativePos = (Vector2)transform.position - (Vector2)_center.position;
                // 接線方向（時計回り）を計算
                Vector2 tangent = new Vector2(relativePos.y, -relativePos.x).normalized;

                _rb.linearVelocity += tangent * minInitSpeed;
                initSpeed = _rb.linearVelocity.magnitude;
            }

            omega2 = (initSpeed * initSpeed) / (_radius * _radius);

            // 主弾の速度を衛星に加算して同期させる
            _rb.linearVelocity += centerRb.linearVelocity;

            _isInitialized = true;
        }
    }

    private void FixedUpdate()
    {
        if (!_isInitialized || _center == null || _rb == null) return;

        // 中心へのベクトル
        Vector2 offset = (Vector2)_center.position - _rb.position;

        // 1. 中心の速度に追従するための力（あるいは速度の同期）
        // 弾丸が置いていかれないように、中心のRigidbodyがあればその速度をベースに考える
        if (centerRb != null)
        {
            _rb.AddForce(omega2 * offset, ForceMode2D.Force);
        }
        else
        {
            // 中心が消失した場合は、そのまま直進するように設定、あるいは消滅
            Destroy(this);
        }
    }
}
