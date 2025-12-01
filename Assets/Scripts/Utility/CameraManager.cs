using UnityEngine;

/// <summary>
/// メインカメラを制御するシングルトンクラス。
/// 画面のズームと位置調整を行います。
/// </summary>
public class CameraManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static CameraManager Instance { get; private set; }

    private Camera mainCamera; // インスペクターで設定するメインカメラ

    private float _defaultOrthographicSize; // 起動時のデフォルトOrthographic Size

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

        // mainCameraが設定されているか確認
        if (mainCamera == null)
        {
            // 設定されていなければ、メインカメラタグのカメラを探す
            mainCamera = Camera.main;
        }

        // カメラのOrthographic Size（画面の高さの半分のワールド座標での大きさ）をデフォルトとして保存
        if (mainCamera != null && mainCamera.orthographic)
        {
            _defaultOrthographicSize = mainCamera.orthographicSize;
        }
        else
        {
            Debug.LogError("CameraController: メインカメラが設定されていないか、Orthographic設定になっていません。");
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
        if (mainCamera == null || !mainCamera.orthographic) return;

        currentRelativeSize = relativeSize;
        // relativeSizeが大きいほど、画面が大きく（より広く）映る = Orthographic Sizeが大きくなる
        // Orthographic Size = デフォルトサイズ * relativeSize
        mainCamera.orthographicSize = _defaultOrthographicSize * relativeSize;
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
        if (mainCamera == null) return;

        currentWorldPos = worldPosition;

        // カメラのZ座標は変更せず、XとY座標を更新
        Vector3 newPosition = new Vector3(
            worldPosition.x,
            worldPosition.y,
            mainCamera.transform.position.z // 現在のZを維持
        );

        mainCamera.transform.position = newPosition;
    }

    public Vector2 GetWorldPosition()
    {
        return currentWorldPos;
    }

    // public float test_size = 2;
    // public void Test()
    // {
    //     SetRelativeCameraSize(test_size);
    // }
    // public void Test2()
    // {
    //     SetCameraPosition(transform.position);
    // }
}