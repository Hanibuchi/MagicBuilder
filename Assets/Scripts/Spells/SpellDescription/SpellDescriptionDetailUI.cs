using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 呪文の詳細説明パネル内の個々の項目（アイコンと説明文）を表示するコンポーネント。
/// </summary>
public class SpellDescriptionDetailUI : MonoBehaviour
{
    [Tooltip("項目アイコンを表示するImageコンポーネント")]
    [SerializeField] private Image iconImage;//

    [Tooltip("項目説明文を表示するTextMeshProUGUIコンポーネント")]
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Tooltip("テキストの表示を開始するまでの遅延時間（秒）")]
    [SerializeField] private float typingStartDelay = 0.2f;

    private Coroutine textCoroutine;

    /// <summary>
    /// 表示するデータを設定します。
    /// </summary>
    /// <param name="item">表示するSpellDescriptionItemデータ。</param>
    public void SetData(SpellDescriptionItem item)
    {
        if (item == null)
        {
            Debug.LogError("渡された SpellDescriptionItem が null です。");
            return;
        }

        // アイコンを設定
        if (iconImage != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = item.icon != null; // アイコンがない場合は非表示にする
        }
        
        // 説明文を設定
        if (descriptionText != null)
        {
            if (textCoroutine != null) StopCoroutine(textCoroutine);
            textCoroutine = StartCoroutine(TypeTextRoutine(item.descriptionText));
        }
    }

    private IEnumerator TypeTextRoutine(string message)
    {
        descriptionText.text = message;
        descriptionText.maxVisibleCharacters = 0;

        if (typingStartDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(typingStartDelay);
        }

        for (int i = 1; i <= message.Length; i++)
        {
            descriptionText.maxVisibleCharacters = i;
            yield return null;
        }
    }
}