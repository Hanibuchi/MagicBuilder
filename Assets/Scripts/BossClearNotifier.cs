using UnityEngine;

/// <summary>
/// ボス敵にアタッチし、自身が破壊されたときにStageManagerにクリア通知を行うコンポーネント。
/// </summary>
public class BossClearNotifier : MonoBehaviour
{
    /// <summary>
    /// このボスが倒された（破壊される直前）時に呼び出されるパブリックメソッド。
    /// 敵のHPが0になった時の処理から呼び出してください。
    /// </summary>
    public void NotifyDefeated()
    {
        StageManager.Instance.NotifyBossDefeatedForClear();
        // Destroy(gameObject); // この通知の後に、ボスオブジェクトが破壊されることが想定される
    }
}