using UnityEngine;
using System.Collections;

/// <summary>
/// ヒット時の時間停止（ヒットストップ）を管理するクラス。
/// </summary>
public class HitStopManager : MonoBehaviour
{
    private static HitStopManager _instance;
    public static HitStopManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("HitStopManager");
                _instance = go.AddComponent<HitStopManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private Coroutine _hitStopCoroutine;
    private float _originalTimeScale = 1f;
    private bool _isHitStopping = false;

    /// <summary>
    /// ヒットストップを実行します。
    /// </summary>
    /// <param name="duration">停止する時間（秒）</param>
    /// <param name="timeScale">停止中のタイムスケール（0で完全停止）</param>
    public void PlayHitStop(float duration, float timeScale = 0f)
    {
        if (duration <= 0) return;

        if (_isHitStopping)
        {
            StopCoroutine(_hitStopCoroutine);
        }
        else
        {
            _originalTimeScale = Time.timeScale;
        }

        _hitStopCoroutine = StartCoroutine(DoHitStop(duration, timeScale));
    }

    private IEnumerator DoHitStop(float duration, float timeScale)
    {
        _isHitStopping = true;
        Time.timeScale = timeScale;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = _originalTimeScale;
        _isHitStopping = false;
        _hitStopCoroutine = null;
    }
}
