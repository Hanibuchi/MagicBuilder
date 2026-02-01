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
    [Tooltip("中心に向かう力の倍率")]
    float omega2;


    public void Init(Transform center, bool isUpper, float radius)
    {
        _center = center;
        _isUpper = isUpper;
        this._radius = radius;
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

            // // 標準的な半径を求める: r = v^2 / a
            // if (_radius > 0.001f && initSpeed > 0.001f)
            // {
            //     _radius = (initSpeed * initSpeed) / _radius;

            //     // 現在の中心からの方向ベクトルを維持しつつ、距離を計算した _radius に調整
            //     Vector2 currentOffsetDir = ((Vector2)transform.position - (Vector2)_center.position).normalized;

            //     // 方向が不定（重なっている）場合は、初期設定時の向き（上下）を再現
            //     if (currentOffsetDir.sqrMagnitude < 0.01f)
            //     {
            //         float angle = _center.eulerAngles.z * Mathf.Deg2Rad;
            //         Vector2 up = new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle));
            //         currentOffsetDir = up * (_isUpper ? 1f : -1f);
            //     }

            //     // 自身を計算された公転半径の位置へ移動
            //     transform.position = (Vector2)_center.position + currentOffsetDir * _radius;
            // }
            // else
            // {
            //     _radius = Vector2.Distance(transform.position, _center.position);
            // }
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
