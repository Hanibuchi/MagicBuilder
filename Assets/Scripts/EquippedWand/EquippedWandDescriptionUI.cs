using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 装備中の杖の情報を表示するUI。
/// 杖の画像、名称、説明、および固定呪文を表示します。
/// </summary>
public class EquippedWandDescriptionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField, Tooltip("杖の見た目を表示するImage")]
    private Image wandImage;
    [SerializeField, Tooltip("杖の名前を表示するテキスト")]
    private TextMeshProUGUI nameText;
    [SerializeField, Tooltip("杖の説明を表示するテキスト")]
    private TextMeshProUGUI descriptionText;
    [SerializeField, Tooltip("固定呪文アイコンを配置する親要素")]
    private Transform fixedSpellsContainer;
    [SerializeField, Tooltip("固定呪文の表示に使用するアイコンプレハブ")]
    private SimpleSpellUI simpleSpellUIPrefab;

    /// <summary>
    /// 杖の表示内容を更新します。
    /// </summary>
    public void SetData(Wand wand)
    {
        if (wand == null)
        {
            ClearFixedSpellIcons();
            if (wandImage != null)
            {
                wandImage.sprite = null;
            }
            if (nameText != null)
            {
                nameText.text = string.Empty;
            }
            if (descriptionText != null)
            {
                descriptionText.text = string.Empty;
            }
            return;
        }

        if (wandImage != null)
        {
            wandImage.sprite = wand.wandSprite;
        }

        if (nameText != null)
        {
            nameText.text = wand.wandName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = wand.description;
        }

        UpdateFixedSpellIcons(wand.fixedSpells);
    }

    private void UpdateFixedSpellIcons(System.Collections.Generic.List<SpellBase> fixedSpells)
    {
        ClearFixedSpellIcons();

        if (fixedSpellsContainer == null || simpleSpellUIPrefab == null || fixedSpells == null)
        {
            return;
        }

        foreach (var fixedSpell in fixedSpells)
        {
            if (fixedSpell == null)
            {
                continue;
            }

            SimpleSpellUI spellUI = Instantiate(simpleSpellUIPrefab, fixedSpellsContainer);
            spellUI.transform.localScale = Vector3.one * 0.5f;
            spellUI.SetData(fixedSpell);
        }
    }

    private void ClearFixedSpellIcons()
    {
        if (fixedSpellsContainer == null)
        {
            return;
        }

        foreach (Transform child in fixedSpellsContainer)
        {
            Destroy(child.gameObject);
        }
    }

    [SerializeField]
    private Wand testWand;

    [ContextMenu("Test Set Data")]
    private void TestSetData()
    {
        SetData(testWand);
    }
}