using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 画面のフェードイン・フェードアウトを制御するクラス。
/// Animatorを使用してアニメーションを再生します。
/// </summary>
public class ScreenFader : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private static readonly int FadeInTrigger = Animator.StringToHash("FadeIn");
    private static readonly int FadeOutTrigger = Animator.StringToHash("FadeOut");

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    /// <summary>
    /// フェードアウト（画面を暗くする）アニメーションを開始します。
    /// </summary>
    public IEnumerator FadeOut(float duration)
    {
        if (animator != null)
        {
            animator.SetTrigger(FadeOutTrigger);
        }
        yield return new WaitForSeconds(duration);
    }

    /// <summary>
    /// フェードイン（画面を見えるようにする）アニメーションを開始します。
    /// </summary>
    public IEnumerator FadeIn(float duration)
    {
        if (animator != null)
        {
            animator.SetTrigger(FadeInTrigger);
        }
        yield return new WaitForSeconds(duration);
    }
}
