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

    // シングルトンの初期化処理
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
    public void SetBGMVolume(float volume)
    {
        SetVolume("BGM_Volume", volume);
    }
    public void SetSEVolume(float volume)
    {
        SetVolume("SE_Volume", volume);
    }

    // 実際のミキサー操作ロジック
    private void SetVolume(string parameterName, float volume)
    {
        // 0.0〜1.0のスライダー値を-80dB〜0dBに変換して設定
        float db = volume > 0 ? 20f * Mathf.Log10(volume) : -80f;
        mixer.SetFloat(parameterName, db);
    }
}