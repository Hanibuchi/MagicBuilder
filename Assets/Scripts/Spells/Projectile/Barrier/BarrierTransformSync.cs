using UnityEngine;

/// <summary>
/// 開始時に親子関係を解除し、元の親オブジェクト（ターゲット）に対して相対的な位置関係のみを同期し続けるコンポーネント。
/// </summary>
public class BarrierTransformSync : MonoBehaviour
{
    private Transform _target;
    private Vector3 _initialLocalPosition;

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
        // 親のスケールに依存しないよう、回転のみを考慮したオフセットを算出
        _initialLocalPosition = Quaternion.Inverse(_target.rotation) * (transform.position - _target.position);

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
        // 親のスケール（反転など）を無視して、初期の相対位置情報のまま追従させる
        transform.position = _target.position + (_target.rotation * _initialLocalPosition);
    }
}
