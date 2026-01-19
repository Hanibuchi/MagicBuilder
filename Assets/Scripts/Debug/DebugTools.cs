using UnityEngine;

/// <summary>
/// 呪文関連のデバッグ機能をまとめたクラス。
/// インスペクターから操作することを想定しています。
/// </summary>
public class DebugTools : MonoBehaviour
{
    /// <summary>
    /// すべての呪文を開放し、1つずつ所持した状態にします。
    /// </summary>
    public void UnlockAndGrantAllSpells()
    {
        if (SpellHoldInfoManager.Instance != null)
        {
            SpellHoldInfoManager.Instance.Test_UnlockAndGrantAllSpells();
        }
        else
        {
            Debug.LogError("SpellHoldInfoManager.Instance が見つかりません。");
        }
    }


    public void UnlockAllWands()
    {
        WandUnlockManager.Instance.UnlockAllWands();
    }


    /// <summary>
    /// 持ち込み呪文の容量を最大にします。
    /// </summary>
    public void MaximizeEquippedSpellCapacity()
    {
        if (EquippedSpellManager.Instance != null)
        {
            EquippedSpellManager.Instance.Test_SetMaxCapacity();
        }
        else
        {
            Debug.LogError("EquippedSpellManager.Instance が見つかりません。");
        }
    }
}
