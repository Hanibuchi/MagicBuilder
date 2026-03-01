using UnityEngine;

public class ExpansionModifier : MonoBehaviour
{
    private float _durationTimer = -1f;
    private Vector3 _totalScaleMultiplier = Vector3.one;

    public void Initialize(float scaleMultiplier, float duration)
    {
        ApplyEffect(scaleMultiplier);
        _durationTimer = duration;
    }

    public void AddEffect(float scaleMultiplier, float duration)
    {
        ApplyEffect(scaleMultiplier);
        _durationTimer = duration;
    }

    private void ApplyEffect(float multiplier)
    {
        transform.localScale *= multiplier;
        _totalScaleMultiplier *= multiplier;
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
        // 効果を解除（逆数を掛ける）
        if (Mathf.Abs(_totalScaleMultiplier.x) > 0.0001f && 
            Mathf.Abs(_totalScaleMultiplier.y) > 0.0001f && 
            Mathf.Abs(_totalScaleMultiplier.z) > 0.0001f)
        {
            transform.localScale = new Vector3(
                transform.localScale.x / _totalScaleMultiplier.x,
                transform.localScale.y / _totalScaleMultiplier.y,
                transform.localScale.z / _totalScaleMultiplier.z
            );
        }
    }
}
