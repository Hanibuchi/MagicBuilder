using UnityEngine;
using UnityEngine.Audio; // Audio Mixerを使うために必要

public class SoundManager : MonoBehaviour
{
    // 外部からアクセスするための静的インスタンス
    public static SoundManager Instance { get; private set; }

    // 1. Audio Mixerへの参照
    [SerializeField] private AudioMixer mixer;

    // 2. BGMとSEのAudioSourceをアタッチ
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;

    const string BGM_VOLUME_PARAM = "BGM_Volume";
    const string SE_VOLUME_PARAM = "SE_Volume";

    // シングルトンの初期化処理
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- BGM再生メソッド ---
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // --- SE再生メソッド ---
    public void PlaySE(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;

        // PlayOneShotの第2引数としてvolumeScaleを渡す
        seSource.PlayOneShot(clip, volumeScale);
    }

    // --- 音量調整メソッド (Audio Mixer経由) ---
    public void SetBGMVolume0to1(float volume)
    {
        SetVolume0to1(BGM_VOLUME_PARAM, volume);
    }
    public void SetSEVolume0to1(float volume)
    {
        SetVolume0to1(SE_VOLUME_PARAM, volume);
    }
    void SetVolume0to1(string parameterName, float volume)
    {
        float db = volume > 0 ? 20f * Mathf.Log10(volume) : -80f;
        SetVolume(parameterName, db);
    }

    public void StopBGMWithFade(float duration = 1.0f)
    {
        // BGMのAudioSourceが再生中でない場合は何もしない
        if (!bgmSource.isPlaying) return;

        // 既にフェードアウトのコルーチンが動いていたら、一度停止する
        StopCoroutine(nameof(FadeOutBGM));

        // フェードアウトのコルーチンを開始
        StartCoroutine(FadeOutBGM(duration));
    }

    float startVolume_db; // 開始時のdB値

    // BGMをフェードアウトさせるコルーチン
    private System.Collections.IEnumerator FadeOutBGM(float duration)
    {
        float currentTime = 0;
        float endVolume_db = -80f; // 終了時のdB値（ほぼ無音）

        // Audio Mixerから現在のボリューム値（dB）を取得
        mixer.GetFloat(BGM_VOLUME_PARAM, out startVolume_db);
        Debug.Log("Start Volume (dB): " + startVolume_db);

        while (currentTime < duration)
        {
            // 経過時間 / フェード時間で0.0〜1.0の割合を計算
            currentTime += Time.deltaTime;
            float t = currentTime / duration;

            // dB値を線形補間（Lerp）
            float currentVolume_db = Mathf.Lerp(startVolume_db, endVolume_db, t);

            // Audio Mixerに設定
            mixer.SetFloat(BGM_VOLUME_PARAM, currentVolume_db);

            yield return null; // 1フレーム待つ
        }

        // フェード完了後、念のためAudio Mixerの音量を最低に設定し、AudioSourceを停止
        mixer.SetFloat(BGM_VOLUME_PARAM, endVolume_db);
        bgmSource.Stop();

        SetVolume(BGM_VOLUME_PARAM, startVolume_db);
    }

    // 実際のミキサー操作ロジック
    private void SetVolume(string parameterName, float volume)
    {
        mixer.SetFloat(parameterName, volume);
    }
}