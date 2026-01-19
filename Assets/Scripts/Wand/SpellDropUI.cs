// ファイル名: SpellDropper.cs (新規)

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

/// <summary>
/// ドロップされた呪文UI。
/// </summary>
public class SpellDropUI : MonoBehaviour
{
    // UIパーツ
    public Image iconImage;
    [SerializeField] private Image frameImage;

    public void SetData(SpellBase data)
    {
        if (iconImage != null && data != null && data.icon != null)
        {
            iconImage.sprite = data.icon;
        }
        else if (data != null)
        {
            Debug.LogWarning("Icon image or data icon is missing.");
        }

        if (data != null)
        {
            SetColor(SpellCommonData.Instance.GetCategoryColor(data.category));
        }
    }

    /// <summary>
    /// ベースの色を設定します。
    /// </summary>
    /// <param name="color">設定する色</param>
    public void SetColor(Color color)
    {
        if (frameImage != null)
        {
            frameImage.color = color;
        }
    }
}