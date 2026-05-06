using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ステージ選択時に詳細情報を表示し、開始や呪文選択への橋渡しを行うUIコントローラー。
/// </summary>
public class StageInfoDisplayUI : MonoBehaviour
{
    [Header("UI要素 - テキスト")]
    [SerializeField] private TextMeshProUGUI islandNameText;      // 島名
    [SerializeField] private TextMeshProUGUI stageNameText;       // ステージ名
    [SerializeField] private TextMeshProUGUI stageSubNameText; // ステージ識別子 (デバッグや内部ID表示用)
    [SerializeField] private TextMeshProUGUI stageTypeText;       // ステージタイプ表示用
    [SerializeField] private TextMeshProUGUI rewardText;          // 報酬表示用

    [Header("UI要素 - ボタン")]
    [SerializeField] private Button startButton;           // ステージ開始ボタン
    [SerializeField] private Button openSpellSelectButton; // 持ち込み呪文選択ボタン
    [SerializeField] private Button openWandSelectButton;  // 持ち込み杖選択ボタン
    [SerializeField] private GameObject spellBadge;        // 新規呪文通知バッジ
    [SerializeField] private GameObject wandBadge;         // 新規杖通知バッジ
    [SerializeField] private Button closeButton;           // 閉じるボタン

    [Header("アニメーター設定")]
    [SerializeField] private Animator rootAnimator;  // UI全体の開閉用 (Open/Close)
    [SerializeField] private Animator frameAnimator; // フレームのスライド用 (Next/Prev)

    public static StageInfoDisplayUI Instance { get; private set; }
    private string currentStageIdentifier;
    public string CurrentStageIdentifier => currentStageIdentifier;

    public Button StartButton => startButton;
    public Button OpenSpellSelectButton => openSpellSelectButton;
    public event System.Action<string> OnUIOpened;
    public event System.Action<string> OnStageInfoSet;

    private void Awake()
    {
        Instance = this;
        // ボタンのイベントリスナー設定
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);

        if (openSpellSelectButton != null)
            openSpellSelectButton.onClick.AddListener(OnOpenSpellSelectButtonClicked);

