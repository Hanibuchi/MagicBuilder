using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System;

public class StageManager : MonoBehaviour, IZeroEnemyNotifier
{
    // --- インスペクタから設定するフィールド ---

    public static StageManager Instance { get; private set; }

    [Header("ステージ設定アセット")]
    [Tooltip("このステージの設定情報を持つStageConfigアセット")]
    [SerializeField]
    private StageConfig stageConfig;

    [Header("ステージクリア設定")]
    [SerializeField]
    private StageClearCondition clearCondition = StageClearCondition.SpecificBossDefeated;

    [Header("ステージ構成要素")]
    [Tooltip("Instantiateするステージ固有のPrefabのリスト")]
    [SerializeField] private List<GameObject> stagePrefabs = new List<GameObject>();

    [Header("プレイヤー設定")]
    [Tooltip("プレイヤーのPrefab")]
    [SerializeField] private GameObject playerPrefab;

    [Tooltip("プレイヤーをInstantiateする初期位置を示すTransformマーカー")]
    [SerializeField] private Transform playerSpawnPoint;

    // --- 定数 ---

    private const string StageCommonSceneName = "Stage_Common";

    [SerializeField] Transform enemySpawnPoint;

    // --- Unityライフサイクルメソッド ---

    private void Awake()
    {
        Instance = this;

        // 1. StageCommonシーンのAdditiveロード
        LoadCommonScene();

        // 2. ステージ固有のPrefabのInstantiate
        InstantiateStageElements();

        // 3. プレイヤーのInstantiate
        InstantiatePlayer();
    }

    // --- プライベートメソッド ---

    /// <summary>
    /// StageCommonシーンがロードされていない場合、Additiveでロードします。
    /// </summary>
    private void LoadCommonScene()
    {
        // SceneManager.GetSceneByNameで、現在ロードされているシーンの中から指定された名前のシーンを探す
        Scene commonScene = SceneManager.GetSceneByName(StageCommonSceneName);

        // シーンが存在しない、または無効な場合はロードする
        if (!commonScene.isLoaded)
        {
            // Additiveでシーンをロードし、現在のステージシーンに追加する
            SceneManager.LoadScene(StageCommonSceneName, LoadSceneMode.Additive);
            Debug.Log($"{StageCommonSceneName} シーンをロードしました。");
        }
        else
        {
            Debug.Log($"{StageCommonSceneName} シーンは既にロードされています。");
        }
    }

    /// <summary>
    /// インスペクタで指定されたステージ固有のPrefabをInstantiateします。
    /// </summary>
    private void InstantiateStageElements()
    {
        if (stagePrefabs.Count == 0)
        {
            Debug.LogWarning("InstantiateするステージPrefabが設定されていません。");
            return;
        }

        Debug.Log("ステージ固有の要素をInstantiateしています...");

        // リスト内のPrefabを一つずつInstantiateする
        foreach (GameObject prefab in stagePrefabs)
        {
            if (prefab != null)
            {
                // Instantiate時に生成したオブジェクトをStageManagerの子に設定（オプション）
                // GameObject instance = Instantiate(prefab, transform);
                GameObject instance = Instantiate(prefab);
                // 必要に応じて、初期位置の設定などを行う
                // instance.transform.position = Vector3.zero; // 必要に応じて調整
                Debug.Log($"Prefab: {prefab.name} をInstantiateしました。");
            }
        }
    }

