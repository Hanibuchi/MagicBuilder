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

    [SerializeField] Transform enemySpawnPoint;

    // --- Unityライフサイクルメソッド ---

    private void Awake()
    {
        Instance = this;
        ApplyStageConfigFromGameManager();

        // 2. ステージ固有のPrefabのInstantiate
        InstantiateStageElements();

        // 3. プレイヤーのInstantiate
        InstantiatePlayer();
    }

    /// <summary>
    /// GameManagerからステージ設定を取得し、このStageManagerに適用します。
    /// GameManagerがない場合は、インスペクタで設定されたデバッグ用の設定を使用します。
    /// </summary>
    private void ApplyStageConfigFromGameManager()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentStageConfig != null)
        {
            this.stageConfig = GameManager.Instance.CurrentStageConfig;
            Debug.Log($"StageManager: GameManagerからステージ設定 '{stageConfig.stageName}' を取得しました。");
        }

        this.clearCondition = stageConfig.clearCondition;
    }

    // --- プライベートメソッド ---

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

    [Header("クリア条件UI設定")]
    [Tooltip("ステージクリア条件に対応するUIプレハブのリスト")]
    [SerializeField]
    private List<ClearConditionUIMapping> clearConditionUIMappings = new List<ClearConditionUIMapping>();

    [System.Serializable]
    public class ClearConditionUIMapping
    {
        public StageClearCondition condition; // 対応するクリア条件
        public GameObject uiPrefab;          // 生成するClearConditionUIプレハブ
    }




    private void Start()
    {
        StartCoroutine(DelayedStartRoutine());
    }

    [Tooltip("InstantiateClearConditionUIを実行するまでの待機時間（秒）")]
    public float startDelayTime = 1.0f; // デフォルト値を設定しておくと便利
    /// <summary>
    /// 指定された時間待機してからクリア条件UIを生成するコルーチン
    /// </summary>
    private IEnumerator DelayedStartRoutine()
    {
        // 指定された時間だけ待機
        // 
        yield return new WaitForSeconds(startDelayTime);

        // 待機後に本来実行したかったメソッドを呼び出す
        InstantiateClearConditionUI();

        // 必要に応じて、待機後の他の初期化処理などをここに追加
    }

    /// <summary>
    /// 設定されたクリア条件に対応するClearConditionUIをInstantiateします。
    /// </summary>
    private void InstantiateClearConditionUI()
    {
        GameObject targetPrefab = null;

        // 現在のクリア条件に対応するプレハブをリストから探す
        foreach (var mapping in clearConditionUIMappings)
        {
            if (mapping.condition == clearCondition)
            {
                targetPrefab = mapping.uiPrefab;
                break;
            }
        }

        if (targetPrefab == null)
        {
            Debug.LogWarning($"現在のクリア条件 ({clearCondition}) に対応するClearConditionUIプレハブが設定されていません。UIを表示せずにゲームを開始します。");
            StartGameImmediately();
            return;
        }

        // UIを生成
        GameObject uiInstance = Instantiate(targetPrefab);
        ClearConditionUI uiController = uiInstance.GetComponent<ClearConditionUI>();

        if (uiController == null)
        {
            Debug.LogError($"生成されたClearConditionUIプレハブに ClearConditionUI コンポーネントが見つかりません。");
            Destroy(uiInstance);
            StartGameImmediately();
            return;
        }

        // UIが閉じられた（クリックされた）後に実行するコールバックを設定
        uiController.SetAction(StartGameImmediately);
        Debug.Log($"クリア条件UI ({clearCondition}) をInstantiateしました。");
    }

    [SerializeField] AudioClip bGM;
    /// <summary>
    /// UI表示なし、またはUIが閉じられた後に実行される、実際のゲーム開始処理。
    /// </summary>
    private void StartGameImmediately()
    {
        Debug.Log("ゲーム開始処理を実行します。");
        GameTimerManager.Instance.StartTimer();
        StartPhase();

        if (SoundManager.Instance != null && bGM != null)
            SoundManager.Instance.PlayBGM(bGM);
    }

    public void StartPhase()
    {
        EnemyPhaseExecutor.Instance.SetSpawnPoint(enemySpawnPoint.position);
        EnemyPhaseExecutor.Instance.StartPhase(stageConfig.enemyPhases, () => { spawnComplete = true; });
        EnemyCounter.Instance.SetZeroNotifier(this);
    }

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

    void OnGameEnd()
    {
        GameTimerManager.Instance.StopTimer(); // タイマーを停止
        WandUIManager.Instance.Hide();
        SpellInventory.Instance.Hide();
    }
    private bool gameEnd = false;
    public bool GameEnd => gameEnd;


    [Tooltip("全ステージのリスト設定を持つScriptableObject。")]
    // StageListConfigのインスタンスをインスペクタから設定できるようにする
    public StageListConfig stageListConfig;
    /// <summary>
    /// ステージクリア時の処理を実行します。
    /// </summary>
    private void HandleStageClear()
    {
        if (gameEnd) return;
        gameEnd = true;
        Debug.Log("🎉 ステージクリア！");
        OnGameEnd();
        StartCoroutine(DelayAndPauseGameOnGameClear());

        if (stageListConfig == null)
        {
            Debug.LogError("StageListConfigが設定されていません。");
            return;
        }
        var nextStage = stageListConfig.GetNextStageInfoByName(stageConfig.stageName);
        if (nextStage == null)
        {
            Debug.LogWarning($"次のステージが存在しません。現在のステージ '{stageConfig.stageName}' は最後のステージです。");
        }
        // このステージの次のステージを最新のステージとして登録しようと試みる。最新のステージでない場合（つまり再プレイ時）何もせず、最新のステージの場合、ステージ選択画面になったときにそのステージの島が選択されるようにする。
        else
        {
            StageUnlockManager.Instance.UnlockStage(nextStage.stageName);
            if (StageUnlockManager.Instance.UpdateLatestReachedStage(nextStage.stageName))
            {
                GameManager.Instance.StageSelectTargetStageName = nextStage.stageName;
                Debug.Log($"最新のステージとして '{nextStage.stageName}' を登録しました。");
            }
            else
            {
                Debug.Log($"最新のステージとして '{nextStage.stageName}' を登録しませんでした。");
            }
        }
    }
    public static Action OnStageClearForceDie;

    [Header("ステージクリア設定")] // 追記
    [Tooltip("クリア後の演出時間（秒）。この時間後にゲームが停止します。")]
    [SerializeField] private float clearDelaySeconds = 2f; // 例として3.0秒

    [SerializeField] AudioClip bossDestroySound;
    [SerializeField] AudioClip clearSound;
    /// <summary>
    /// 指定された秒数だけ待機した後、ゲームを停止します。
    /// </summary>
    private IEnumerator DelayAndPauseGameOnGameClear() // 追記
    {
        if (SoundManager.Instance != null && bossDestroySound != null)
        {
            SoundManager.Instance.StopBGMWithFade(0.5f);
            SoundManager.Instance.PlaySE(bossDestroySound);
        }

        Time.timeScale = 0.5f;
        yield return new WaitForSeconds(clearDelaySeconds * 0.5f);
        OnStageClearForceDie?.Invoke(); // 全ての敵に死亡通知を送る
        Time.timeScale = 1f;

        yield return new WaitForSeconds(clearDelaySeconds);
        PlayerController.Instance.Victory();

        if (SoundManager.Instance != null && clearSound != null)
            SoundManager.Instance.PlaySE(clearSound);

        yield return new WaitForSeconds(clearDelaySeconds);
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
        if (gameEnd) return;
        gameEnd = true;
        OnGameEnd();
        Debug.Log("💀 ゲームオーバー！");
        // ゲームオーバー演出（コルーチン）
        StartCoroutine(DelayAndPauseGameOnGameOver());
    }

    [SerializeField] private float gameOverDelaySeconds = 1.5f; // 例として3.0秒
    private IEnumerator DelayAndPauseGameOnGameOver()
    {
        Time.timeScale = 0.5f;
        yield return new WaitForSeconds(gameOverDelaySeconds * 0.5f);

        Time.timeScale = 1f;
        yield return new WaitForSeconds(gameOverDelaySeconds);

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
        bool clicked = false;
        Action onStageSelect = () =>
        {
            if (clicked) return;
            clicked = true;
            Debug.Log("ステージセレクトへ");
            Time.timeScale = 1f;
            GameManager.Instance.LoadStageSelectScene();
        };
        Action onRetry = () =>
        {
            if (clicked) return;
            clicked = true;
            Debug.Log("リトライ");
            Time.timeScale = 1f;
            GameManager.Instance.OnStageStart(stageConfig);
        };
        Action onNextStage = () =>
        {
            if (clicked) return;
            clicked = true;
            Debug.Log("次のステージへ");
            Time.timeScale = 1f; /* 次のステージへ遷移 */
            GameManager.Instance.LoadStageSelectScene();
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