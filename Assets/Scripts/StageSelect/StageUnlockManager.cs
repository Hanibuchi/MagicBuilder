using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;

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
    private const string LATEST_STAGE_KEY = "Latest_Reached_Stage_ID";
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

    /// <summary>
    /// PlayerPrefsに保存されている、プレイヤーが到達した最新のステージ識別子を取得します。
    /// 保存されていない場合は、初期ステージIDを返します。
    /// </summary>
    /// <returns>最新のステージ識別子</returns>
    public string GetLatestReachedStageID()
    {
        // PlayerPrefs.GetString(キー, デフォルト値)
        // データがない場合は firstStageID を返します。
        return PlayerPrefs.GetString(LATEST_STAGE_KEY, firstStageID);
    }

    /// <summary>
    /// 現在のステージ識別子と、保存されている最新のステージ識別子を比較し、
    /// StageListConfigの登録順に基づき、より新しいステージであれば更新して保存します。
    /// </summary>
    /// <param name="stageId">チェックするステージ識別子</param>
    /// <returns>最新ステージIDの更新に成功した場合 true、失敗した場合 false</returns>
    public bool UpdateLatestReachedStage(string stageId) // 戻り値を bool に変更
    {
        // 1. 基本的な無効チェック
        if (string.IsNullOrEmpty(stageId))
        {
            Debug.LogWarning("ステージIDが無効なため、進捗の更新をスキップします。");
            return false; // 失敗
        }
        if (stageListConfig == null || stageListConfig.stages == null)
        {
            Debug.LogError("StageListConfigが設定されていないか、ステージリストが空です。進捗を比較できません。");
            return false; // 失敗
        }

        string currentLatestId = GetLatestReachedStageID();

        // 2. StageListConfigに登録されている全ステージ名のリストを作成
        List<string> orderedStageNames = stageListConfig.stages
            .Where(config => config != null)
            .Select(config => config.stageName)
            .ToList();

        // 3. 比較対象のステージ名がリストに存在するか確認
        if (!orderedStageNames.Contains(stageId))
        {
            Debug.LogWarning($"ステージID '{stageId}' はStageListConfigに登録されていません。進捗の更新をスキップします。");
            return false; // 失敗
        }

        // 4. 進捗の比較

        // 保存されていた最新IDがConfigに存在しない場合、渡されたIDを最新とする（強制更新）
        if (!orderedStageNames.Contains(currentLatestId))
        {
            Debug.LogWarning($"保存されていた最新ステージID '{currentLatestId}' がConfigに存在しないため、'{stageId}' で強制的に更新します。");
            // 比較せずにそのまま更新フェーズへ進む
        }
        else
        {
            // 登録順に基づきインデックスを取得し、比較する
            int newStageIndex = orderedStageNames.IndexOf(stageId);
            int currentLatestIndex = orderedStageNames.IndexOf(currentLatestId);

            // 新しいステージのインデックスが、現在の最新ステージのインデックス以下の場合
            // (既に到達済みか、それよりも前のステージであるため、更新しない)
            if (newStageIndex <= currentLatestIndex)
                // 例: 最新が "1-5" で、渡されたのが "1-3" の場合
                return false; // 失敗 (更新の必要なし)
        }

        // 5. 更新と保存 (成功)
        PlayerPrefs.SetString(LATEST_STAGE_KEY, stageId);
        PlayerPrefs.Save();
        Debug.Log($"最新到達ステージIDが **{stageId}** に更新されました。");
        return true; // 成功
    }
}