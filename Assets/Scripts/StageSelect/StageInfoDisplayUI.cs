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

    [Header("UI要素 - ボタン")]
    [SerializeField] private Button startButton;           // ステージ開始ボタン
    [SerializeField] private Button openSpellSelectButton; // 持ち込み呪文選択ボタン
    [SerializeField] private Button closeButton;           // 閉じるボタン

    [Header("アニメーター設定")]
    [SerializeField] private Animator rootAnimator;  // UI全体の開閉用 (Open/Close)
    [SerializeField] private Animator frameAnimator; // フレームのスライド用 (Next/Prev)

    public static StageInfoDisplayUI Instance { get; private set; }
    private string currentStageIdentifier;

    private void Awake()
    {
        Instance = this;
        // ボタンのイベントリスナー設定
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);

        if (openSpellSelectButton != null)
            openSpellSelectButton.onClick.AddListener(OnOpenSpellSelectButtonClicked);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        gameObject.SetActive(false);
        close = true;
    }
    StageSelectUI stageSelectUI;
    /// <summary>
    /// ステージ情報をセットして表示を更新します。
    /// </summary>
    public void SetStageInfo(StageSelectUI stageSelectUI, string islandName, string stageSubName, string identifier)
    {
        this.stageSelectUI = stageSelectUI;
        currentStageIdentifier = identifier;

        if (islandNameText != null) islandNameText.text = islandName;
        if (stageNameText != null) stageNameText.text = identifier;
        if (stageSubNameText != null) stageSubNameText.text = stageSubName;
    }

    /// <summary>
    /// UIを表示します。
    /// </summary>
    public void Open()
    {
        if (!close) return;
        close = false;
        gameObject.SetActive(true);
        if (rootAnimator != null)
        {
            rootAnimator.SetTrigger("Open");
            rootAnimator.ResetTrigger("Close");
        }
    }

    [SerializeField] bool close = true;
    /// <summary>
    /// UIを閉じます。
    /// </summary>
    public void Close()
    {
        if (close) return;
        close = true;
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

    bool stageStartClicked = false;
    /// <summary>
    /// ステージ開始ボタンが押された時の処理
    /// </summary>
    private void OnStartButtonClicked()
    {
        if (stageStartClicked) return;
        stageStartClicked = true;
        if (string.IsNullOrEmpty(currentStageIdentifier)) return;

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
            EquippedSpellController.Instance.OpenSpellSelectionUI(() => spellSelectClicked = false);
        }
        else
        {
            Debug.LogError("StageInfoDisplayUI: EquippedSpellController.Instance が見つかりません。");
        }
    }
}