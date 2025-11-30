using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class StageManager : MonoBehaviour
{
    // --- インスペクタから設定するフィールド ---

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
        EnemyPhaseExecutor.Instance.StartPhase(test_phases);
    }

    public EnemyPhaseConfig[] test_phases;
}