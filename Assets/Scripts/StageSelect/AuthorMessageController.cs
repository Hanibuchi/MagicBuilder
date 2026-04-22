using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using TMPro;

public class AuthorMessageController : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeButton;
    
    [Header("Message Settings")]
    [SerializeField, TextArea(5, 10)] 
    private string authorMessage = "6-6クリアおめでとうございます！\n\nここまでプレイしていただき\n本当にありがとうございました！\n\n作者より";

    [Header("Sound Settings")]
    [SerializeField, Tooltip("メッセージ表示開始時に流すBGM")]
    private AudioClip bgmClip;
    [SerializeField, Tooltip("メッセージ表示開始時のSE")]
    private AudioClip openSeClip;
    [SerializeField, Tooltip("閉じるボタンを押した時のSE")]
    private AudioClip closeSeClip;
    [SerializeField, Tooltip("文字表示中のタイピングSE")]
    private AudioClip typingSeClip;
    [SerializeField, Tooltip("タイピングSEを鳴らす文字間隔")]
    private int typingSeInterval = 3;

    private bool isProcessed = false;
    private Action onClosedAction;
    private Coroutine textCoroutine;

    private void Start()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            StartCoroutine(FadeRoutine(1f));
        }
    }

    public void Initialize(Action onClosed = null)
    {
        if (SoundManager.Instance != null)
        {
            if (bgmClip != null) SoundManager.Instance.PlayBGM(bgmClip);
            if (openSeClip != null) SoundManager.Instance.PlaySE(openSeClip);
        }

        if (messageText != null)
        {
            if (textCoroutine != null) StopCoroutine(textCoroutine);
            textCoroutine = StartCoroutine(TypeTextRoutine(authorMessage));
        }

        this.onClosedAction = onClosed;
        
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() =>
            {
                if (isProcessed) return;
                isProcessed = true;

                if (SoundManager.Instance != null && closeSeClip != null)
                {
                    SoundManager.Instance.PlaySE(closeSeClip);
                }

                Close();
            });
        }
    }

    private IEnumerator TypeTextRoutine(string message)
    {
        messageText.text = message;
        messageText.maxVisibleCharacters = 0;

        for (int i = 1; i <= message.Length; i++)
        {
            messageText.maxVisibleCharacters = i;
            
            if (typingSeClip != null && SoundManager.Instance != null && i % typingSeInterval == 0)
            {
                SoundManager.Instance.PlaySE(typingSeClip, 0.5f); // 少し音量を下げるなど調整可能
            }

            yield return null;
        }
    }

    public void Close()
    {
        if (canvasGroup != null)
        {
            StartCoroutine(FadeRoutine(0f, () =>
            {
                onClosedAction?.Invoke();
                Destroy(gameObject);
            }));
        }
        else
        {
            onClosedAction?.Invoke();
            Destroy(gameObject);
        }
    }

    private IEnumerator FadeRoutine(float targetAlpha, Action onComplete = null)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        onComplete?.Invoke();
    }
}