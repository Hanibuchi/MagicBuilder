// StageSelectUI.cs

using UnityEngine;
using System.Linq;

/// <summary>
/// 選択された島に対応するステージ一覧を表示するUIを管理するクラス。
/// IslandSelectorの操作を受け付け、ステージボタンを生成・表示します。
/// </summary>
public class StageSelectUI : MonoBehaviour
{
    // --- インスペクタから設定するフィールド ---

    [Header("UI設定")]
    [Tooltip("このUI全体のAnimatorコンポーネント。")]
    [SerializeField]
    private Animator uiAnimator;

    [Tooltip("ステージボタンを配置する親となるTransform。")]
    [SerializeField]
    private Transform stageButtonParent;

    [Header("プレハブ設定")]
    [Tooltip("ステージボタンのプレハブ。StageButtonコンポーネントがアタッチされている必要があります。")]
    [SerializeField]
    private GameObject stageButtonPrefab;

    [Header("データ設定")]
    [Tooltip("島とステージの紐づけ情報を持つScriptableObject。")]
    [SerializeField]
    private IslandStageMappingConfig islandStageMapConfig;

    [Tooltip("全ての島選択用コンポーネントのリスト。")]
    [SerializeField]
    private IslandSelector[] allIslandSelectors;

    // --- 内部状態 ---

    private IslandSelector currentSelectedIsland = null; // 現在選択中のIslandSelector

    private string selectAnimTrigger = "Show"; // UI表示アニメーションのトリガー名
    private string normalizedAnimTrigger = "Hide"; // UI非表示アニメーションのトリガー名

    // --- 初期化 ---

    private void Start()
    {
        // 必須コンポーネントのチェック
        if (uiAnimator == null || stageButtonPrefab == null || islandStageMapConfig == null || stageButtonParent == null)
        {
            Debug.LogError("StageSelectUI: 必須フィールドが設定されていません。", this);
            enabled = false;
            return;
        }

        // 初期状態として非表示
        gameObject.SetActive(false);
    }

    // --- 外部から呼び出されるメソッド (IslandSelectorと連携) ---

    /// <summary>
    /// IslandSelectorから呼び出され、新しい島が選択されたことを通知します。
    /// </summary>
    /// <param name="islandID">選択された島の識別子。</param>
    public void OnIslandSelected(string islandID)
    {
        // 1. 他の島の選択解除
        NormalizeAllOtherIslands(islandID);

        // 2. UIの表示アニメーション開始
        gameObject.SetActive(true);
        uiAnimator.SetTrigger(selectAnimTrigger);

        // 3. ステージボタンの生成と表示
        GenerateStageButtons(islandID);
    }

    /// <summary>
    /// IslandSelectorから呼び出され、島の選択が解除されたことを通知します。
    /// </summary>
    public void OnIslandDeselected()
    {
        // UIの非表示アニメーション開始
        uiAnimator.SetTrigger(normalizedAnimTrigger);
        currentSelectedIsland = null;
    }

    // --- 内部ヘルパーメソッド ---

    /// <summary>
    /// 新しく選択された島以外の全てのIslandSelectorを非選択状態(Normalize)にします。
    /// </summary>
    /// <param name="newlySelectedIslandID">新しく選択された島のID。</param>
    private void NormalizeAllOtherIslands(string newlySelectedIslandID)
    {
        if (allIslandSelectors == null) return;

        // 現在選択中のIslandSelectorを更新
        IslandSelector newIsland = null;

        foreach (var selector in allIslandSelectors)
        {
            if (selector == null) continue;

            if (selector.islandID == newlySelectedIslandID)
            {
                // 新しく選択された島
                newIsland = selector;
                // ここではSelect()がIslandSelector側で既に呼ばれているので何もしない
            }
            else
            {
                // 他の全ての島を非選択状態へ
                selector.Normalize();
            }
        }
        currentSelectedIsland = newIsland;
    }


    /// <summary>
    /// 既存のボタンを全て削除し、指定された島のステージボタンを生成します。
    /// </summary>
    /// <param name="islandID">ステージボタンを生成したい島の識別子。</param>
    private void GenerateStageButtons(string islandID)
    {
        // 既存のボタンを全て削除
        foreach (Transform child in stageButtonParent)
        {
            Destroy(child.gameObject);
        }

        // 1. 紐づけ設定からステージリストを取得
        string[] stageEntries = islandStageMapConfig.GetStagesForIsland(islandID);

        if (stageEntries == null || stageEntries.Length == 0)
        {
            Debug.LogWarning($"ステージID '{islandID}' に対応するステージが設定されていません。");
            return;
        }

        // 2. リストに基づいてボタンを生成
        foreach (var entry in stageEntries)
        {
            // ボタンのインスタンス化
            GameObject buttonObject = Instantiate(stageButtonPrefab, stageButtonParent);
            
            // StageButtonコンポーネントを取得
            StageButton stageButton = buttonObject.GetComponent<StageButton>();

            if (stageButton != null)
            {
                var stageInfo = StageStarter.Instance.GetStageInfoByName(entry);
                // StageButtonに識別子と表示名を設定
                stageButton.Setup(stageInfo.stageName, stageInfo.subStageName);
            }
            else
            {
                Debug.LogError("StageSelectUI: StageButtonPrefabにStageButtonコンポーネントが見つかりません！");
                Destroy(buttonObject); // 無効なオブジェクトを削除
            }
        }
    }

    // --- アニメーションから呼び出すメソッド ---

    /// <summary>
    /// UI非表示アニメーションの最後に呼び出され、UIを完全に非アクティブにします。
    /// </summary>
    public void SetUIInactive()
    {
        gameObject.SetActive(false);
        // ステージボタンのクリアは、次の島が選択されたとき（GenerateStageButtonsの冒頭）に行うため、ここでは不要
    }
}