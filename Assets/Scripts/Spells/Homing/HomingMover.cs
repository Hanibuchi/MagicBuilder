using UnityEngine;

public class HomingMover : MonoBehaviour
{
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private float _searchRange = 5f;
    [SerializeField] private float _springConstant = 1f; // ばね定数 (距離に比例する力)

    private Rigidbody2D _rb;
    private Transform _target;
    private bool _isInitialized = false;

    private void Awake()
    {
        _rb = GetComponentInChildren<Rigidbody2D>();
    }

    public void Initialize(LayerMask targetLayer, float searchRange, float springConstant)
    {
        _targetLayer = targetLayer;
        _searchRange = searchRange;
        _springConstant = springConstant;
        _isInitialized = true;
    }

    public void AddSpringConstant(float value)
    {
        _springConstant += value;
    }

    private void FixedUpdate()
    {
        if (!_isInitialized || _rb == null) return;

        if (_target == null || !IsTargetValid(_target))
        {
            _target = FindNearestTarget();
        }

        if (_target != null)
        {
            MoveTowardsTarget();
        }
    }

    private bool IsTargetValid(Transform target)
    {
        if (target == null) return false;
        if (Vector2.Distance(transform.position, target.position) > _searchRange)
        {
            return false;
        }
        if (!target.gameObject.activeInHierarchy)
        {
            return false;
        }
        return true;
    }

    private Transform FindNearestTarget()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _searchRange, _targetLayer);
        Transform nearest = null;
        float minDistance = float.MaxValue;

        foreach (var col in colliders)
        {
            // 自身や自身の子オブジェクトは除外する
            if (col.transform.IsChildOf(transform)) continue;

            float distance = Vector2.Distance(transform.position, col.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = col.transform;
            }
        }

        return nearest;
    }

    private void MoveTowardsTarget()
    {
        // ターゲットへの変位ベクトル (x)
        Vector2 offset = (Vector2)_target.position - _rb.position;

        // 復元力 (F = kx)
        Vector2 springForce = offset * _springConstant;

        _rb.AddForce(springForce);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _searchRange);
    }
}
