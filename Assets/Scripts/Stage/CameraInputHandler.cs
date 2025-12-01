using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UIオブジェクト上での入力を受け付け、CameraManagerを通じてカメラを制御する。
/// - ドラッグ操作でカメラを移動。
/// - 2本指のピンチ操作でカメラのズームを行う。
/// </summary>
public class CameraInputHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private Transform limitPointA;
    [SerializeField] private Transform limitPointB;

    // ★追加: カメラが移動できる境界のワールド座標
    private float minX;
    private float maxX;
    private float minY;
    private float maxY;

    // ★追加: カメラのズーム制限 (任意)
    [SerializeField] float minRelativeSize = 1f; // 最小ズーム (最大拡大)
    [SerializeField] float maxRelativeSize = 5.0f; // 最大ズーム (最小拡大)

    // カメラ移動用
    private bool isDragging = false;
    private Vector2 startScreenPosition; // ドラッグ開始時の指のスクリーン座標
    private Vector2 startCameraWorldPosition; // ドラッグ開始時のカメラの中心ワールド座標
    private Camera mainCamera; // メインカメラのキャッシュ

    // ズーム用
    private float initialPinchDistance = 0f; // ピンチ開始時の2本指間のスクリーン距離
    private float initialCameraRelativeSize = 0f; // ピンチ開始時のカメラの相対サイズ

    private void Awake()
    {
        // メインカメラの取得
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("CameraInputHandler: メインカメラが見つかりません。");
        }

        // ★追加: 境界のワールド座標を計算
        if (limitPointA != null && limitPointB != null)
        {
            float ax = limitPointA.position.x;
            float ay = limitPointA.position.y;
            float bx = limitPointB.position.x;
            float by = limitPointB.position.y;

            // min/maxを計算 (AとBの座標の小さい方と大きい方をそれぞれmin/maxとする)
            minX = Mathf.Min(ax, bx);
            maxX = Mathf.Max(ax, bx);
            minY = Mathf.Min(ay, by);
            maxY = Mathf.Max(ay, by);
        }
        else
            Debug.LogWarning("CameraInputHandler: limitPointAまたはlimitPointBが設定されていません。カメラの移動範囲制限は無効になります。");
    }

    private void Update()
    {
        // CameraManagerのインスタンスがない場合は処理を中断
        if (CameraManager.Instance == null || mainCamera == null) return;


        if (isDragging)
        {
            // 複数の指の入力を処理
            int touchCount = Input.touchCount;
            // 1. カメラ移動 (1本以上の指でドラッグ中)
            HandleCameraMovement(touchCount);

            // 2. ピンチ操作 (2本以上の指が触れている場合)
            if (touchCount >= 2)
            {
                HandlePinchZoom();
            }
        }
    }

    // --- カメラ移動の実装 ---
    private int trackingFingerId = -1; // ★追加: ドラッグ操作の基準となる指のID

    /// <summary>
    /// カメラ移動の処理を実行します。
    /// ドラッグ開始地点のワールド座標が、ドラッグ中ずっと指の下に追従するようにカメラを動かします。
    /// </summary>
    /// <param name="currentTouchCount">現在のタッチ数。</param>
    private void HandleCameraMovement(int currentTouchCount)
    {
        // タッチ入力がない場合は、マウスの左クリック(0)でのドラッグを想定
        if (currentTouchCount > 0) // タッチ入力の場合
        {
            Touch? currentTouch = null;
            foreach (Touch touch in Input.touches)
            {
                if (touch.fingerId == trackingFingerId)
                {
                    currentTouch = touch;
                    break;
                }
            }

            if (currentTouch == null)
                return;

            var currentScreenPos = currentTouch.Value.position;

            // スクリーン上の差をワールド座標の差に変換
            Vector2 worldDelta = mainCamera.ScreenToWorldPoint(currentScreenPos) -
                                 mainCamera.ScreenToWorldPoint(startScreenPosition);

            // カメラを移動させる新しい中心ワールド座標
            Vector2 newPosition = startCameraWorldPosition - worldDelta;

            TrySetCameraPosition(newPosition);
        }
        else
        {
            // マウスの左ボタンが押されている場合
            if (!Input.GetMouseButton(0)) return;

            // マウスの現在のスクリーン位置
            Vector2 currentScreenPos = Input.mousePosition;

            // スクリーン上の差をワールド座標の差に変換
            Vector2 worldDelta = mainCamera.ScreenToWorldPoint(currentScreenPos) -
                                 mainCamera.ScreenToWorldPoint(startScreenPosition);

            // カメラを移動させる新しい中心ワールド座標
            Vector2 newPosition = startCameraWorldPosition - worldDelta;

            TrySetCameraPosition(newPosition);
        }
    }

    // --- ピンチズームの実装 ---

    /// <summary>
    /// 2本指でのピンチ操作によるズーム処理を実行します。
    /// </summary>
    private void HandlePinchZoom()
    {
        // 2本指のタッチがあることを確認
        if (Input.touchCount < 2) return;

        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        // 現在の2本指間の距離
        float currentPinchDistance = Vector2.Distance(touch0.position, touch1.position);

        // initialPinchDistanceが0（ピンチ開始前）の場合は、ここで初期値を設定
        if (initialPinchDistance == 0f)
        {
            initialPinchDistance = currentPinchDistance;
            initialCameraRelativeSize = CameraManager.Instance.GetSize();
            return;
        }

        // 新しい相対的なカメラサイズを設定
        // distanceRatio が大きい（ピンチアウト） => より広い範囲を映す => relativeSize が大きくなる
        float newRelativeSize = initialCameraRelativeSize * initialPinchDistance / currentPinchDistance;

        // ズーム制限（任意）：極端なズームを避けるための最小/最大値
        // newRelativeSize = Mathf.Clamp(newRelativeSize, 0.5f, 5.0f); 

        TrySetRelativeCameraSize(newRelativeSize);
    }


    private void TrySetCameraPosition(Vector2 newPosition)
    {
        if (CameraManager.Instance == null || mainCamera == null) return;

        // カメラの現在のOrthographic Size (画面の高さの半分)
        float halfHeight = mainCamera.orthographicSize;
        // アスペクト比から画面の幅の半分を計算
        float halfWidth = halfHeight * mainCamera.aspect;

        // X軸の制限
        float targetMinX = minX + halfWidth;
        float targetMaxX = maxX - halfWidth;
        if (targetMinX > targetMaxX)
            return;

        // Y軸の制限
        float targetMinY = minY + halfHeight;
        float targetMaxY = maxY - halfHeight;
        if (targetMinY > targetMaxY)
            return;

        // Clampを適用して、新しい中心座標を計算
        // ただし、境界がカメラのサイズより狭い場合（例えばmaxX - minX < halfWidth * 2）は、
        // Min/Maxが逆転する可能性があるため、Mathf.Min/Mathf.Maxで安全に処理します。

        float finalX = Mathf.Clamp(newPosition.x, targetMinX, targetMaxX);
        float finalY = Mathf.Clamp(newPosition.y, targetMinY, targetMaxY);

        Vector2 finalPosition = new Vector2(finalX, finalY);

        CameraManager.Instance.SetCameraPosition(finalPosition);
    }

    // ★追加: カメラサイズを制限付きで設定する
    private void TrySetRelativeCameraSize(float newRelativeSize)
    {
        if (CameraManager.Instance == null || mainCamera == null) return;

        // ズーム制限（任意）：極端なズームを避けるための最小/最大値
        float clampedSize = Mathf.Clamp(newRelativeSize, minRelativeSize, maxRelativeSize);

        float defaultHalfHeight = CameraManager.Instance.DefaultOrthographicSize;
        float aspect = mainCamera.aspect;

        // 1. X軸方向のサイズ制限を計算

        // X軸方向で境界内におさまるかチェック
        // (maxX - minX)は境界の幅。これが画面の幅(currentHalfWidth * 2)よりも小さければ、
        // 画面の幅を境界の幅に合わせて縮小しなければならない。
        // 境界の幅に完全に収まる最大の relativeSize を計算
        float maxWidthRelativeSize = (maxX - minX) / (defaultHalfHeight * aspect * 2);
        clampedSize = Mathf.Min(clampedSize, maxWidthRelativeSize);

        // 2. Y軸方向のサイズ制限を計算

        // 境界の高さに完全に収まる最大の relativeSize を計算
        float maxHeightRelativeSize = (maxY - minY) / (defaultHalfHeight * 2);
        clampedSize = Mathf.Min(clampedSize, maxHeightRelativeSize);

        // --- 制限処理の終了 ---

        CameraManager.Instance.SetRelativeCameraSize(clampedSize);

        // サイズ変更後、カメラが範囲外に出ていないかを確認し、必要なら位置を補正
        // (サイズの変更でカメラの視界が広がった結果、端のオブジェクトが範囲外に出るのを防ぐ)
        TrySetCameraPosition(CameraManager.Instance.GetWorldPosition());
    }

    // --- イベントハンドラ (フラグ管理と初期値設定) ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 画面移動のための初期値を設定
        isDragging = true;
        startCameraWorldPosition = CameraManager.Instance.GetWorldPosition();

        if (Input.touchCount > 0)
        {
            trackingFingerId = Input.GetTouch(0).fingerId;
            startScreenPosition = Input.GetTouch(0).position;
        }
        else
            startScreenPosition = Input.mousePosition;

        // ピンチズームのための初期化
        // 2本指になった瞬間に初期化が行われるように、ここでは初期値をリセット
        initialPinchDistance = 0f;
    }

    // IDragHandler の実装。このメソッドは、アタッチされたUIオブジェクトがドラッグされたときに毎フレーム呼ばれます。
    // OnDrag系は2本指以上だと呼び出されないため、フラグを立てる目的で利用します。
    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 最後に離した指が、OnPointerDownで追跡していた最初の指だった場合、ドラッグ終了
        // if (Input.touchCount == 0 || (Input.touchCount == 1 && Input.GetTouch(0).fingerId != eventData.pointerId))
        // 指が完全に離れた、または追跡していた指が離れた場合
        isDragging = false;
        initialPinchDistance = 0f; // ピンチ状態もリセット
    }
}