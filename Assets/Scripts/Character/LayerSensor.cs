// LayerSensor.cs
using UnityEngine;

public class LayerSensor : MonoBehaviour
{
    [Header("センサー設定")]
    [Tooltip("感知対象とするLayerマスク")]
    [SerializeField]
    private LayerMask targetLayer;

    [Tooltip("トリガーを区別するためのID")]
    [SerializeField] string triggerID = "Default";

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
        // 衝突したオブジェクトのLayerが指定のtargetLayerに含まれているかチェック
        if ((targetLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            // ハンドラーが設定されていれば通知を実行
            handlerInstance?.OnTriggerSensed(triggerID);
        }
    }

    // 衝突継続中（毎フレーム）
    private void OnTriggerStay2D(Collider2D other)
    {
        // 衝突したオブジェクトのLayerが指定のtargetLayerに含まれているかチェック
        // ※ 毎フレームの呼び出しは負荷になる場合があるため、使用頻度を考慮してください。
        if ((targetLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            // ハンドラーが設定されていれば通知を実行
            handlerInstance?.OnTriggerSensed(triggerID);
        }
    }
}

public interface ITriggerHandler
{
    /// <summary>
    /// LayerSensorによってトリガーが感知された際に呼び出されます。
    /// </summary>
    /// <param name="triggerID">トリガーを区別するためのID。</param>
    void OnTriggerSensed(string triggerID);
}