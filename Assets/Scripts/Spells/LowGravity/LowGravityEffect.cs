using UnityEngine;

public class LowGravityEffect : MonoBehaviour
{
    [SerializeField] private float destroyDelay = 3f;

    private Transform _target;
    private Vector3 _offset;
    private Animator _animator;
    private bool _isEnding = false;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void Initialize(Transform target, Vector3 offset)
    {
        _target = target;
        _offset = offset;
    }

    private void Update()
    {
        if (_isEnding) return;

        if (_target != null)
        {
            transform.position = _target.position + _offset;
        }
        else
        {
            EndEffect();
        }
    }

    public void EndEffect()
    {
        if (_isEnding) return;
        _isEnding = true;

        if (_animator != null)
        {
            _animator.SetTrigger("End");
        }

        Destroy(gameObject, destroyDelay);
    }
}
