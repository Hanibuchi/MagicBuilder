using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 呪文のアイコン表示と、クリック時の詳細説明表示のみを行うシンプルなUIクラス
/// </summary>
public class SimpleSpellUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image frameImage;
    
    private SpellBase spellData;

    public void SetData(SpellBase data)
    {
        this.spellData = data;
        
        if (data == null) return;

        if (iconImage != null)
        {
            if (data.icon != null) iconImage.sprite = data.icon;
            iconImage.color = data.iconColor;
            iconImage.material = data.iconMaterial;
        }

        if (frameImage != null && SpellCommonData.Instance != null)
        {
            frameImage.color = SpellCommonData.Instance.GetCategoryColor(data.category);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"SimpleSpellUI clicked: {spellData?.spellName ?? "null"}");
        if (spellData == null) return;

        // ドラッグ中は判定しない
        if (eventData.dragging) return;

        if (SpellDescriptionUI.Instance != null)
        {
            Debug.Log($"SimpleSpellUI clicked: {spellData.spellName}");
            SpellDescriptionUI.Instance.StartShowAnimation(spellData);
        }
    }
}
