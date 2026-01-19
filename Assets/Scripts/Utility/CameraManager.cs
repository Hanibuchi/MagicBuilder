using UnityEngine;
using Unity.Cinemachine;
using Unity.VisualScripting;

/// <summary>
/// メインカメラを制御するシングルトンクラス。
/// Cinemachineの仮想カメラを通じて画面のズームと位置調整を行います。
/// </summary>
public class CameraManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static CameraManager Instance { get; private set; }

    [SerializeField] private CinemachineCamera virtualCamera; // 仮想カメラへの参照
    [SerializeField] private CinemachineImpulseSource impulseSource; // 集中管理用インパルスソース

    private float _defaultOrthographicSize; // 起動時のデフォルトOrthographic Size
    public float DefaultOrthographicSize => _defaultOrthographicSize;

    private void Awake()
    {
        // シングルトンパターンの実装
        if (Instance == null)
        {
            Instance = this;
            // シーンをまたいでも破棄されないようにする場合は以下をコメント解除
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 既にインスタンスが存在する場合、このオブジェクトを破棄
            Destroy(gameObject);
            return;
        }

        // 仮想カメラが未設定なら自動取得を試みる
        if (virtualCamera == null)
        {
            virtualCamera = FindFirstObjectByType<CinemachineCamera>();
        }

        // インパルスソースが未設定なら自動取得を試みる
        if (impulseSource == null)
        {
            impulseSource = GetComponent<CinemachineImpulseSource>();
        }

        SetOrthographicSize();
    }

    /// <summary>
    /// 指定した座標でカメラ振動を発生させます。
    /// </summary>
    /// <param name="position">振動の発生位置</param>
    /// <param name="force">振動の強さ</param>
    public void RequestImpulse(float force = 1.0f)
    {
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse(force);
        }
    }

    public void SetOrthographicSize()
    {
        // カメラのOrthographic Size（画面の高さの半分のワールド座標での大きさ）をデフォルトとして保存
        if (virtualCamera != null)
        {
            _defaultOrthographicSize = virtualCamera.Lens.OrthographicSize;
        }
        else
        {
            Debug.LogError("CameraManager: CinemachineCamera が設定されていないか、見つかりません。");
        }
    }

    float currentRelativeSize = 1;

    /// <summary>
    /// 画面の映す範囲を、標準サイズ(1)に対する相対的な大きさ（相似比）で変更します。
    /// 相対的な大きさは Orthographic Size の逆数に比例します。
    /// ズームアウト（より広く映す）には、relativeSizeを1より大きくします。
    /// </summary>
    /// <param name="relativeSize">標準サイズを1としたときの相対的な画面の大きさ（相似比）。</param>
    public void SetRelativeCameraSize(float relativeSize)
    {
        if (virtualCamera == null) return;

        currentRelativeSize = relativeSize;

        // LensSettingsは構造体なので、取得してから書き戻す
        var lens = virtualCamera.Lens;
        lens.OrthographicSize = _defaultOrthographicSize * relativeSize;
        virtualCamera.Lens = lens;
    }

    public float GetSize()
    {
        return currentRelativeSize;
    }

    Vector2 currentWorldPos;
    /// <summary>
    /// 指定されたワールド座標を中心としてカメラが映すように移動します。
    /// Z座標はカメラの現在のZ座標を維持します。
    /// </summary>
    /// <param name="worldPosition">カメラの中心としたいワールド座標。</param>
    public void SetCameraPosition(Vector2 worldPosition)
    {
        if (virtualCamera == null) return;

        currentWorldPos = worldPosition;

        // 仮想カメラのZ座標は変更せず、XとY座標を更新
        Vector3 newPosition = new Vector3(
            worldPosition.x,
            worldPosition.y,
            virtualCamera.transform.position.z // 現在のZを維持
        );

        virtualCamera.transform.position = newPosition;
    }

    public Vector2 GetWorldPosition()
    {
        return currentWorldPos;
    }
}