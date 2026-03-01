using UnityEngine;
using System.Collections;

/// <summary>
/// 追従呪文の弾丸に付与され、中心の弾丸の進行方向に対して直角にバネ運動を行うコンポーネント。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class FollowingSatellite : MonoBehaviour
{
    private Transform _center;
    private Rigidbody2D _rb;
    private Rigidbody2D _centerRb;
    private float _amplitude;
    private float _frequency;
    private float _side; // 1 or -1
    private bool _isInitialized = false;

    private float _startTime;

    public void Init(Transform center, float side, float amplitude, float frequency)
    {
        _center = center;
        _side = side;
        _amplitude = amplitude;
        _frequency = frequency;
        _rb = GetComponent<Rigidbody2D>();
        if (_center != null) _centerRb = _center.GetComponent<Rigidbody2D>();

        StartCoroutine(SyncInitialState());
    }

    private IEnumerator SyncInitialState()
    {
        yield return null; // 1フレーム待機してメイン弾の速度が確定するのを待つ

        if (_rb != null && _centerRb != null)
        {
            // 進行方向を基準に上下に配置
            Vector2 forward = _centerRb.linearVelocity.sqrMagnitude > 0.01f 
                ? _centerRb.linearVelocity.normalized 
                : (Vector2)_center.right;
            
            Vector2 perpendicular = new Vector2(-forward.y, forward.x);
            
            // 初期位置設定
            transform.position = (Vector2)_center.position + perpendicular * (_side * _amplitude);
            
            // 速度をメイン弾と完全に同期
            _rb.linearVelocity = _centerRb.linearVelocity;
            
            _startTime = Time.time;
            _isInitialized = true;
        }
    }

    private void FixedUpdate()
    {
        if (!_isInitialized || _center == null || _centerRb == null || _rb == null) return;

        // 1. 基本的な速度同期（メイン弾の動きに追従）
        // オフセットを維持するために力を加える
        Vector2 desiredPosBase = (Vector2)_center.position;
        
        // 2. 進行方向に対して直角なバネ運動（正弦波による擬似バネ運動）
        Vector2 forward = _centerRb.linearVelocity.sqrMagnitude > 0.01f 
            ? _centerRb.linearVelocity.normalized 
            : (Vector2)_center.right;
        Vector2 perpendicular = new Vector2(-forward.y, forward.x);

        float phase = (Time.time - _startTime) * _frequency * 2f * Mathf.PI;
        float currentOffset = Mathf.Cos(phase) * (_side * _amplitude);
        
        Vector3 targetPos = (Vector2)_center.position + perpendicular * currentOffset;

        // 硬い追従を実現するために位置を直接補正するか、AddForceを使用する
        // 追従呪文なので、メイン弾から離れすぎないように位置を強めに同期
        Vector2 diff = (Vector2)targetPos - _rb.position;
        
        // 速度も同期させつつ、位置のズレを解消する力を加える
        _rb.linearVelocity = _centerRb.linearVelocity + (diff / Time.fixedDeltaTime) * 0.5f;
    }
}
