using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 呪文の詳細説明パネル内の個々の項目（アイコンと説明文）を表示するコンポーネント。
/// </summary>
public class SpellDescriptionDetailUI : MonoBehaviour
{
    [Tooltip("項目アイコンを表示するImageコンポーネント")]
    [SerializeField] private Image iconImage;//

    [Tooltip("項目説明文を表示するTextMeshProUGUIコンポーネント")]
    [SerializeField] private TextMeshProUGUI descriptionText;

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
            descriptionText.text = item.descriptionText;
        }
    }
}