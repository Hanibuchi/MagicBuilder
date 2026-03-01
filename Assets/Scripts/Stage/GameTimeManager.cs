using UnityEngine;

/// <summary>
/// ゲームの経過時間を管理するシングルトンクラス。
/// </summary>
public class GameTimerManager : MonoBehaviour
{
    // シングルトンのインスタンス
    private static GameTimerManager _instance;
    public static GameTimerManager Instance => _instance;

    // --- 内部状態 ---

    // タイマーが現在動いているか
    private bool _isRunning = false;

    // タイマーが一時停止しているか
    private bool _isPaused = false;

    // 経過時間 (秒)
    private float _elapsedTime = 0f;

    // 時間変更を通知するリスナー (一つのみ登録可能)
    private ITimeChangeListener _listener;

    // --- Unity ライフサイクル ---

    private void Awake()
    {
        // 既にインスタンスが存在し、それが自分自身でない場合は、自分を破棄して重複を防ぐ
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        // シーン切り替え時に破棄されないように設定
        // DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (_isRunning && !_isPaused)
        {
            // 経過時間を加算
            _elapsedTime += Time.deltaTime;

            // リスナーに通知
            _listener?.OnTimeChanged(_elapsedTime);
        }
    }

    // --- 外部から呼ばれるメソッド ---

    /// <summary>
    /// タイマーを**開始**します。既に動いている場合はリセットして開始します。
    /// </summary>
    public void StartTimer()
    {
        _elapsedTime = 0f;
        _isRunning = true;
        _isPaused = false;
        Debug.Log("Game Timer Started.");
        _listener?.OnTimeChanged(_elapsedTime); // 開始時の通知
    }

    /// <summary>
    /// タイマーを**一時停止**します。
    /// </summary>
    public void PauseTimer()
    {
        if (_isRunning)
        {
            _isPaused = true;
            Debug.Log("Game Timer Paused. Current Time: " + _elapsedTime.ToString("F2") + "s");
        }
    }

    /// <summary>
    /// 一時停止からタイマーを**再開**します。
    /// </summary>
    public void ResumeTimer()
    {
        if (_isRunning && _isPaused)
        {
            _isPaused = false;
            Debug.Log("Game Timer Resumed.");
        }
    }

    /// <summary>
    /// タイマーを**停止**します。
    /// </summary>
    /// <returns>停止時点での最終経過時間 (秒)。</returns>
    public float StopTimer()
    {
        _isRunning = false;
        _isPaused = false;
        float finalTime = _elapsedTime;
        Debug.Log("Game Timer Stopped. Final Time: " + finalTime.ToString("F2") + "s");
        return finalTime;
    }

    /// <summary>
    /// 現在の経過時間を取得します。
    /// </summary>
    /// <returns>現在の経過時間 (秒)。</returns>
    public float GetElapsedTime()
    {
        return _elapsedTime;
    }

    /// <summary>
    /// 時間変更の通知を受け取るリスナーを登録します。
    /// </summary>
    /// <param name="listener">登録する ITimeChangeListener のインスタンス。</param>
    public void RegisterListener(ITimeChangeListener listener)
    {
        _listener = listener;
        Debug.Log("Time Change Listener Registered.");
    }

    /// <summary>
    /// 登録されているリスナーを解除します。
    /// </summary>
    public void UnregisterListener()
    {
        _listener = null;
        Debug.Log("Time Change Listener Unregistered.");
    }
}

/// <summary>
/// タイマーの時間変更を監視するためのインターフェース。
/// </summary>
public interface ITimeChangeListener
{
    /// <summary>
    /// タイマーの値が変更されたときに呼び出されます。
    /// </summary>
    /// <param name="newTime">現在の経過時間 (秒)。</param>
    void OnTimeChanged(float newTime);
}