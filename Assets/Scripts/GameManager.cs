// GameManager.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム全体の管理（シングルトン、DontDestroyOnLoad）。
/// ステージの構成情報保持とステージシーンのロードを担当します。
/// </summary>
public class GameManager : MonoBehaviour, IStageStartListener
{
    // ★ 外部からStageConfigへのアクセスが必要な場合のPublicプロパティ
    public StageConfig CurrentStageConfig { get; private set; }

    /// <summary>
    /// ステージ選択画面に遷移した際、自動的に選択を試みるターゲットステージの名前。
    /// </summary>
    public string StageSelectTargetStageName;

    // --- シングルトン ---
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // シーンを跨いでオブジェクトを維持
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // ゲーム開始時にバナー広告を表示
        // AdController.Instance?.ShowBanner(); // UIの配置を変えないといけないのだるい。
    }

    // --- IStageStartListener の実装 ---

    /// <summary>
    /// StageStarterからステージ開始通知を受け取り、ステージ設定を保持してシーンをロードします。
    /// </summary>
    /// <param name="config">開始するステージのStageConfig</param>
    public void OnStageStart(StageConfig config)
    {
        if (config == null)
        {
            Debug.LogError("StageConfigがnullのため、ステージを開始できません。");
            return;
        }

        // 1. ステージ構成情報を保持
        CurrentStageConfig = config;
        Debug.Log($"GameManager: ステージ設定 '{config.stageName}' を保持しました。");
        // ステージ選択画面に遷移した際、自動的に選択を試みるターゲットステージの名前を更新
        StageSelectTargetStageName = config.stageName;

        // 2. ステージシーンをロード
        Debug.Log($"GameManager: ステージシーン '{config.SceneName}' をロードします。");

        // LoadSceneMode.Singleでロードすると、現在のシーン（例：ステージ選択シーン）をアンロードし、
        // StageManagerを持つStage_Gameplayシーンをロードします。
        SceneTransitionManager.Instance.LoadScenesWithTransition(config.SceneName);
        // SceneTransitionManager.Instance.LoadScenesWithTransition(new List<string> { config.SceneName, STAGE_COMMON_SCENE_NAME });
    }

    const string STAGE_SELECT_SCENE_NAME = "StageSelect";
    public void LoadStageSelectScene()
    {
        SceneTransitionManager.Instance.LoadScenesWithTransition(STAGE_SELECT_SCENE_NAME);
    }
}