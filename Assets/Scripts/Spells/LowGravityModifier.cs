using UnityEngine;

public class LowGravityModifier : MonoBehaviour
{
    private float _durationTimer = -1f;
    private float _totalGravityChange = 0f;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(float gravityChange, float duration)
    {
        ApplyEffect(gravityChange);
        _durationTimer = duration;
    }

    public void AddEffect(float gravityChange, float duration)
    {
        ApplyEffect(gravityChange);
        _durationTimer = duration;
    }

    private void ApplyEffect(float change)
    {
        if (_rb != null)
        {
            _rb.gravityScale += change;
            _totalGravityChange += change;
        }
    }

    private void Update()
    {
        if (_durationTimer > 0)
        {
            _durationTimer -= Time.deltaTime;
            if (_durationTimer <= 0)
            {
                Destroy(this);
            }
        }
    }

    private void OnDestroy()
    {
        if (_rb != null)
        {
            // _rbがまだ存在する場合のみ元に戻す
            _rb.gravityScale -= _totalGravityChange;
        }
    }
}
