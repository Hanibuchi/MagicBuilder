using UnityEngine;

/// <summary>
/// 敵の移動速度を一時的に変更するコンポーネント。
/// </summary>
public class EnemySpeedModifier : MonoBehaviour
{
    private EnemyMovementBase _movement;
    private float _totalMultiplierApplied = 1.0f;
    private float _durationTimer = 0f;

    private void Awake()
    {
        _movement = GetComponentInParent<EnemyMovementBase>();
        if (_movement == null)
        {
            _movement = GetComponentInChildren<EnemyMovementBase>();
        }
    }

    public void Initialize(float multiplier, float duration)
    {
        if (_movement == null)
        {
            Destroy(this);
            return;
        }

        ApplyEffect(multiplier);
        _durationTimer = duration;
    }

    public void AddEffect(float multiplier, float duration)
    {
        ApplyEffect(multiplier);
        _durationTimer = duration;
    }

    private void ApplyEffect(float multiplier)
    {
        if (_movement != null)
        {
            // 乗算で適用
            _movement.SpellSpeedMultiplier *= multiplier;
            _totalMultiplierApplied *= multiplier;
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
        if (_movement != null && Mathf.Abs(_totalMultiplierApplied) > 0.0001f)
        {
            // 自身が掛けた分だけ割って戻す
            _movement.SpellSpeedMultiplier /= _totalMultiplierApplied;
        }
    }
}
