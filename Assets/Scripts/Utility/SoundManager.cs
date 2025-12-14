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

    // AudioMixer用
    const string BGM_VOLUME_PARAM = "BGM_Volume";
    const string SE_VOLUME_PARAM = "SE_Volume";
    // PlayerPrefs用
    const string BGM_VOLUME_SAVE_KEY = "Save_BGM_Volume_0to1";
    const string SE_VOLUME_SAVE_KEY = "Save_SE_Volume_0to1";

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

    void Start()
    {
        LoadVolumes();
    }

    /// <summary>
    /// PlayerPrefsから音量設定をロードし、Audio Mixerに適用する
    /// </summary>
    private void LoadVolumes()
    {
        // BGM音量のロード (デフォルトは1.0)
        float loadedBGMVolume = GetBGMVolume0to1();
        SetBGMVolume0to1(loadedBGMVolume);

        // SE音量のロード (デフォルトは1.0)
        float loadedSEVolume = GetSEVolume0to1();
        SetSEVolume0to1(loadedSEVolume);
    }

    // --- BGM再生メソッド ---
    /// <summary>
    /// BGMを再生します。
    /// </summary>
    /// <param name="clip">再生するオーディオクリップ</param>
    /// <param name="startTime">再生を開始する秒数 (デフォルトは0秒)</param>
    public void PlayBGM(AudioClip clip, float startTime = 0f)
    {
        if (clip == null) return;

        // 既存のクリップを設定
        bgmSource.clip = clip;
        bgmSource.loop = true;

        // 新しい引数startTimeを適用
        // AudioSource.timeに秒数を設定することで、その位置から再生が始まります。
        if (startTime >= 0f && startTime < clip.length)
        {
            bgmSource.time = startTime;
        }
        else
        {
            // startTimeがクリップの長さを超えている、または負の値の場合は、最初から再生 (または何もしない)
            bgmSource.time = 0f;
        }

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
        PlayerPrefs.SetFloat(BGM_VOLUME_SAVE_KEY, volume);
        PlayerPrefs.Save();
    }
    public void SetSEVolume0to1(float volume)
    {
        SetVolume0to1(SE_VOLUME_PARAM, volume);
        PlayerPrefs.SetFloat(SE_VOLUME_SAVE_KEY, volume);
        PlayerPrefs.Save();
    }
    void SetVolume0to1(string parameterName, float volume)
    {
        float db = volume > 0 ? 20f * Mathf.Log10(volume) : -80f;
        SetVolume(parameterName, db);
    }

    // --- 音量取得メソッド (Audio Mixer経由) ---
    public float GetBGMVolume0to1()
    {
        return PlayerPrefs.GetFloat(BGM_VOLUME_SAVE_KEY, 1.0f);
    }

    public float GetSEVolume0to1()
    {
        return PlayerPrefs.GetFloat(SE_VOLUME_SAVE_KEY, 1.0f);
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