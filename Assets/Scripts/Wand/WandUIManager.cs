using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

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
            transform.root.gameObject.SetActive(false);
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

        UpdateWandSwitchButtons(index);
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
        WandUI wandUI = container.GetComponent<WandUI>();
        wandUI.transform.SetAsFirstSibling();

        if (wandUI == null)
        {
            Debug.LogError("wandUIPrefabの子オブジェクトからWandUIコンポーネントが見つかりませんでした。");
            Destroy(container);
            return null;
        }
        // ★ 追加: 新しく生成・登録されたWandUIはデフォルトで非アクティブにする
        wandUI.gameObject.SetActive(false);
        RegisterWandUI(wandUI);

        UpdateWandSwitchButtons(AttackManager.Instance.GetCurrentWandIndex());
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
        // ★ 追加: 登録された全てのハンドラにも通知
        foreach (var handler in dragHandlers)
        {
            handler.OnSpellDragBegan();
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
        // ★ 追加: 登録された全てのハンドラにも通知
        foreach (var handler in dragHandlers)
        {
            handler.OnSpellDragEnded();
        }
    }



    // ★ 追加: SpellDragHandlerのリスト
    private List<ISpellDragHandler> dragHandlers = new List<ISpellDragHandler>(); // ★ 新規追加
    /// <summary>
    /// SpellDragHandlerが生成されたときに自身を登録します。
    /// </summary>
    public void RegisterSpellDragHandler(ISpellDragHandler handler) // ★ 新規追加
    {
        if (!dragHandlers.Contains(handler))
        {
            dragHandlers.Add(handler);
            Debug.Log($"SpellDragHandler登録完了。現在 {dragHandlers.Count} 個のハンドラが登録されています。");
        }
    }
    /// <summary>
    /// SpellDragHandlerが破棄されたときに登録を解除します。
    /// </summary>
    public void UnregisterSpellDragHandler(ISpellDragHandler handler) // ★ 新規追加
    {
        dragHandlers.Remove(handler);
    }



    [Header("杖切り替えUI")] // ★ 追加
    [SerializeField] private Button switchNextButton; // ★ 追加: 次の杖へ
    [SerializeField] private Button switchPrevButton; // ★ 追加: 前の杖へ
    // ★ 杖切り替えリスナー
    private WandSwitchListener wandSwitchListener; // ★ 追加: WandsControllerが実装
    void Start() // ★ 追加: ボタンリスナーの設定とAttackManagerのイベント登録
    {
        // ボタンにクリックリスナーを登録
        switchNextButton?.onClick.AddListener(() => OnSwitchWandClicked(1));
        switchPrevButton?.onClick.AddListener(() => OnSwitchWandClicked(-1));
    }
    public void SetWandSwitchListener(WandSwitchListener listener)
    {
        wandSwitchListener = listener;
    }
    /// <summary>
    /// ボタンがクリックされたときにWandsControllerに切り替えを指示します。
    /// </summary>
    /// <param name="direction">切り替え方向 (1: 次, -1: 前)</param>
    private void OnSwitchWandClicked(int direction) // ★ 新規追加
    {
        var index = AttackManager.Instance.GetCurrentWandIndex() + direction;
        if (wandSwitchListener != null)
        {
            wandSwitchListener.SwitchWand(index);
        }
        else
        {
            Debug.LogWarning("WandSwitchListenerが設定されていません。杖切り替えが実行できません。");
        }
    }
    /// <summary>
    /// AttackManagerからの通知を受けて、杖切り替えボタンの表示を更新します。
    /// </summary>
    /// <param name="newIndex">新しく選択された杖のインデックス</param>
    private void UpdateWandSwitchButtons(int newIndex) // ★ 新規追加
    {
        int totalWands = activeWandUIs.Count;

        // 次へボタンの表示制御
        bool canSwitchNext = newIndex < totalWands - 1;
        switchNextButton?.gameObject.SetActive(canSwitchNext);

        // 前へボタンの表示制御
        bool canSwitchPrev = newIndex > 0;
        switchPrevButton?.gameObject.SetActive(canSwitchPrev);

        Debug.Log($"杖切り替えUIを更新: 現在のIndex={newIndex}, Next={canSwitchNext}, Prev={canSwitchPrev}");
    }

    public void Show()
    {
        transform.root.gameObject.SetActive(true);
    }
    const string HIDE_TRIGGER = "Hide";
    [SerializeField] Animator animator;
    public void Hide()
    {
        if (animator != null)
        {
            animator.SetTrigger(HIDE_TRIGGER);
        }
    }
}

/// <summary>
/// 杖切り替えイベントを受け取るためのインターフェース
/// </summary>
public interface WandSwitchListener
{
    /// <summary>
    /// 杖を一つ切り替えます（インデックスの更新）
    /// </summary>
    /// <param name="index">次の杖のindex</param>
    void SwitchWand(int index);
}

/// <summary>
/// SpellUIのドラッグ開始/終了イベントを受け取るためのインターフェース。
/// </summary>
public interface ISpellDragHandler
{
    /// <summary>
    /// SpellUIのドラッグが開始されたときに呼ばれます。
    /// </summary>
    void OnSpellDragBegan();

    /// <summary>
    /// SpellUIのドラッグが終了したときに呼ばれます（ドロップ成功/失敗にかかわらず）。
    /// </summary>
    void OnSpellDragEnded();
}