using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 画面上の全てのWandUIインスタンスを管理し、グローバルなUIイベントをブロードキャストします。
/// </summary>
public class WandUIManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static WandUIManager Instance { get; private set; }

    private List<WandUI> activeWandUIs = new List<WandUI>();

    [Header("UIとコンポーネントのプレハブ")] // ★ 追加: WandsControllerから移動
    [Tooltip("WandUIとWandControllerを保持するオブジェクトのプレハブ")] // ★ 追加
    [SerializeField] private GameObject wandUIPrefab; // ★ 追加

    [Tooltip("WandUIを配置する親のTransform")] // ★ 追加
    [SerializeField] private Transform wandUIParent; // ★ 追加

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // シーンをまたいで保持する場合はDontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// WandUIのコンテナとWandUIのインスタンスを生成し、WandUIを返します。
    /// </summary>
    /// <returns>生成されたWandUIコンポーネント。失敗した場合はnull。</returns>
    public WandUI CreateWandUIInstance() // ★ 新規追加
    {
        if (wandUIPrefab == null)
        {
            Debug.LogError("WandUI Prefabが設定されていません。WandUIManagerを確認してください。");
            return null;
        }

        // 1. インスタンスの生成（WandsControllerから移動）
        GameObject container = Instantiate(wandUIPrefab, wandUIParent);
        WandUI wandUI = container.GetComponentInChildren<WandUI>();

        if (wandUI == null)
        {
            Debug.LogError("wandUIPrefabの子オブジェクトからWandUIコンポーネントが見つかりませんでした。");
            Destroy(container);
            return null;
        }
        RegisterWandUI(wandUI);

        return wandUI;
    }

    /// <summary>
    /// WandUIが生成されたときに自身を登録します。
    /// </summary>
    public void RegisterWandUI(WandUI wandUI)
    {
        if (!activeWandUIs.Contains(wandUI))
        {
            activeWandUIs.Add(wandUI);
            Debug.Log($"WandUI登録完了。現在 {activeWandUIs.Count} 個のUIがアクティブです。");
        }
    }

    /// <summary>
    /// WandUIが破棄されたときに登録を解除します。
    /// </summary>
    public void UnregisterWandUI(WandUI wandUI)
    {
        activeWandUIs.Remove(wandUI);
    }

    /// <summary>
    /// 全てのWandUIに、SpellUIのドラッグが開始されたことを通知します。
    /// </summary>
    public void NotifySpellDragBeganToAll()
    {
        foreach (var wandUI in activeWandUIs)
        {
            wandUI.NotifySpellDragBegan();
        }
    }

    /// <summary>
    /// 全てのWandUIに、SpellUIのドラッグが終了したことを通知します。
    /// </summary>
    public void NotifySpellDragEndedToAll()
    {
        foreach (var wandUI in activeWandUIs)
        {
            wandUI.NotifySpellDragEnded();
        }
    }
}