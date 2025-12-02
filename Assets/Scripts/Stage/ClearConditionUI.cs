using UnityEngine;
using UnityEngine.UI; // UI要素を扱うために必要
using System.Collections;
using System;
using UnityEngine.EventSystems; // コルーチンを使うために必要

public class ClearConditionUI : MonoBehaviour, IPointerClickHandler
{
    // オブジェクトが破棄されるときに実行される Action
    private Action onClosedAction;
    /// <summary>
    /// 生成時と破棄時に実行する処理を設定するメソッド。
    /// </summary>
    /// <param name="onGenerated">生成時 (Awake) に実行したい Action。</param>
    /// <param name="onClosed">破棄時 (OnDestroy) に実行したい Action。</param>
    public void SetAction(Action onClosed)
    {
        // 渡された Action をプライベート変数に格納
        onClosedAction = onClosed;
    }

    // アニメーションが完了した後にクリックを有効にするかどうか
    private bool isReadyToStartGame = false;

    /// <summary>
    /// ゲーム開始の準備ができた状態にする。
    /// このメソッドは、アニメーションの**最後**で呼び出されるように、
    /// **チェックマークアニメーションのアニメーションイベント**として設定するのが最も確実です。
    /// </summary>
    public void ReadyToStartGame()
    {
        isReadyToStartGame = true;
        // 例として、デバッグログを表示
        Debug.Log("クリア条件UI: クリックによるゲーム開始準備完了。");
    }

    const string GAME_START_TRIGGER = "GameStart";
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isReadyToStartGame)
        {
            GetComponent<Animator>().SetTrigger(GAME_START_TRIGGER);
        }
    }

    /// <summary>
    /// このUIオブジェクトをシーンから削除する。
    /// </summary>
    public void DestroySelf()
    {
        onClosedAction?.Invoke();
        Destroy(gameObject);
        Debug.Log("クリア条件UIが削除されました。");
    }
}