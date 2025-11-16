using UnityEngine;
using UnityEngine.EventSystems;
using System;

/// <summary>
/// 画面クリック（ポインターダウン）を感知し、登録されたメソッドを実行するシングルトンクラス。
/// Noitaのクリックトリガー呪文などの実装に利用できます。
/// </summary>
public class ClickTriggerInputReader : MonoBehaviour, IPointerDownHandler
{
    // 💡 シングルトンインスタンス
    private static ClickTriggerInputReader _instance;
    public static ClickTriggerInputReader Instance => _instance;

    // 💡 登録されたメソッドを保持するイベント
    // 引数も返り値も持たないメソッド（Action）を外部から登録できます
    private Action _onPointerDownAction;

    /// <summary>
    /// Awakeはオブジェクトの初期化時に呼び出されます。
    /// ここでシングルトンの設定を行います。
    /// </summary>
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            // 既にインスタンスが存在し、それが自分自身ではない場合は、新しいインスタンスを破棄します
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            // シーンを跨いでも破棄されないように設定（必要に応じて）
            // Don'tDestroyOnLoad(this.gameObject);
        }
    }

    /// <summary>
    /// 実行するメソッドを登録します。
    /// </summary>
    /// <param name="callback">登録するメソッド（引数なし、返り値なし）</param>
    public void RegisterCallback(Action callback)
    {
        _onPointerDownAction += callback;
    }

    /// <summary>
    /// 登録されたメソッドを解除します。
    /// </summary>
    /// <param name="callback">解除するメソッド（引数なし、返り値なし）</param>
    public void UnregisterCallback(Action callback)
    {
        _onPointerDownAction -= callback;
    }

    /// <summary>
    /// IPointerDownHandler インターフェースの実装。
    /// 画面がクリック（ポインターダウン）されたときにUnityによって自動で呼び出されます。
    /// </summary>
    /// <param name="eventData">イベントデータ</param>
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Pointer down detected.");
        _onPointerDownAction?.Invoke();
        _onPointerDownAction = null;
    }
}