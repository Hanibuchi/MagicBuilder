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

    [Header("杖の開放演出")]
    [Tooltip("杖の開放演出を制御するコンポーネントの参照。")]
    [SerializeField] private WandAcquisitionEffect wandAcquisitionEffect;
    [Tooltip("演出終了後、自動選択が行われるまでの待ち時間。")]
    [SerializeField] private float autoSelectDelay = 0.5f;
    // -------------------------

    private const string CAMERA_POS_X_KEY = "StageSelect_CameraPosX";
    private const string CAMERA_POS_Y_KEY = "StageSelect_CameraPosY";

    private void Start()
    {
        // カメラ位置の復元
        RestoreCameraPosition();

        // 必須コンポーネントのチェック
        if (stageSelectUI == null || islandStageMapConfig == null)
        {
            Debug.LogError("StageSelectController: StageSelectUI または IslandStageMappingConfig が設定されていません。");
        }

        // Listenerの登録等、初期セットアップ
        InitializeStageStarter();

        // 杖の開放演出のチェックを開始
        CheckWandReleaseEffects();
    }

    /// <summary>
    /// StageStarterへのリスナー登録を行います。
    /// </summary>
    private void InitializeStageStarter()
    {
        StageStarter starter = StageStarter.Instance;
        if (starter == null)
        {
            Debug.LogError("StageStarterがシーンに見つかりません。ステージ開始システムが機能しません。");
            return;
        }

        starter.SetStageStartListener(this);

        if (GameManager.Instance != null)
        {
            starter.SetStageStartListener(GameManager.Instance);
            Debug.Log("StageSelectController: GameManagerをStageStarterに登録しました。");
        }
        else
        {
            Debug.LogError("GameManagerインスタンスが見つかりません。登録できませんでした。");
        }
    }

    /// <summary>
    /// 未再生の杖開放演出があるか確認し、あれば順次再生します。
    /// 全て終了後、または演出がない場合に、島の自動選択やUI表示などの初期化処理を行います。
    /// </summary>
    private void CheckWandReleaseEffects()
    {
        Wand[] pendingWands = WandUnlockManager.Instance.GetPendingPresentationWands();

        if (pendingWands != null && pendingWands.Length > 0 && wandAcquisitionEffect != null)
        {
            ShowNextWandEffect(pendingWands, 0);
        }
        else
        {
            // 演出がない場合は即座に初期化
            InitializeStageSelectView();
        }
    }

    /// <summary>
    /// 杖の開放演出を表示します（最初の1つのみ）。
    /// 次回この画面に来た際に残りの演出を表示するため、表示した杖のみを完了として記録します。
    /// </summary>
    private void ShowNextWandEffect(Wand[] wands, int index)
    {
        // 最初の1つ目の演出を表示
        Wand currentWand = wands[index];
        wandAcquisitionEffect.gameObject.SetActive(true);
        wandAcquisitionEffect.Setup(
            currentWand.wandSprite,
            currentWand.presentationSprite,
            currentWand.wandName,
            currentWand.description,
            () =>
            {
                // 演出した杖のみを完了として記録
                WandUnlockManager.Instance.MarkPresentationPerformed(currentWand.type);

                // 演出終了後に初期化処理へ
                StartCoroutine(DelayedInitializeStageSelectView());
            }
        );
        wandAcquisitionEffect.StartEffect();
    }

    /// <summary>
    /// 演出終了後、少し待ってから初期化処理を行います。
    /// </summary>
    private System.Collections.IEnumerator DelayedInitializeStageSelectView()
    {
        yield return new WaitForSeconds(autoSelectDelay);
        InitializeStageSelectView();
    }

    /// <summary>
    /// ステージ選択画面の表示状態を初期化します（島の自動選択、通貨表示、BGM再生）。
    /// </summary>
    private void InitializeStageSelectView()
    {
        AutomaticallySelectIsland();

        if (CurrencyUI.Instance != null)
        {
            CurrencyUI.Instance.Show();
        }

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
        SaveCameraPosition();

        if (CurrencyUI.Instance != null)
        {
            CurrencyUI.Instance.Hide();
        }
        SoundManager.Instance.StopBGMWithFade(0.5f);
    }

    /// <summary>
    /// 現在のカメラ位置を保存します。
    /// </summary>
    private void SaveCameraPosition()
    {
        if (CameraManager.Instance != null)
        {
            Vector2 pos = CameraManager.Instance.GetWorldPosition();
            PlayerPrefs.SetFloat(CAMERA_POS_X_KEY, pos.x);
            PlayerPrefs.SetFloat(CAMERA_POS_Y_KEY, pos.y);
            PlayerPrefs.Save();
            Debug.Log($"StageSelectController: カメラ位置を保存しました ({pos.x}, {pos.y})");
        }
    }

    /// <summary>
    /// 保存されたカメラ位置を復元します。
    /// </summary>
    private void RestoreCameraPosition()
    {
        if (PlayerPrefs.HasKey(CAMERA_POS_X_KEY) && PlayerPrefs.HasKey(CAMERA_POS_Y_KEY))
        {
            float x = PlayerPrefs.GetFloat(CAMERA_POS_X_KEY);
            float y = PlayerPrefs.GetFloat(CAMERA_POS_Y_KEY);
            if (CameraManager.Instance != null)
            {
                CameraManager.Instance.SetCameraPosition(new Vector2(x, y));
                Debug.Log($"StageSelectController: カメラ位置を復元しました ({x}, {y})");
            }
        }
    }

    public void Test()
    {
        WandUnlockManager.Instance.UnlockWand(WandType.Fire);
    }
}