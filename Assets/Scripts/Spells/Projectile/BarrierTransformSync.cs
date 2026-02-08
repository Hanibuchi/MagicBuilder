using UnityEngine;

/// <summary>
/// 開始時に親子関係を解除し、元の親オブジェクト（ターゲット）に対して相対的な位置・回転・スケールを同期し続けるコンポーネント。
/// </summary>
public class BarrierTransformSync : MonoBehaviour
{
    private Transform _target;
    private Vector3 _initialLocalPosition;
    private Quaternion _initialLocalRotation;
    private Vector3 _initialLocalScale;

    private void Start()
    {
        _target = transform.parent;
        if (_target == null)
        {
            Debug.LogWarning($"[BarrierTransformSync] {gameObject.name} に親オブジェクトが見つかりません。同期をキャンセルします。");
            enabled = false;
            return;
        }

        // 初期状態での相対的なトランスフォーム情報を保存
        _initialLocalPosition = transform.localPosition;
        _initialLocalRotation = transform.localRotation;
        _initialLocalScale = transform.localScale;

        // 親子関係を解除（グローバル空間へ移動）
        transform.SetParent(null);
    }

    private void LateUpdate()
    {
        // ターゲットが破棄されている、または非アクティブな場合は自身も処理を止める（または破棄する）
        if (_target == null)
        {
            Destroy(gameObject);
            return;
        }

        // --- 位置の同期 ---
        // 親のワールド座標、回転、スケールを考慮して、元のローカル位置をワールド座標に変換
        transform.position = _target.TransformPoint(_initialLocalPosition);

        // --- 回転の同期 ---
        transform.rotation = _target.rotation * _initialLocalRotation;

        // --- スケールの同期 ---
        // ターゲットの（親としての）localScaleに、自身の初期localScaleを掛け合わせる
        Vector3 targetScale = _target.localScale;
        transform.localScale = new Vector3(
            targetScale.x * _initialLocalScale.x,
            targetScale.y * _initialLocalScale.y,
            targetScale.z * _initialLocalScale.z
        );
    }
}
