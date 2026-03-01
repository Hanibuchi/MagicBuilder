using UnityEngine;

/// <summary>
/// 範囲ビジュアルのサイズ変更（拡大・縮小）と位置同期を担当するクラス。
/// </summary>
public class RangeVisualCloser : MonoBehaviour
{
    [Tooltip("searchRangeに対するプレハブのスケール倍率")]
    [SerializeField] private float visualScaleMultiplier = 2.0f;

    private Transform _followTarget;
    private float _targetScale;
    private float _duration;
    private float _currentScale = 0f;
    private bool _isShrinking = false;
    private float _elapsed = 0f;
    private float _initialScaleOnShrink;

    public void Setup(Transform followTarget, float searchRange, float duration)
    {
        _followTarget = followTarget;
        _targetScale = searchRange * visualScaleMultiplier;
        _duration = duration;
        transform.localScale = Vector3.zero;
        _currentScale = 0f;
        _isShrinking = false;
        _elapsed = 0f;

        SnapToTarget();
    }

    public void SnapToTarget()
    {
        if (_followTarget != null)
        {
            transform.position = _followTarget.position;
        }
    }

    public void ShrinkAndDestroy(float duration)
    {
        _isShrinking = true;
        _duration = duration;
        _elapsed = 0f;
        _initialScaleOnShrink = transform.localScale.x;
        _followTarget = null; // 縮小開始後は追従をやめる
    }

    private void Update()
    {
        // 追従中なら位置を更新
        SnapToTarget();
        if (!_isShrinking)
        {
            // 出現アニメーション（拡大）
            if (_duration > 0)
            {
                float growSpeed = _targetScale / _duration;
                _currentScale = Mathf.MoveTowards(_currentScale, _targetScale, growSpeed * Time.deltaTime);
            }
            else
            {
                _currentScale = _targetScale;
            }
        }
        else
        {
            // 消滅アニメーション（縮小）
            if (_duration <= 0)
            {
                Destroy(gameObject);
                return;
            }

            _elapsed += Time.deltaTime;
            float t = 1f - (_elapsed / _duration);
            _currentScale = Mathf.Max(0, _initialScaleOnShrink * t);

            if (t <= 0)
            {
                Destroy(gameObject);
                return;
            }
        }

        transform.localScale = new Vector3(_currentScale, _currentScale, 1f);
    }
}
