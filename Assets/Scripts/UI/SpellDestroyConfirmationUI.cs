using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using TMPro;

public class SpellDestroyConfirmationUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    
    [Header("Spell Icon Configuration")]
    [SerializeField] private Transform spellIconContainer;
    [SerializeField] private SimpleSpellUI simpleSpellUIPrefab;

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

    public void Initialize(string message, SpellBase spellData, Action onYes, Action onNo, Action onClosed = null)
    {
        if (messageText != null)
        {
            if (textCoroutine != null) StopCoroutine(textCoroutine);
            textCoroutine = StartCoroutine(TypeTextRoutine(message));
        }

        // 呪文のアイコンを生成
        if (spellData != null && simpleSpellUIPrefab != null && spellIconContainer != null)
        {
            ClearSpellIcon();
            SimpleSpellUI spellUI = Instantiate(simpleSpellUIPrefab, spellIconContainer);
            spellUI.SetData(spellData);
        }

        this.onClosedAction = onClosed;

        if (yesButton != null)
        {
            yesButton.onClick.RemoveAllListeners();
            yesButton.onClick.AddListener(() =>
            {
                if (isProcessed) return;
                isProcessed = true;
                onYes?.Invoke();
                Close();
            });
        }

        if (noButton != null)
        {
            noButton.onClick.RemoveAllListeners();
            noButton.onClick.AddListener(() =>
            {
                if (isProcessed) return;
                isProcessed = true;
                onNo?.Invoke();
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
            yield return null;
        }
    }

    private void ClearSpellIcon()
    {
        if (spellIconContainer == null) return;
        
        foreach (Transform child in spellIconContainer)
        {
            Destroy(child.gameObject);
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
        if (canvasGroup == null)
        {
            onComplete?.Invoke();
            yield break;
        }

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
