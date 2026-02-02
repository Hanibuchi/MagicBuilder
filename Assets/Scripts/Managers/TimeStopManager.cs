using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// プロジェクト全体の時間停止（タイムスケール）を集中管理するクラス。
/// </summary>
public class TimeStopManager : MonoBehaviour
{
    private static TimeStopManager _instance;
    public static TimeStopManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // シーン内に存在するか確認
                _instance = FindFirstObjectByType<TimeStopManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("TimeStopManager");
                    _instance = go.AddComponent<TimeStopManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private Dictionary<object, float> _timeScaleRequests = new Dictionary<object, float>();
    private float _defaultTimeScale = 1f;

    /// <summary>
    /// 時間停止またはスローモーションをリクエストします。
    /// 同時に複数のリクエストがある場合、最も低いタイムスケールが適用されます。
    /// </summary>
    /// <param name="source">リクエスト元のオブジェクト（解除時に必要）</param>
    /// <param name="timeScale">適用したいタイムスケール</param>
    public void RequestTimeStop(object source, float timeScale = 0f)
    {
        if (source == null) return;

        if (_timeScaleRequests.ContainsKey(source))
        {
            _timeScaleRequests[source] = timeScale;
        }
        else
        {
            _timeScaleRequests.Add(source, timeScale);
        }

        UpdateTimeScale();
    }

    /// <summary>
    /// 指定したソースからの時間停止リクエストを解除します。
    /// </summary>
    /// <param name="source">リクエスト時に使用したオブジェクト</param>
    public void ReleaseTimeStop(object source)
    {
        if (source == null) return;

        if (_timeScaleRequests.Remove(source))
        {
            UpdateTimeScale();
        }
    }

    private void UpdateTimeScale()
    {
        if (_timeScaleRequests.Count > 0)
        {
            // 最小のタイムスケールを採用する
            float minScale = _timeScaleRequests.Values.Min();
            Time.timeScale = minScale;
        }
        else
        {
            Time.timeScale = _defaultTimeScale;
        }
    }

    /// <summary>
    /// 一定時間だけタイムスケールを変更（ヒットストップなど）します。
    /// </summary>
    public void PlayHitStop(float duration, float timeScale = 0f)
    {
        if (duration <= 0) return;
        StartCoroutine(DoHitStop(duration, timeScale));
    }

    private IEnumerator DoHitStop(float duration, float timeScale)
    {
        object requestSource = new object();
        RequestTimeStop(requestSource, timeScale);

        yield return new WaitForSecondsRealtime(duration);

        ReleaseTimeStop(requestSource);
    }

    /// <summary>
    /// 全てのリクエストを強制解除します（シーン遷移時など）
    /// </summary>
    public void ResetAllRequests()
    {
        _timeScaleRequests.Clear();
        Time.timeScale = _defaultTimeScale;
    }
}
