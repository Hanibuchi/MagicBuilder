using UnityEngine;

public class AttractionEffect : MonoBehaviour
{
    private Transform _target;
    private Animator _animator;
    private bool _isDestroying = false;

    [Tooltip("範囲に対するスケールの比例定数")]
    [SerializeField] private float scaleMultiplier = 1.0f;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void Init(Transform target, float range)
    {
        _target = target;
        transform.localScale = Vector3.one * (range * scaleMultiplier);
    }

    private void Update()
    {
        if (_target != null)
        {
            transform.position = _target.position;
        }
    }

    public void Finish()
    {
        if (_isDestroying) return;
        _isDestroying = true;

        if (_animator != null)
        {
            _animator.SetTrigger("Destroy");
        }

        // 数秒後にオブジェクトを破棄（アニメーションの長さを考慮）
        Destroy(gameObject, 0.5f);
    }
}
