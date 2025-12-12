// StageSelectController.cs
using UnityEngine;
using System.Linq;

/// <summary>
/// ステージ選択画面における操作を担い、GameManagerをStageStarterに登録し、
/// 選択されたステージの開始をStageStarterに要求します。
/// </summary>
public class StageSelectController : MonoBehaviour, IStageStartListener
{
    [SerializeField] AudioClip bGM;

    // --- 追加するフィールド ---
    [Header("ステージ選択UI連携")]
    [Tooltip("ステージ選択UIの参照。対応する島を自動選択するために使用。")]
    [SerializeField]
    private StageSelectUI stageSelectUI; // StageSelectUIへの参照

    [Tooltip("島とステージの紐づけ情報を持つScriptableObject。")]
    [SerializeField]
    private IslandStageMappingConfig islandStageMapConfig; // IslandStageMappingConfigへの参照
    // -------------------------

    private void Start()
    {
        // StageStarterインスタンスを取得
        StageStarter starter = StageStarter.Instance;

        if (starter == null)
        {
            Debug.LogError("StageStarterがシーンに見つかりません。ステージ開始システムが機能しません。");
            return;
        }

        starter.SetStageStartListener(this);

        // 必須コンポーネントのチェック（追加）
        if (stageSelectUI == null || islandStageMapConfig == null)
        {
            Debug.LogError("StageSelectController: StageSelectUI または IslandStageMappingConfig が設定されていません。");
        }
        // GameManagerをStageStarterのリスナーとして登録
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManagerインスタンスが見つかりません。登録できませんでした。");
        }

        starter.SetStageStartListener(GameManager.Instance);
        Debug.Log("StageSelectController: GameManagerをStageStarterに登録しました。");

        // ステージ選択UIの自動選択処理
        AutomaticallySelectIsland();

        if (SoundManager.Instance != null && bGM != null)
        {
            SoundManager.Instance.PlayBGM(bGM);
        }
    }

    /// <summary>
    /// GameManagerから取得したステージ識別子に基づき、対応する島を自動選択します。
    /// </summary>
    private void AutomaticallySelectIsland()
    {
        if (GameManager.Instance == null || stageSelectUI == null || islandStageMapConfig == null) return;

        if (string.IsNullOrEmpty(GameManager.Instance.StageSelectTargetStageName)) // ゲーム開始時or再開時
        {
            Debug.Log("自動選択のためのステージ識別子が見つかりませんでした。");
            // 最新到達ステージIDを取得して設定。ゲーム開始時は最初のステージ、ゲーム再開時は最新のステージが返される。
            GameManager.Instance.StageSelectTargetStageName = StageUnlockManager.Instance.GetLatestReachedStageID();
        }
        string targetStageName = GameManager.Instance.StageSelectTargetStageName;

        // IslandStageMappingConfigから、指定されたステージを含む島IDを検索
        string islandIDToSelect = islandStageMapConfig.GetIslandIDForStage(targetStageName);

        if (!string.IsNullOrEmpty(islandIDToSelect))
        {
            Debug.Log($"自動的に島 '{islandIDToSelect}' を選択します。");
            // StageSelectUIのメソッドを呼び出して島を選択
            stageSelectUI.OnIslandSelected(islandIDToSelect);
        }
        else
        {
            Debug.LogWarning($"ステージ '{targetStageName}' に対応する島が見つかりませんでした。自動選択をスキップします。");
        }
    }


    public void OnStageStart(StageConfig config)
    {
        SoundManager.Instance.StopBGMWithFade(0.5f);
    }
}