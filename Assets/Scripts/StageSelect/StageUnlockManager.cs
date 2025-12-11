using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class StageUnlockManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static StageUnlockManager Instance { get; private set; }

    [Header("設定")]
    [Tooltip("全ステージのリスト設定を持つScriptableObject。")]
    // StageListConfigのインスタンスをインスペクタから設定できるようにする
    public StageListConfig stageListConfig;

    // ステージの解放状態を保持するDictionary
    // Key: ステージ識別子 (string)、Value: 解放されているか (bool)
    private Dictionary<string, bool> unlockedStages = new Dictionary<string, bool>();
    // 永続化のためのキー
    private const string UNLOCK_KEY_PREFIX = "Stage_Unlocked_";
    [SerializeField] string firstStageID = "1-1";

    private void Awake()
    {
        // シングルトンパターンの実装
        if (Instance == null)
        {
            Instance = this;
            // シーン遷移時にオブジェクトが破棄されないようにする
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 既にインスタンスが存在する場合は自身を破棄
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 初回ロード時に保存されている解放状態をロードする
        LoadUnlockStates();

        // 初期ステージを設定（例: "Stage01" は最初から解放されている）
        UnlockStage(firstStageID); // 初期ステージを強制的に解放
    }


    // --- 内部処理と永続化 ---
    /// <summary>
    /// StageListConfigに基づき、PlayerPrefsから全てのステージの解放状態をロードします。
    /// </summary>
    private void LoadUnlockStates()
    {
        if (stageListConfig == null || stageListConfig.stages == null)
        {
            Debug.LogError("LoadUnlockStates: StageListConfig またはその stages 配列が不正です。");
            return;
        }

        unlockedStages.Clear();

        // StageListConfigに登録されている全てのステージ設定を巡回
        foreach (var stageConfig in stageListConfig.stages)
        {
            if (stageConfig == null) continue;

            string stageId = stageConfig.stageName;

            // PlayerPrefsからキーを読み込む。キーが存在しない場合 (0) は未解放とする。
            // 1 が解放済み、0 が未解放
            int isUnlockedInt = PlayerPrefs.GetInt(UNLOCK_KEY_PREFIX + stageId, 0);

            // Dictionaryに状態を格納
            // Keyが存在しない状態でアクセスされることを防ぐため、明示的に設定する
            unlockedStages[stageId] = (isUnlockedInt == 1);

            // ロード状態のログ（デバッグ用）
            Debug.Log($"[Load] Stage: {stageId}, Unlocked: {unlockedStages[stageId]}");
        }
    }

    // --- 公開メソッド ---

    /// <summary>
    /// 指定されたステージが解放されているかを確認します。
    /// </summary>
    /// <param name="stageId">ステージ識別子 (例: "Stage01", "Puzzle03")</param>
    /// <returns>解放されていれば true、そうでなければ false</returns>
    public bool IsStageUnlocked(string stageId)
    {
        // Dictionaryにキーが存在し、かつ値が true であれば解放済み
        return unlockedStages.ContainsKey(stageId) && unlockedStages[stageId];
    }

    /// <summary>
    /// 指定されたステージを解放し、その状態を保存します。
    /// </summary>
    /// <param name="stageId">ステージ識別子</param>
    public void UnlockStage(string stageId)
    {
        if (!unlockedStages.ContainsKey(stageId))
            unlockedStages[stageId] = false;
        if (!unlockedStages[stageId])
        {
            unlockedStages[stageId] = true;
            SaveUnlockState(stageId, true);
            Debug.Log($"ステージ **{stageId}** が解放されました。");
        }
    }

    /// <summary>
    /// ステージの解放状態を PlayerPrefs に保存します。
    /// </summary>
    /// <param name="stageId">ステージ識別子</param>
    /// <param name="isUnlocked">解放状態</param>
    private void SaveUnlockState(string stageId, bool isUnlocked)
    {
        // bool を int に変換して保存 (true: 1, false: 0)
        PlayerPrefs.SetInt(UNLOCK_KEY_PREFIX + stageId, isUnlocked ? 1 : 0);
        PlayerPrefs.Save(); // 変更をディスクに書き込む
    }
}