// BossAppearanceManager.cs
using UnityEngine;
using System;

/// <summary>
/// ボス出現時の演出を管理するシングルトンクラス。
/// </summary>
public class BossAppearanceManager : MonoBehaviour
{
    public static BossAppearanceManager Instance { get; private set; }

    [SerializeField] private Animator bossAppearanceAnimator;
    [SerializeField] private string animationTriggerName = "Play";

    [Header("音響設定")]
    [SerializeField] private AudioClip bossAppearanceSE;
    [SerializeField, Range(0f, 2f)] private float bossAppearanceSEVolume = 1f;

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

    /// <summary>
    /// ボス出現演出のアニメーションをトリガーします。
    /// </summary>
    public void PlayBossAppearanceAnimation()
    {
        Debug.Log("BossAppearanceManager: ボス出現演出再生！");
        if (bossAppearanceAnimator != null)
        {
            bossAppearanceAnimator.SetTrigger(animationTriggerName);
        }
    }

    /// <summary>
    /// アニメーションイベントから呼び出すためのSE再生メソッド。
    /// </summary>
    public void PlayAppearanceSE()
    {
        if (SoundManager.Instance != null && bossAppearanceSE != null)
        {
            SoundManager.Instance.PlaySE(bossAppearanceSE, bossAppearanceSEVolume);
        }
    }
}
