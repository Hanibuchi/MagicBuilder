using UnityEngine;
using System.Collections.Generic;

public class PenetrationModifier : MonoBehaviour
{
    private float _durationTimer = -1f;
    private bool _originalIsTrigger;
    private Dictionary<Collider2D, bool> _originalStates = new Dictionary<Collider2D, bool>();

    public void Initialize(float duration)
    {
        if (TryGetComponent<Collider2D>(out var col))
        {
            _originalIsTrigger = col.isTrigger;
            col.isTrigger = true;
        }

        if (TryGetComponent<MyObjectController>(out var controller) && !controller.IsProjectile)
        {
            foreach (var collider in controller.GetHitBoxes())
            {
                if (collider != null && !_originalStates.ContainsKey(collider))
                {
                    _originalStates[collider] = collider.isTrigger;
                    collider.isTrigger = true;
                }
            }
        }
        _durationTimer = duration;
    }

    public void AddEffect(float duration)
    {
        _durationTimer = duration;
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
        if (TryGetComponent<Collider2D>(out var col))
        {
            col.isTrigger = _originalIsTrigger;
        }

        foreach (var kvp in _originalStates)
        {
            if (kvp.Key != null)
            {
                kvp.Key.isTrigger = kvp.Value;
            }
        }
    }
}
