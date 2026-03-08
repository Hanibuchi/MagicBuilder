// LayerSensor.cs
using UnityEngine;

public class LayerSensor : MonoBehaviour
{
    [Header("センサー設定")]
    [Tooltip("感知対象とするLayerマスク")]
    [SerializeField]
    private LayerMask targetLayer;

    public string TriggerID => triggerID;
    [Tooltip("トリガーを区別するためのID")]
    [SerializeField] string triggerID = "attack1";

    // 登録されたハンドラーのインスタンス
    private ITriggerHandler handlerInstance;

    /// <summary>
    /// 通知を受け取るハンドラーのインスタンスを設定します。
    /// </summary>
    /// <param name="handler">ITriggerHandlerインターフェースを実装したインスタンス。</param>
    public void SetHandler(ITriggerHandler handler)
    {
        handlerInstance = handler;
    }

    // 衝突開始時
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"LayerSensor: {other.gameObject.name} がトリガーに入った。");
        // 衝突したオブジェクトのLayerが指定のtargetLayerに含まれているかチェック
        if ((targetLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            // ハンドラーが設定されていれば通知を実行
            handlerInstance?.OnTriggerSensed(triggerID, other.transform.position);
        }
    }

    // 衝突継続中（毎フレーム）
    private void OnTriggerStay2D(Collider2D other)
    {
        // Debug.Log($"LayerSensor: {other.gameObject.name} がトリガー内に滞在中。");
        // 衝突したオブジェクトのLayerが指定のtargetLayerに含まれているかチェック
        // ※ 毎フレームの呼び出しは負荷になる場合があるため、使用頻度を考慮してください。
        if ((targetLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            // Debug.Log($"LayerSensor: {other.gameObject.name} がトリガー内に滞在中で、対象Layerに含まれています。");
            // ハンドラーが設定されていれば通知を実行
            handlerInstance?.OnTriggerSensed(triggerID, other.transform.position);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"LayerSensor: {other.gameObject.name} がトリガーから出た。");
        // 離脱したオブジェクトのLayerが指定のtargetLayerに含まれているかチェック
        if ((targetLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            // ハンドラーが設定されていれば、離脱通知を実行
            // 離脱時はターゲットの位置情報 (Vector2 target) は不要なため、IDのみ渡します。
            handlerInstance?.OnTriggerExited(triggerID);
        }
    }
}

public interface ITriggerHandler
{
    /// <summary>
    /// LayerSensorによってトリガーが感知された際に呼び出されます。
    /// </summary>
    /// <param name="triggerID">トリガーを区別するためのID。</param>
    /// <param name="target">感知された対象のワールド座標。</param>
    void OnTriggerSensed(string triggerID, Vector2 target);

    /// <summary>
    /// LayerSensorからトリガー対象が離脱した際に呼び出されます。（離脱）
    /// </summary>
    /// <param name="triggerID">トリガーを区別するためのID。</param>
    void OnTriggerExited(string triggerID); // ★ このメソッドを追加
}