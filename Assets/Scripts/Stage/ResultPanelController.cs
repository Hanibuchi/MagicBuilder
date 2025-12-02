using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshProUGUIを使うために必要
using System;
using System.Collections; // コルーチンを使うために必要

public class ResultPanelController : MonoBehaviour
{
    // --- インスペクタから設定するUI要素 ---

    [Header("結果表示オブジェクト")]
    [Tooltip("勝利時にアクティブにするオブジェクト (例: 'Victory'テキスト)")]
    [SerializeField] private GameObject victoryObject;
    [Tooltip("敗北時にアクティブにするオブジェクト (例: 'Fail'テキスト)")]
    [SerializeField] private GameObject failObject;

    [Header("スコア表示要素")]
    [Tooltip("ステージ名を表示するTextMeshProUGUI")]
    [SerializeField] private TextMeshProUGUI stageNameText;
    [Tooltip("ステージサブ名を表示するTextMeshProUGUI")]
    [SerializeField] private TextMeshProUGUI stageSubNameText;
    [Tooltip("スコアを表示するTextMeshProUGUI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [Tooltip("総合得点を表示するTextMeshProUGUI")]
    [SerializeField] private TextMeshProUGUI clearTimeText;
    [Tooltip("メッセージを表示するTextMeshProUGUI")]
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("アニメーションと遅延設定")]
    [Tooltip("スコア表示などを含むアニメーター")]
    [SerializeField] private Animator scoreAnimator;
    [Tooltip("ボタンクリックから実際の処理が実行されるまでの遅延時間（秒）")]
    [SerializeField] private float buttonClickDelay = 0.5f;

    // --- ボタン要素 ---

    [Header("ボタンと遷移先のメソッド")]
    [SerializeField] private Button stageSelectButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button nextStageButton;
    [SerializeField] private Button spellChangeButton;

    // --- 外部から設定するデータ（例：StageManagerから取得を想定） ---

    public struct StageResultData
    {
        public string stageName;
        public string stageSubName;
        public int score;
        public float clearTimeSeconds; // クリアタイムを追加
        public string message;
    }

    private Action onStageSelect;
    private Action onRetry;
    private Action onNextStage;
    private Action onSpellChange;

    // --- 定数 ---
    private const string VICTORY_MESSAGE = "魔法の呪文で世界を救った！"; // 勝利時のメッセージ
    private const string DEFEAT_MESSAGE = "残念！魔物に敗北してしまった..."; // 敗北時のメッセージ

    // --- Unityライフサイクルメソッド ---

    /// <summary>
    /// ボタンに対応する外部メソッドを登録します。
    /// </summary>
    public void SetupActions(Action stageSelect, Action retry, Action nextStage, Action spellChange)
    {
        onStageSelect = stageSelect;
        onRetry = retry;
        onNextStage = nextStage;
        onSpellChange = spellChange;

        // 2. ボタンにメソッドを登録（クリック遅延付き）
        stageSelectButton?.onClick.AddListener(() => StartCoroutine(DelayExecuteAction(onStageSelect)));
        retryButton?.onClick.AddListener(() => StartCoroutine(DelayExecuteAction(onRetry)));
        nextStageButton?.onClick.AddListener(() => StartCoroutine(DelayExecuteAction(onNextStage)));
        spellChangeButton?.onClick.AddListener(() => StartCoroutine(DelayExecuteAction(onSpellChange)));
    }

    /// <summary>
    /// 結果データをUIに設定します。
    /// </summary>
    void SetResultData(StageResultData data)
    {
        if (stageNameText != null) stageNameText.text = data.stageName;
        if (stageSubNameText != null) stageSubNameText.text = data.stageSubName;
        if (scoreText != null) scoreText.text = "スコア " + data.score.ToString("N0"); // 3桁区切り
        if (clearTimeText != null) clearTimeText.text = "クリアタイム " + FormatTime(data.clearTimeSeconds);
        if (messageText != null) messageText.text = data.message;
    }

    /// <summary>
    /// ステージ勝利時の表示設定を行います。
    /// </summary>
    public void DisplayVictory(StageResultData data)
    {
        Debug.Log("ResultPanelController: 勝利表示");
        SetResultData(data);

        // UIアクティブ制御
        if (victoryObject != null) victoryObject.SetActive(true);
        if (failObject != null) failObject.SetActive(false);
        if (nextStageButton != null) nextStageButton.gameObject.SetActive(true);
        if (retryButton != null) retryButton.gameObject.SetActive(false); // リトライは非表示

        // アニメーターのBool設定
        if (scoreAnimator != null)
        {
            scoreAnimator.SetBool("ShowScore", true);
            scoreAnimator.SetBool("ShowClearTime", true); // 勝利時: スコア、クリアタイム、メッセージを表示
            scoreAnimator.SetBool("ShowMessage", true);
        }
    }

    /// <summary>
    /// ステージ敗北時の表示設定を行います。
    /// </summary>
    public void DisplayDefeat(StageResultData data)
    {
        Debug.Log("ResultPanelController: 敗北表示");
        SetResultData(data);

        // UIアクティブ制御
        if (victoryObject != null) victoryObject.SetActive(false);
        if (failObject != null) failObject.SetActive(true);
        if (nextStageButton != null) nextStageButton.gameObject.SetActive(false); // ネクストステージは非表示
        if (retryButton != null) retryButton.gameObject.SetActive(true);

        // アニメーターのBool設定
        if (scoreAnimator != null)
        {
            scoreAnimator.SetBool("ShowScore", true);
            scoreAnimator.SetBool("ShowClearTime", false); // 敗北時: スコア、メッセージを表示 (クリアタイムは非表示)
            scoreAnimator.SetBool("ShowMessage", true);
        }
    }

    /// <summary>
    /// 指定された時間待機してから、アクションを実行します。
    /// </summary>
    private IEnumerator DelayExecuteAction(Action action)
    {
        // 実行前の演出（例：ボタンの非活性化など）
        // 処理が重い場合（例：シーン遷移）にゲームを止めないよう、Time.unscaledDeltaTimeの使用を検討しても良い

        yield return new WaitForSeconds(buttonClickDelay);

        action?.Invoke();
        // ここで、このUIパネル自体を非アクティブにするなどの処理を追加することもできます
    }

    // 必要に応じて、時間を "分:秒.ミリ秒" 形式に整形するヘルパーメソッドを追加
    private string FormatTime(float totalSeconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(totalSeconds);

        // 💡 3600秒（1時間）未満の場合
        if (totalSeconds < 3600f)
        {
            return timeSpan.ToString(@"mm\:ss");
        }
        // 💡 3600秒（1時間）以上の場合
        else
        {
            return timeSpan.ToString(@"hh\:mm\:ss");
        }
    }
}