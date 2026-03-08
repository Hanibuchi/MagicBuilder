using UnityEngine;
using System.Collections.Generic;

public class LowGravityModifier : MonoBehaviour
{
    private float _durationTimer = -1f;
    private float _totalGravityChange = 0f;
    private Rigidbody2D _rb;

    private List<LowGravityEffect> _activeEffects = new List<LowGravityEffect>();

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(float gravityChange, float duration, GameObject effectPrefab, Vector3 effectOffset)
    {
        ApplyEffect(gravityChange, effectPrefab, effectOffset);
        _durationTimer = duration;
    }

    public void AddEffect(float gravityChange, float duration, GameObject effectPrefab, Vector3 effectOffset)
    {
        ApplyEffect(gravityChange, effectPrefab, effectOffset);
        _durationTimer = duration;
    }

    private void ApplyEffect(float change, GameObject effectPrefab, Vector3 offset)
    {
        if (_rb != null)
        {
            _rb.gravityScale += change;
            _totalGravityChange += change;

            // Instantiation
            if (effectPrefab != null)
            {
                var go = Instantiate(effectPrefab, transform.position, Quaternion.identity);
                var effect = go.GetComponent<LowGravityEffect>();
                if (effect != null)
                {
                    effect.Initialize(transform, offset);
                    _activeEffects.Add(effect);
                }
            }
        }
    }

    private void Update()
    {
        if (_durationTimer > 0)
        {
            _durationTimer -= Time.deltaTime;
            if (_durationTimer <= 0)
            {
                EndAllEffects();
                Destroy(this);
            }
        }
    }

    private void EndAllEffects()
    {
        foreach (var effect in _activeEffects)
        {
            if (effect != null)
            {
                effect.EndEffect();
            }
        }
        _activeEffects.Clear();
    }

    private void OnDestroy()
    {
        if (_rb != null)
        {
            // _rbがまだ存在する場合のみ元に戻す
            _rb.gravityScale -= _totalGravityChange;
        }
        EndAllEffects();
    }
}
