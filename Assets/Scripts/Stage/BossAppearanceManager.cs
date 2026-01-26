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
    /// ボス出現演出のアニメーションを再生します。
    /// </summary>
    public void PlayBossAppearanceAnimation()
    {
        Debug.Log("BossAppearanceManager: ボス出現演出再生！");
        if (bossAppearanceAnimator != null)
        {
            bossAppearanceAnimator.SetTrigger(animationTriggerName);
        }
    }
}
