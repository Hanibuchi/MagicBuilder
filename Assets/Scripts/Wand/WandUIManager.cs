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
    /// 指定されたインデックスのWandUIをアクティブにし、それ以外を非アクティブにします。
    /// </summary>
    /// <param name="index">アクティブにするWandUIのインデックス。</param>
    public void SetActiveWandUI(int index) // ★ 新規追加
    {
        if (activeWandUIs.Count == 0)
        {
            Debug.LogWarning("登録されているWandUIがありません。");
            return;
        }

        if (index < 0 || index >= activeWandUIs.Count)
        {
            Debug.LogError($"指定されたインデックス {index} は無効です。有効範囲: 0 から {activeWandUIs.Count - 1} まで。");
            return;
        }

        // 全てのWandUIを走査し、アクティブ/非アクティブを設定
        for (int i = 0; i < activeWandUIs.Count; i++)
        {
            bool isActive = (i == index);
            activeWandUIs[i].gameObject.SetActive(isActive); // 親のGameObjectを制御
        }

        Debug.Log($"WandUIをインデックス {index} のものに切り替えました。");
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
        // ★ 追加: 新しく生成・登録されたWandUIはデフォルトで非アクティブにする
        wandUI.gameObject.SetActive(false);
        RegisterWandUI(wandUI);

        return wandUI;
    }

    /// <summary>
    /// WandUIが生成されたときに自身を登録します。
    /// </summary>
    void RegisterWandUI(WandUI wandUI)
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
    void UnregisterWandUI(WandUI wandUI)
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