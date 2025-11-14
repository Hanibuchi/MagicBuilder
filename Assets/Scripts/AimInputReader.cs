using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// 画面のドラッグ入力から発射角度と強さを計算し、IAimControllerに伝えるクラス
/// </summary>
public class AimInputReader : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("設定")]
    [Tooltip("発射の開始地点となるワールド座標")]
    [SerializeField]
    private Transform startPointTransform;

    [Tooltip("最大のドラッグ距離に対応する発射強度 (1.0f)")]
    [SerializeField]
    private float maxDragDistance = 200f;

    private IAimController aimController;

    // 発射開始地点のスクリーン座標を格納するフィールド
    private Vector2 startPointScreenPosition;
    private bool isAiming = false;

    /// <summary>
    /// IAimControllerをセットするためのパブリックメソッド
    /// </summary>
    public void SetAimController(IAimController controller)
    {
        this.aimController = controller;
    }

    /// <summary>
    /// ドラッグ開始時の処理
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Drag Start");
        if (aimController == null || startPointTransform == null)
        {
            Debug.LogError("AimController または StartPointTransform が設定されていません。");
            return;
        }

        isAiming = true;
    }

    /// <summary>
    /// ドラッグ中の処理
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!isAiming || aimController == null) return;

        // 1. ドラッグの変位（画面座標）を計算
        // 現在のドラッグ位置から、発射開始地点のスクリーン座標を引く
        startPointScreenPosition = Camera.main.WorldToScreenPoint(startPointTransform.position);
        Vector2 dragDelta = eventData.position - startPointScreenPosition;

        // 2. 角度と強さを計算
        CalculateAimParameters(dragDelta, out float angle, out float power);

        // 3. IAimControllerのメソッドを呼び出し、補助線を表示
        aimController.UpdateAimLine(angle, power);
    }

    /// <summary>
    /// ドラッグ終了時の処理
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isAiming || aimController == null) return;

        // 1. ドラッグの変位（画面座標）を計算
        // 現在のドラッグ位置から、発射開始地点のスクリーン座標を引く
        startPointScreenPosition = Camera.main.WorldToScreenPoint(startPointTransform.position);
        Vector2 dragDelta = eventData.position - startPointScreenPosition;

        // 2. 角度と強さを計算
        CalculateAimParameters(dragDelta, out float angle, out float power);

        // 3. IAimControllerのメソッドを呼び出し、魔法を発射
        aimController.ReleaseMagic(angle, power);

        isAiming = false;
        aimController.ClearAimLine();
    }

    /// <summary>
    /// ドラッグの変位から角度と強さを計算するヘルパーメソッド
    /// </summary>
    private void CalculateAimParameters(Vector2 dragDelta, out float angle, out float power)
    {
        Vector2 launchVector = -dragDelta;

        // 強度の計算: ドラッグ距離を最大距離で正規化 (0.0f ～ 1.0fにクランプ)
        power = Mathf.Clamp01(launchVector.magnitude / maxDragDistance);

        // 角度の計算: X軸 (右) を基準にした角度
        angle = Mathf.Atan2(launchVector.y, launchVector.x) * Mathf.Rad2Deg;
    }
}


// IAimControllerの定義は変更なし
public interface IAimController
{
    void UpdateAimLine(float angle, float power);
    void ClearAimLine();
    void ReleaseMagic(float angle, float power);
}