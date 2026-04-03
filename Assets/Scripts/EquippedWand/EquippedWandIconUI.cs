using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 装備中の杖の見た目のみを表示するUI。
/// </summary>
public class EquippedWandIconUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField, Tooltip("杖の見た目を表示するImage")]
    private Image wandImage;

    /// <summary>
    /// 杖の表示内容を更新します。
    /// </summary>
    public void SetData(Wand wand)
    {
        if (wandImage == null)
        {
            return;
        }

        if (wand == null)
        {
            wandImage.sprite = null;
            return;
        }

        wandImage.sprite = wand.wandSprite;
    }

    [SerializeField]
    private Wand testWand;

    [ContextMenu("Test Set Data")]
    private void TestSetData()
    {
        SetData(testWand);
    }
}