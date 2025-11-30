using UnityEngine;

/// <summary>
/// アニメーションイベントから呼び出され、指定された石オブジェクトを非表示にします。
/// </summary>
public class StoneDropBatAnimationHelper : MonoBehaviour
{
    // インスペクタから設定できるようにするためのフィールド
    [SerializeField]
    GameObject[] stoneObjectsToHide; // 非表示にしたい石のゲームオブジェクト

    /// <summary>
    /// StoneObjectToHideに設定されたゲームオブジェクトを非表示にします。
    /// このメソッドはアニメーションイベントから呼び出されます。
    /// </summary>
    public void HideStone()
    {
        foreach (var stoneObject in stoneObjectsToHide)
            // stoneObjectToHideが設定されているかチェック
            if (stoneObjectsToHide != null)
            {
                // オブジェクトを非アクティブにする
                stoneObject.SetActive(false);
                Debug.Log($"{stoneObject.name} を非表示にしました。");
            }
            else
            {
                // 設定されていない場合はエラーログを出力
                Debug.LogError("非表示にする石のオブジェクトが設定されていません！インスペクタを確認してください。", this);
            }
    }
}
