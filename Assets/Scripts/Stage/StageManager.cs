using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System;

public class StageManager : MonoBehaviour
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
        // デバッグ用：UIを表示せずに即座に開始する場合
        if (clearCondition == StageClearCondition.Debug_None)
        {
            Debug.Log("クリア条件が Debug_None のため、UIを表示せずにゲームを開始します。");
            StartGameImmediately();
            return;
        }

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

    [Header("開始時演出設定")]
    [Tooltip("持ち込み呪文の全投入にかける合計時間（秒）")]
    [SerializeField] private float totalSpellDropDuration = 1.2f;
    [Tooltip("最後の呪文が投入されてから敵の生成を開始するまでの予備待機時間")]
    [SerializeField] private float postDropWaitTime = 1.0f;
    [SerializeField] AudioClip bGM;
    /// <summary>
    /// UI表示なし、またはUIが閉じられた後に実行される、実際のゲーム開始処理。
    /// </summary>
    private void StartGameImmediately()
    {
        // 解放されている杖を取得して追加
        var unlockedWands = WandUnlockManager.Instance.GetUnlockedWands();
        if (unlockedWands != null && unlockedWands.Length > 0)
        {
            foreach (var wand in unlockedWands)
            {
                wand.Reset();
                WandsController.Instance.GenerateNewWand(wand);
            }
            // 最初の杖に切り替え
            WandsController.Instance.SwitchWand(0);
        }

        WandUIManager.Instance.Show();
        SpellInventory.Instance.Show();
        // 直接 StartPhase を呼ばず、呪文投入演出のコルーチンを開始する
        StartCoroutine(EquipSpellsSequenceRoutine());
    }

    /// <summary>
    /// 持ち込み呪文を一つずつインベントリに飛ばし、完了後にフェーズを開始するコルーチン
    /// </summary>
    private IEnumerator EquipSpellsSequenceRoutine()
    {
        // 1. 持ち込み呪文のリストを取得
        var equippedSpells = EquippedSpellManager.Instance.GetEquippedSpells();
        // 有効な呪文の数をカウント（nullを除外）
        int activeSpellCount = 0;
        if (equippedSpells != null)
        {
            foreach (var spell in equippedSpells)
            {
                if (spell != null) activeSpellCount++;
            }
        }

        // 投入間隔を計算。呪文数が多いほど間隔を短くし、合計時間を一定に近づける。
        float interval = (activeSpellCount > 0) ? totalSpellDropDuration / activeSpellCount : 0.5f;

        yield return new WaitForSeconds(interval);

        // プレイヤーのTransformを取得
        Transform playerTransform = PlayerController.Instance.transform;

        if (activeSpellCount > 0)
        {
            Debug.Log($"{activeSpellCount} 個の持ち込み呪文を投入します。間隔: {interval:F2}秒");

            foreach (var spell in equippedSpells)
            {
                // 空スロット(null)はスキップ
                if (spell == null) continue;

                // SpellDropManager を通じて呪文を飛ばす
                // 開始地点は PlayerController の position
                SpellDropManager.Instance.DropSpell(playerTransform.position, spell);

                // 設定された間隔だけ待機
                yield return new WaitForSeconds(interval);
            }

            // 全ての呪文を飛ばし終えた後、アニメーションが落ち着くまで少し待つ
            yield return new WaitForSeconds(postDropWaitTime);
        }

        // 2. 演出完了後にフェーズ（敵の出現）を開始
        Debug.Log("呪文投入演出が完了しました。敵のフェーズを開始します。");

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

    void OnGameEnd()
    {
        GameTimerManager.Instance.StopTimer(); // タイマーを停止
        WandUIManager.Instance.Hide();
        SpellInventory.Instance.Hide();
    }
    private bool gameEnd = false;
    public bool GameEnd => gameEnd;

    // 報酬の付与
    bool isFirstClear = true;

    [Tooltip("全ステージのリスト設定を持つScriptableObject。")]
    // StageListConfigのインスタンスをインスペクタから設定できるようにする
    public StageListConfig stageListConfig;

    [Tooltip("島とステージの紐づけ情報を持つScriptableObject。")]
    public IslandStageMappingConfig islandStageMapConfig;

    /// <summary>
    /// ステージクリア時の処理を実行します。
    /// </summary>
    public void HandleStageClear()
    {
        if (gameEnd) return;
        gameEnd = true;
        Debug.Log("🎉 ステージクリア！");
        OnGameEnd();
        StartCoroutine(DelayAndPauseGameOnGameClear());

        // 島の最後のステージをクリアしたかチェック
        if (islandStageMapConfig != null)
        {
            if (islandStageMapConfig.IsLastStageOfIsland(stageConfig.stageName, out WandType wandType))
            {
                Debug.Log($"島の最後のステージクリア：杖 {wandType} をアンロックします。");
                WandUnlockManager.Instance.UnlockWand(wandType);
            }
        }

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
                isFirstClear = true;
            }
            else
            {
                Debug.Log($"最新のステージとして '{nextStage.stageName}' を登録しませんでした。");
                isFirstClear = false;
            }
        }
    }
    public static Action OnStageClearForceDie;

    [Header("ステージクリア設定")] // 追記
    [Tooltip("クリア後の演出時間（秒）。この時間後にゲームが停止します。")]
    [SerializeField] private float clearDelaySeconds = 2f; // 例として3.0秒

    [Header("報酬設定")]
    [SerializeField, Tooltip("未クリアステージをクリアした時の報酬額")]
    private int firstClearReward = 100;
    [SerializeField, Tooltip("既クリアステージをクリアした時の報酬額")]
    private int repeatClearReward = 10;

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

    private bool hasUsedContinue = false;

    public void GameOver()
    {
        if (gameEnd) return;

        // 初回のゲームオーバー時のみコンティニュー広告を表示
        if (!hasUsedContinue && AdController.Instance != null)
        {
            hasUsedContinue = true;
            // 広告UI表示中は時間を止める
            Time.timeScale = 0f;
            AdController.Instance.ShowContinueAdUI(
                onReward: () =>
                {
                    Time.timeScale = 1f;
                    PlayerController.Instance.Revive();
                    Debug.Log("広告視聴報酬: プレイヤーを復活させました。");
                },
                onCancel: () =>
                {
                    Time.timeScale = 1f;
                    HandleGameOver();
                }
            );
            return;
        }

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
        SoundManager.Instance.StopBGMWithFade(0.5f);
        PlayerController.Instance.PlayDieSound();
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

            int reward = isFirstClear ? firstClearReward : repeatClearReward;
            if (CurrencyController.Instance != null)
            {
                CurrencyController.Instance.AddCurrency(reward);
            }
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
            if (AdController.Instance != null)
            {
                AdController.Instance.ShowStageEndAd(() => GameManager.Instance.LoadStageSelectScene());
            }
            else
            {
                GameManager.Instance.LoadStageSelectScene();
            }
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
            if (AdController.Instance != null)
            {
                AdController.Instance.ShowStageEndAd(() => GameManager.Instance.LoadStageSelectScene());
            }
            else
            {
                GameManager.Instance.LoadStageSelectScene();
            }
        };
        Action onSpellChange = () =>
        {
            if (clicked) return;
            clicked = true;
            Debug.Log("呪文変更へ");
            EquippedSpellController.Instance?.OpenSpellSelectionUI(() => { clicked = false; controller.UpdateSpellBadge(); });
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
    SpecificBossDefeated,
    // デバッグ用: UIを表示せず即座に開始
    Debug_None
    // 必要であれば他の条件（例: 時間切れ、パズルクリアなど）を追加可能
}