using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class StageManager : MonoBehaviour, IZeroEnemyNotifier
{
    // --- インスペクタから設定するフィールド ---

    public static StageManager Instance { get; private set; }

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
    /// <summary>
    /// ステージクリア時の処理を実行します。
    /// </summary>
    private void HandleStageClear()
    {
        if (isStageClear) return; // 既にクリア済みの場合は何もしない
        isStageClear = true;
        Debug.Log("🎉 ステージクリア！");
        StartCoroutine(DelayAndPauseGame());
    }

    [Header("ステージクリア設定")] // 追記
    [Tooltip("クリア後の演出時間（秒）。この時間後にゲームが停止します。")]
    [SerializeField] private float clearDelaySeconds = 1f; // 例として3.0秒
    [SerializeField] private float clearDelaySeconds2 = 1f; // 例として3.0秒

    /// <summary>
    /// 指定された秒数だけ待機した後、ゲームを停止します。
    /// </summary>
    private IEnumerator DelayAndPauseGame() // 追記
    {
        Debug.Log($"クリア演出のため {clearDelaySeconds} 秒間待機します...");
        Time.timeScale = 0.5f;

        // 指定された秒数だけ待機
        yield return new WaitForSeconds(clearDelaySeconds);

        Time.timeScale = 1f;

        yield return new WaitForSeconds(clearDelaySeconds2);
        PlayerController.Instance.Victory();
        Debug.Log("ゲームを一時停止しました。");
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