        if (openWandSelectButton != null)
            openWandSelectButton.onClick.AddListener(OnOpenWandSelectButtonClicked);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        gameObject.SetActive(false);
        close = true;
    }
    StageSelectUI stageSelectUI;
    /// <summary>
    /// ステージ情報をセットして表示を更新します。
    /// </summary>
    public void SetStageInfo(StageSelectUI stageSelectUI, string islandName, string stageSubName, string identifier, StageConfig stageConfig)
    {
        this.stageSelectUI = stageSelectUI;
        currentStageIdentifier = identifier;

        if (islandNameText != null) islandNameText.text = islandName;
        if (stageNameText != null) stageNameText.text = $"ステージ{identifier}";
        if (stageSubNameText != null) stageSubNameText.text = stageSubName;
        if (stageTypeText != null)
        {
            stageTypeText.text = stageConfig.stageType == StageType.Rush ? "ラッシュ" : "パズル";
        }

        if (rewardText != null)
        {
            if (stageConfig.clearCondition == StageClearCondition.Endless)
            {
                rewardText.text = $"スコア×{stageConfig.endlessRewardMultiplier:F1}";
            }
            else
            {
                bool isCleared = StageUnlockManager.Instance.IsStageCleared(identifier);
                int rewardAmt = isCleared ? stageConfig.repeatClearReward : stageConfig.firstClearReward;
                rewardText.text = $"{rewardAmt}";
            }
        }

        bool isRush = stageConfig.stageType == StageType.Rush;
        if (openSpellSelectButton != null) openSpellSelectButton.gameObject.SetActive(isRush);
        if (openWandSelectButton != null) openWandSelectButton.gameObject.SetActive(isRush);

        OnStageInfoSet?.Invoke(identifier);
    }

    /// <summary>
    /// UIを表示します。
    /// </summary>
    public void Open()
    {
        if (!close) return;
        close = false;
        gameObject.SetActive(true);

        UpdateSpellBadge();
        UpdateWandBadge();

        if (rootAnimator != null)
        {
            rootAnimator.SetTrigger("Open");
            rootAnimator.ResetTrigger("Close");
        }

        OnUIOpened?.Invoke(currentStageIdentifier);
    }

    /// <summary>
    /// 新規取得呪文がある場合、バッジを表示します。
    /// </summary>
    private void UpdateSpellBadge()
    {
        if (spellBadge != null)
        {
            bool hasNew = SpellHoldInfoManager.Instance.HasAnyNewlyUnlockedSpells();
            spellBadge.SetActive(hasNew);
        }
    }

    /// <summary>
    /// 新規取得杖がある場合、バッジを表示します。
    /// </summary>
    private void UpdateWandBadge()
    {
        bool hasNew = wandBadge != null && WandUnlockManager.Instance != null ? WandUnlockManager.Instance.HasAnyNewlyUnlockedWands() : false;
        wandBadge.SetActive(hasNew);
    }

    bool close = true;
    [SerializeField] AudioClip closeSound;
    /// <summary>
    /// UIを閉じます。
    /// </summary>
    public void Close()
    {
        if (close) return;
        close = true;

        if (SoundManager.Instance != null && closeSound != null)
            SoundManager.Instance.PlaySE(closeSound);

        if (rootAnimator != null)
        {
            rootAnimator.SetTrigger("Close");
            rootAnimator.ResetTrigger("Open");
        }
        else
        {
            gameObject.SetActive(false);
        }
        stageSelectUI.OnStageInfoDisplayUIClosed();
    }

    public void SetActiveFalse() // アニメーションから呼び出す用。
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 次のステージへの切り替えアニメーションを再生します。
    /// </summary>
    public void PlayNextAnimation()
    {
        if (frameAnimator != null)
            frameAnimator.SetTrigger("Next");
    }

    /// <summary>
    /// 前のステージへの切り替えアニメーションを再生します。
    /// </summary>
    public void PlayPrevAnimation()
    {
        if (frameAnimator != null)
            frameAnimator.SetTrigger("Prev");
    }

    [SerializeField] AudioClip stageStartSound;
    bool stageStartClicked = false;
    /// <summary>
    /// ステージ開始ボタンが押された時の処理
    /// </summary>
    private void OnStartButtonClicked()
    {
        if (stageStartClicked) return;
        stageStartClicked = true;
        if (string.IsNullOrEmpty(currentStageIdentifier)) return;

        if (SoundManager.Instance != null && stageStartSound != null)
            SoundManager.Instance.PlaySE(stageStartSound);

        // StageStarterを利用してステージを開始
        if (StageStarter.Instance != null)
        {
            StageStarter.Instance.StartStageByName(currentStageIdentifier);
            stageSelectUI.OnIslandDeselected();
        }
        else
        {
            Debug.LogError("StageInfoDisplayUI: StageStarter.Instance が見つかりません。");
        }
    }

    bool spellSelectClicked = false;
    /// <summary>
    /// 持ち込み呪文選択ボタンが押された時の処理
    /// </summary>
    private void OnOpenSpellSelectButtonClicked()
    {
        if (spellSelectClicked) return;
        spellSelectClicked = true;

        // 既存の EquippedSpellController を呼び出してUIを開く
        if (EquippedSpellController.Instance != null)
        {
            EquippedSpellController.Instance.OpenSpellSelectionUI(() => { spellSelectClicked = false; UpdateSpellBadge(); });
        }
        else
        {
            Debug.LogError("StageInfoDisplayUI: EquippedSpellController.Instance が見つかりません。");
        }
    }

    bool wandSelectClicked = false;
    /// <summary>
    /// 持ち込み杖選択ボタンが押された時の処理
    /// </summary>
    private void OnOpenWandSelectButtonClicked()
    {
        if (wandSelectClicked) return;
        wandSelectClicked = true;

        if (EquippedWandController.Instance != null)
        {
            EquippedWandController.Instance.OpenWandSelectionUI(() => { wandSelectClicked = false; UpdateWandBadge(); });
        }
        else
        {
            wandSelectClicked = false;
            Debug.LogError("StageInfoDisplayUI: EquippedWandController.Instance が見つかりません。");
        }
    }
}