    /// <summary>
    /// プレイヤーのPrefabを指定された初期位置にInstantiateします。
    /// </summary>
    private void InstantiatePlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("プレイヤーPrefabが設定されていません！");
            return;
        }

        if (playerSpawnPoint == null)
        {
            Debug.LogError("プレイヤー初期位置マーカー (Player Spawn Point) が設定されていません！");
            return;
        }

        // プレイヤーを初期位置に、回転を維持してInstantiateする
        Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);

        Debug.Log($"プレイヤーを位置: {playerSpawnPoint.position} にInstantiateしました。");
    }

    private void Start()
    {
        GameTimerManager.Instance.StartTimer();
        StartPhase();
    }

    public void StartPhase()
    {
        EnemyPhaseExecutor.Instance.SetSpawnPoint(enemySpawnPoint.position);
        EnemyPhaseExecutor.Instance.StartPhase(test_phases, () => { spawnComplete = true; });
        EnemyCounter.Instance.SetZeroNotifier(this);
    }

    public EnemyPhaseConfig[] test_phases;


    /// <summary>
    /// ボスとして指定された敵が倒されたときに呼び出されます。
    /// これにより、ステージクリアが判定されます。
    /// </summary>
    public void NotifyBossDefeatedForClear() // メソッド名を変更
    {
        Debug.Log("🛡️ ボス撃破通知を受け取りました！");
        if (clearCondition == StageClearCondition.SpecificBossDefeated)
            HandleStageClear();
    }

    bool spawnComplete = false;
    // IZeroEnemyNotifier インターフェースの実装
    public void OnEnemyCountZero()
    {
        if (clearCondition == StageClearCondition.AllEnemiesDefeated && spawnComplete)
        {
            HandleStageClear();
        }
    }


    private bool isStageClear = false;
    private bool isGameOver = false; // ★ 追加: ゲームオーバーフラグ
    /// <summary>
    /// ステージクリア時の処理を実行します。
    /// </summary>
    private void HandleStageClear()
    {
        if (isStageClear || isGameOver) return;
        isStageClear = true;
        Debug.Log("🎉 ステージクリア！");
        GameTimerManager.Instance.StopTimer();

        StartCoroutine(DelayAndPauseGameOnGameClear());
    }
    public static Action OnStageClearForceDie;

    [Header("ステージクリア設定")] // 追記
    [Tooltip("クリア後の演出時間（秒）。この時間後にゲームが停止します。")]
    [SerializeField] private float clearDelaySeconds = 1.5f; // 例として3.0秒

    /// <summary>
    /// 指定された秒数だけ待機した後、ゲームを停止します。
    /// </summary>
    private IEnumerator DelayAndPauseGameOnGameClear() // 追記
    {
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(clearDelaySeconds);
        OnStageClearForceDie?.Invoke(); // 全ての敵に死亡通知を送る
        Time.timeScale = 1f;

        yield return new WaitForSecondsRealtime(clearDelaySeconds);
        PlayerController.Instance.Victory();

        yield return new WaitForSecondsRealtime(clearDelaySeconds);
        InstantiateResultPanel(true); // 勝利 (isVictory: true) でリザルトを表示
    }

    public void GameOver()
    {
        HandleGameOver();
    }

    // ★ 新規追加: ゲームオーバー時の処理を実行します。
    /// <summary>
    /// ゲームオーバー時の処理を実行します。（主にプレイヤーHPが0になった時などに呼び出す）
    /// </summary>
    void HandleGameOver()
    {
        if (isStageClear || isGameOver) return;
        isGameOver = true;
        Debug.Log("💀 ゲームオーバー！");
        GameTimerManager.Instance.StopTimer(); // タイマーを停止
        // ゲームオーバー演出（コルーチン）
        StartCoroutine(DelayAndPauseGameOnGameOver());
    }

    [SerializeField] private float gameOverDelaySeconds = 1.5f; // 例として3.0秒
    private IEnumerator DelayAndPauseGameOnGameOver()
    {
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(gameOverDelaySeconds);

        Time.timeScale = 1f;
        yield return new WaitForSecondsRealtime(gameOverDelaySeconds);

        // Time.timeScale = 0f;
        InstantiateResultPanel(false); // 敗北 (isVictory: false) でリザルトを表示
        Debug.Log("ゲームオーバー後、ゲームを一時停止しました。");
    }

    [Header("UI設定")]
    [Tooltip("ステージクリア/ゲームオーバー時にInstantiateするリザルトパネルのPrefab")]
    [SerializeField] private GameObject resultPanelPrefab;
    private const string VICTORY_MESSAGE = "勝利！"; // StageResultData用
    private const string DEFEAT_MESSAGE = "もう一度挑戦しましょう！"; // StageResultData用

    /// <summary>
    /// リザルトパネルをInstantiateし、結果データを設定します。
    /// </summary>
    private void InstantiateResultPanel(bool isVictory)
    {
        if (resultPanelPrefab == null)
        {
            Debug.LogError("リザルトパネルPrefabが設定されていません！");
            return;
        }

        // プレハブをInstantiate
        GameObject panelInstance = Instantiate(resultPanelPrefab);
        ResultPanelController controller = panelInstance.GetComponent<ResultPanelController>();

        if (controller == null)
        {
            Debug.LogError("Instantiateされたプレハブに ResultPanelController が見つかりません！");
            return;
        }

        // データの準備
        // ※ 本来はStageDataなどから取得しますが、ここでは仮のデータまたは既存マネージャーから取得
        ResultPanelController.StageResultData data = new ResultPanelController.StageResultData
        {
            stageName = stageConfig.stageName,
            stageSubName = stageConfig.subStageName,
            score = Mathf.RoundToInt(ScoreManager.Instance.GetTotalScore()), // ScoreManagerから取得
            clearTimeSeconds = GameTimerManager.Instance.GetElapsedTime(), // GameTimerManagerから取得
            message = isVictory ? VICTORY_MESSAGE : DEFEAT_MESSAGE
        };

        // UIへの設定と表示制御
        if (isVictory)
        {
            controller.DisplayVictory(data);
        }
        else
        {
            controller.DisplayDefeat(data);
        }

        // ボタンアクションの設定（例：シーン遷移処理を実装）
        // ※ この部分の具体的な実装は、プロジェクトのシーン管理によって異なります。
        Action onStageSelect = () =>
        {
            Debug.Log("ステージセレクトへ");
            Time.timeScale = 1f;
            /* SceneManager.LoadScene("StageSelectScene"); */
        };
        Action onRetry = () =>
        {
            Debug.Log("リトライ");
            Time.timeScale = 1f;
            // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        };
        Action onNextStage = () =>
        {
            Debug.Log("次のステージへ");
            Time.timeScale = 1f; /* 次のステージへ遷移 */
        };
        Action onSpellChange = () =>
        {
            Debug.Log("呪文変更へ");
            Time.timeScale = 1f; /* 呪文変更画面へ遷移 */
        };

        controller.SetupActions(onStageSelect, onRetry, onNextStage, onSpellChange);
        Debug.Log($"リザルトパネルをInstantiateし、{(isVictory ? "勝利" : "敗北")}結果を設定しました。");
    }
}

// ファイル名: StageManager.cs (StageManagerクラスの外側、またはStageManagerクラスの中で定義)
public enum StageClearCondition
{
    // 敵の数が0になったとき
    AllEnemiesDefeated,
    // 特定のボスを倒したとき
    SpecificBossDefeated
    // 必要であれば他の条件（例: 時間切れ、パズルクリアなど）を追加可能
}