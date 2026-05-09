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
    [ContextMenu("すべての呪文を開放・所持")]
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

    [ContextMenu("すべての杖を開放")]
    public void UnlockAllWands()
    {
        WandUnlockManager.Instance.UnlockAllWands();
    }


    /// <summary>
    /// すべてのステージを開放します。
    /// </summary>
    [ContextMenu("すべてのステージを開放")]
    public void UnlockAllStage()
    {
        if (StageUnlockManager.Instance != null)
        {
            StageUnlockManager.Instance.UnlockAllStages();
        }
        else
        {
            Debug.LogError("StageUnlockManager.Instance が見つかりません。");
        }
    }


    /// <summary>
    /// 持ち込み呪文の容量を最大にします。
    /// </summary>
    [ContextMenu("持ち込み呪文の容量を最大化")]
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

    /// <summary>
    /// 所持金を最大にします。
    /// </summary>
    [ContextMenu("所持金を最大化")]
    public void MaximizeCurrency()
    {
        if (CurrencyController.Instance != null)
        {
            CurrencyController.Instance.Test_SetMaxCurrency();
        }
        else
        {
            Debug.LogError("CurrencyController.Instance が見つかりません。");
        }
    }

    /// <summary>
    /// すべてのデバッグメソッドを実行します。
    /// </summary>
    [ContextMenu("すべてのデバッグメソッドを実行")]
    public void ExecuteAllDebugMethods()
    {
        UnlockAndGrantAllSpells();
        UnlockAllWands();
        UnlockAllStage();
        MaximizeEquippedSpellCapacity();
        MaximizeCurrency();
    }

    /// <summary>
    /// PlayerPrefsのデータをすべて削除します。
    /// </summary>
    [ContextMenu("PlayerPrefsデータをクリア")]
    public void ClearPlayerPrefs()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearPlayerPrefs();
        }
        else
        {
            Debug.LogError("GameManager.Instance が見つかりません。");
        }
    }
}

