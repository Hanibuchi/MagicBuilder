using UnityEngine;
using System.Collections; // Coroutineを使うために必要

public class SimpleAudioPlayer : MonoBehaviour
{
    // === 公開変数 ===
    [Tooltip("再生したいオーディオクリップを設定します。")]
    public AudioClip audioClipToPlay;

    [Tooltip("再生を遅延させる時間（秒）を設定します。")]
    public float delayInSeconds = 0f; // デフォルトを3秒に設定
    [Tooltip("オーディオの音量を設定します。")]
    [Range(0f, 1f)]
    public float volume = 1.0f;

    // === 初期化処理 ===
    void Start()
    {
        // 遅延再生を行うコルーチンを開始します
        StartCoroutine(PlayAudioAfterDelay(delayInSeconds));
    }

    // === コルーチン関数 ===
    /// <summary>
    /// 指定された遅延時間の後にオーディオクリップを再生します。
    /// </summary>
    /// <param name="delay">再生を遅延させる時間（秒）</param>
    /// <returns>コルーチン用のIEnumerator</returns>
    private IEnumerator PlayAudioAfterDelay(float delay)
    {
        // 指定された秒数待機します
        yield return new WaitForSeconds(delay);

        // クリップが設定されているか確認します
        if (SoundManager.Instance != null && audioClipToPlay != null)
        {
            // PlayClipAtPointを使って、指定座標でオーディオを再生します
            // この関数は、再生のための一時的なGameObjectを作成し、再生後に破棄します。
            SoundManager.Instance.PlaySE(audioClipToPlay, volume);
        }
        else
        {
            Debug.LogWarning("DelayedAudioPlayer: audioClipToPlayが設定されていません。オーディオは再生されませんでした。");
        }
    }
}