using UnityEngine;

/// <summary>
/// 発射体を最も近いターゲットへ即座にワープさせるコンポーネント。
/// </summary>
public class TeleportHomingMover : MonoBehaviour
{
    private LayerMask _targetLayer;
    private float _searchRange;
    private float _teleportInterval;
    private float _timer;
    private bool _isInitialized = false;

    private Vector2 _relativeOffset;
    private Transform _lastTarget;

    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponentInChildren<Rigidbody2D>();
    }

    /// <summary>
    /// ホーミング設定を初期化し、即座にワープを実行します。
    /// </summary>
    public void Initialize(LayerMask targetLayer, float searchRange, float teleportInterval)
    {
        _targetLayer = targetLayer;
        _searchRange = searchRange;
        _teleportInterval = teleportInterval;
        _isInitialized = true;
        _timer = 0f; // 初回は即座にワープ

        ExecuteTeleport();
    }

    private void FixedUpdate()
    {
        if (!_isInitialized) return;

        _timer -= Time.fixedDeltaTime;
        if (_timer <= 0f)
        {
            ExecuteTeleport();
            _timer = _teleportInterval;
        }
    }

    private void ExecuteTeleport()
    {
        Collider2D targetCollider = FindNearestTarget();
        if (targetCollider != null)
        {
            Transform targetTransform = targetCollider.transform;

            // ターゲットが変わった場合、または初回のみ相対位置を計算して保存
            if (targetTransform != _lastTarget)
            {
                _lastTarget = targetTransform;
                Vector2 targetTransformPos = targetTransform.position;
                // ターゲットのコライダー上で、現在の放射物に最も近い座標を取得
                Vector2 closestPoint = targetCollider.ClosestPoint(transform.position);
                // ターゲットのTransform座標との相対的な位置（オフセット）を計算して保存
                _relativeOffset = closestPoint - targetTransformPos;
            }

            // 保存された相対位置だけずらした地点を目的地にする
            Vector2 destination = (Vector2)targetTransform.position + _relativeOffset;
            
            if (_rb != null)
            {
                // 慣性をリセットしてターゲット上で静止するようにする
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;
                _rb.position = destination;
            }
            transform.position = destination;
        }
        else
        {
            _lastTarget = null;
        }
    }

    private Collider2D FindNearestTarget()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _searchRange, _targetLayer);
        Collider2D nearest = null;
        float minDistance = float.MaxValue;

        foreach (var col in colliders)
        {
            // 自身や自身の子オブジェクトは除外
            if (col.transform.IsChildOf(transform)) continue;

            float distance = Vector2.Distance(transform.position, col.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = col;
            }
        }

        return nearest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _searchRange);
    }
}
