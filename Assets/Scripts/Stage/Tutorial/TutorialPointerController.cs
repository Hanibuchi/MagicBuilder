using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro; // TextMeshProを使用していると仮定

/// <summary>
/// チュートリアルの指差し指示や説明テキストを制御するクラス
/// Canvasを持つオブジェクトにアタッチして使用する想定です
/// </summary>
[RequireComponent(typeof(Canvas))]
public class TutorialPointerController : MonoBehaviour
{
    [Header("Pointer Settings")]
    [Tooltip("指マークのアニメーター")]
    [SerializeField] private Animator pointerAnimator;
    [Tooltip("指マークのRectTransform")]
    [SerializeField] private RectTransform pointerRectTransform;

    [Header("Description Settings")]
    [Tooltip("説明を表示するためのテキスト")]
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Animation Settings")]
    [Tooltip("ドラッグにかかる時間（秒）")]
    [SerializeField] private float dragDuration = 1.0f;
    [Tooltip("ドラッグ時に押す・離すアニメーションを待つ時間（秒）")]
    [SerializeField] private float dragAnimationWaitTime = 1f;

    private Coroutine currentAnimationCoroutine;
    private Coroutine currentTextCoroutine;

    // Animatorのパラメーター名のハッシュ値（パフォーマンス対策）
    private readonly int tapTriggerHash = Animator.StringToHash("Tap");
    private readonly int dragStartTriggerHash = Animator.StringToHash("DragStart");
    private readonly int dragEndTriggerHash = Animator.StringToHash("DragEnd");

    private void Awake()
    {
        // 初期状態では指マークとテキストを非表示にする
        HidePointer();
        HideDescription();
    }

    /// <summary>
    /// 説明テキストを毎フレーム1文字ずつ表示します。
    /// </summary>
    /// <param name="text">表示する文字列</param>
    public void ShowDescription(string text)
    {
        if (descriptionText != null)
        {
            if (currentTextCoroutine != null)
            {
                StopCoroutine(currentTextCoroutine);
            }

            descriptionText.gameObject.SetActive(true);
            currentTextCoroutine = StartCoroutine(ShowTextRoutine(text));
        }
        else
        {
            Debug.LogWarning("Description Text is not assigned in the inspector.");
        }
    }

    private IEnumerator ShowTextRoutine(string text)
    {
        descriptionText.text = "";
        for (int i = 0; i < text.Length; i++)
        {
            descriptionText.text += text[i];
            yield return null; // 1フレーム待機
        }
    }

    /// <summary>
    /// 説明テキストを非表示にします。
    /// </summary>
    public void HideDescription()
    {
        if (currentTextCoroutine != null)
        {
            StopCoroutine(currentTextCoroutine);
            currentTextCoroutine = null;
        }

        if (descriptionText != null)
        {
            descriptionText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 指マークを非表示＆アニメーションを停止します。
    /// </summary>
    public void HidePointer()
    {
        StopCurrentAnimation();
        if (pointerAnimator != null)
        {
            pointerAnimator.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 指定した画面座標でのタップアニメーションを再生します。
    /// </summary>
    /// <param name="screenPosition">タップ位置(スクリーン座標)</param>
    public void PlayTapAnimation(Vector2 screenPosition)
    {
        StopCurrentAnimation();
        currentAnimationCoroutine = StartCoroutine(TapRoutine(screenPosition));
    }

    /// <summary>
    /// 指定した開始位置から終了位置までのドラッグアニメーションを再生します。
    /// </summary>
    /// <param name="startScreenPos">ドラッグ開始位置(スクリーン座標)</param>
    /// <param name="endScreenPos">ドラッグ終了位置(スクリーン座標)</param>
    public void PlayDragAnimation(Vector2 startScreenPos, Vector2 endScreenPos)
    {
        StopCurrentAnimation();
        currentAnimationCoroutine = StartCoroutine(DragRoutine(startScreenPos, endScreenPos));
    }

    private void StopCurrentAnimation()
    {
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
            currentAnimationCoroutine = null;
        }
    }

    private IEnumerator TapRoutine(Vector2 screenPosition)
    {
        if (pointerAnimator == null || pointerRectTransform == null) yield break;

        pointerRectTransform.position = screenPosition;
        pointerAnimator.gameObject.SetActive(true);

        // タップ（押す・離す）アニメーションを開始
        pointerAnimator.SetTrigger(tapTriggerHash);

        yield return null;
    }

    private IEnumerator DragRoutine(Vector2 startPos, Vector2 endPos)
    {
        if (pointerAnimator == null || pointerRectTransform == null) yield break;

        pointerRectTransform.position = startPos;
        pointerAnimator.gameObject.SetActive(true);

        // ① 押す（ホールド）アニメーション
        pointerAnimator.SetTrigger(dragStartTriggerHash);

        // 押した実感が出るまで少し待機
        yield return new WaitForSecondsRealtime(dragAnimationWaitTime);

        // ② ドラッグ（移動）
        float elapsedTime = 0f;
        while (elapsedTime < dragDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / dragDuration);

            // 滑らかに移動させるためのイージング処理 (SmoothStep)
            float easeT = t * t * (3f - 2f * t);

            pointerRectTransform.position = Vector2.Lerp(startPos, endPos, easeT);
            yield return null;
        }

        pointerRectTransform.position = endPos;

        // ③ ドロップ（離す）アニメーション
        pointerAnimator.SetTrigger(dragEndTriggerHash);

        // 離した後の余韻で少し待機
        yield return new WaitForSecondsRealtime(dragAnimationWaitTime);
    }
}