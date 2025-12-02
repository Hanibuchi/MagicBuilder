// GameManager.cs
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
        // 起動時にStageStarterに自身をリスナーとして登録する（ステージ選択画面などがある場合、
        // StageSelectControllerなどから登録される方が自然ですが、デバッグ用にここに記述）
        // StageSelectControllerがロードされた際にそちらで登録するのが理想です。
        // StageStarter starter = FindObjectOfType<StageStarter>();
        // if (starter != null)
        // {
        //     starter.SetStageStartListener(this);
        // }
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

        // 2. ステージシーンをロード
        Debug.Log($"GameManager: ステージシーン '{config.SceneName}' をロードします。");
        
        // LoadSceneMode.Singleでロードすると、現在のシーン（例：ステージ選択シーン）をアンロードし、
        // StageManagerを持つStage_Gameplayシーンをロードします。
        SceneManager.LoadScene(config.SceneName);
    }
}