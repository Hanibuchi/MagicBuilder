using UnityEngine;
using System;

public class SpellDestroyController : MonoBehaviour
{
    public static SpellDestroyController Instance { get; private set; }

    [SerializeField] private SpellDestroyConfirmationUI confirmationUIPrefab;
    private SpellDestroyConfirmationUI currentUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 呪文の破棄確認UIを表示します。
    /// </summary>
    /// <param name="spell">破棄する対象の呪文</param>
    public void RequestDestroySpell(SpellBase spell)
    {
        if (spell == null || currentUI != null) return;

        // 時間停止を要求
        TimeStopManager.Instance.RequestTimeStop(this, 0f);

        currentUI = Instantiate(confirmationUIPrefab);
        
        string message = $"以下の呪文を破棄しますか？";

        currentUI.Initialize(
            message,
            spell,
            onYes: () =>
            {
                // インベントリから削除
                if (SpellInventory.Instance != null)
                {
                    SpellInventory.Instance.RemoveSpellFromInventory(spell);
                }
            },
            onNo: () =>
            {
                // 何もしない
            },
            onClosed: () =>
            {
                // 時間停止を解除
                TimeStopManager.Instance.ReleaseTimeStop(this);
                currentUI = null;
            }
        );
    }

    [SerializeField] private SpellBase testSpell; // テスト用の呪文データ
    [ContextMenu("テスト: 呪文破棄UIを表示")]
    private void TestShowDestroyUI()
    {

        RequestDestroySpell(testSpell);
    }
}
