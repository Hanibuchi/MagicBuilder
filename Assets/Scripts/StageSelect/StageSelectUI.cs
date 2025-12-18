// StageSelectUI.cs

using UnityEngine;
using System.Linq;
using TMPro;

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

    [SerializeField]
    TextMeshProUGUI islandNameText;

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
    [SerializeField] AudioClip islandSelectSound;
    [SerializeField] float islandSelectSoundVolume = 1.0f;

    /// <summary>
    /// IslandSelectorから呼び出され、新しい島が選択されたことを通知します。
    /// </summary>
    /// <param name="islandID">選択された島の識別子。</param>
    public void OnIslandSelected(string islandID)
    {
        Debug.Log($"島が選択されました: {islandID}");
        if (allIslandSelectors == null) return;

        foreach (var selector in allIslandSelectors)
        {
            if (selector == null) continue;

            if (selector.islandID == islandID)
            {
                // 新しく選択された島
                currentSelectedIsland = selector;
                selector.Select();
            }
            else
            {
                // 他の全ての島を非選択状態へ
                selector.Normalize();
            }
        }

        // 2. UIの表示アニメーション開始
        gameObject.SetActive(true);
        uiAnimator.SetTrigger(selectAnimTrigger);

        if (SoundManager.Instance != null && islandSelectSound != null)
            SoundManager.Instance.PlaySE(islandSelectSound, islandSelectSoundVolume);

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

        foreach (var selector in allIslandSelectors)
            selector?.Normalize();
    }

    // --- 内部ヘルパーメソッド ---

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

        islandNameText.text = islandStageMapConfig.GetIslandNameByID(islandID);
        // 1. 紐づけ設定からステージリストを取得
        string[] stageEntries = islandStageMapConfig.GetStagesForIsland(islandID);

        if (stageEntries == null || stageEntries.Length == 0)
        {
            Debug.LogWarning($"ステージID '{islandID}' に対応するステージが設定されていません。");
            return;
        }

        // StageUnlockManagerのインスタンスを取得
        if (StageUnlockManager.Instance == null)
        {
            Debug.LogError("StageUnlockManagerが見つかりません。ステージの解放状態を判定できません。", this);
            return;
        }

        // 最新到達ステージIDを取得 (このIDの次のステージが「NEW!」となる)
        string latestReachedStageId = StageUnlockManager.Instance.GetLatestReachedStageID();

        int num = 1;
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
                string displayStageName = num.ToString();
                string displaySubName = stageInfo.subStageName;
                bool isUnlocked = StageUnlockManager.Instance.IsStageUnlocked(stageInfo.stageName);


                if (isUnlocked)// 1. 解放済みの場合
                {
                    // StageButtonに識別子と表示名を設定
                    stageButton.Setup(this, stageInfo.stageName, displayStageName, stageInfo.subStageName, num);
                    Debug.Log($"latestReachedStageId: {latestReachedStageId}, entry: {entry}");
                    if (latestReachedStageId == entry)
                        // 最新到達ステージの次のステージであれば、NEW!マークを表示
                        stageButton.ShowNewStageIndicator();
                    else
                        // 既に到達済みのステージ（latestIndex以下）は、NEW!マークを非表示
                        stageButton.HideNewStageIndicator();
                }
                else
                {
                    // 3. 未解放の場合

                    // 表示名を???にする
                    displaySubName = "???";

                    // StageButtonに識別子と表示名を設定
                    stageButton.Setup(this, stageInfo.stageName, displayStageName, displaySubName, num);

                    // ボタンを無効化
                    stageButton.DisableButton();
                    // 未解放なのでNEW!マークは表示しない（非表示にする）
                    stageButton.HideNewStageIndicator();
                }

                num++;
            }
            else
            {
                Debug.LogError("StageSelectUI: StageButtonPrefabにStageButtonコンポーネントが見つかりません！");
                Destroy(buttonObject); // 無効なオブジェクトを削除
            }
        }
    }

    private int currentShowingStageIndex = -1;

    /// <summary>
    /// ボタンが押された時に StageInfoDisplayUI を制御する
    /// </summary>
    public void HandleStageButtonClick(string identifier, int index)
    {
        Debug.Log($"HandleStageButtonClick: identifier={identifier}, index={index}");
        if (StageInfoDisplayUI.Instance == null) return;

        var stageInfo = StageStarter.Instance.GetStageInfoByName(identifier);
        string islandName = islandStageMapConfig.GetIslandNameByID(currentSelectedIsland.islandID);

        // すでに表示中の場合、アニメーションを判定
        if (currentShowingStageIndex != -1)
        {
            if (index > currentShowingStageIndex)
                StageInfoDisplayUI.Instance.PlayNextAnimation();
            else if (index < currentShowingStageIndex)
                StageInfoDisplayUI.Instance.PlayPrevAnimation();
        }
        Debug.Log($"HandleStageButtonClick: identifier={identifier}, index={index}");
        // 情報の更新と表示
        StageInfoDisplayUI.Instance.SetStageInfo(this, islandName, stageInfo.subStageName, identifier);
        StageInfoDisplayUI.Instance.Open();

        // 現在の状態を記録
        currentShowingStageIndex = index;
    }

    public void OnStageInfoDisplayUIClosed()
    {
        currentShowingStageIndex = -1;
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