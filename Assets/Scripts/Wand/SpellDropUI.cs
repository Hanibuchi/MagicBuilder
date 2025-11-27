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

    public void SetData(SpellBase data)
    {
        if (iconImage != null && data.icon != null)
        {
            iconImage.sprite = data.icon;
        }
        else
        {
            Debug.LogWarning("Icon image or data icon is missing.");
        }
    }
}