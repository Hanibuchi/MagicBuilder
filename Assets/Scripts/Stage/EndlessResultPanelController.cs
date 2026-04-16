using UnityEngine;
using System.Linq;

/// <summary>
/// エンドレスモード専用のリザルトパネルコントローラー。
/// 継承元のResultPanelControllerのメソッドをオーバーライドし、
/// エンドレスモードの文脈（生存時間などに合わせた表示やボタン表示）に修正します。
/// </summary>
public class EndlessResultPanelController : ResultPanelController
{
    [System.Serializable]
    public struct ScoreMessageData
    {
        [Tooltip("このスコア以上の場合にメッセージが表示されます。")]
        public int thresholdScore;
        [Tooltip("表示されるメッセージ内容。")]
        public string message;
    }

    [Header("エンドレスモードのメッセージ設定")]
    [SerializeField]
    private ScoreMessageData[] scoreMessages = new ScoreMessageData[]
    {
        new ScoreMessageData { thresholdScore = 10000, message = "神の領域..." },
        new ScoreMessageData { thresholdScore = 5000, message = "伝説の魔法使い！" },
        new ScoreMessageData { thresholdScore = 1000, message = "素晴らしい戦いぶりだ！" },
        new ScoreMessageData { thresholdScore = 0, message = "力尽きた..." }
    };

    protected override void SetResultData(StageResultData data)
    {
        // スコアに応じたメッセージを決定（閾値の降順で判定）
        string selectedMessage = "力尽きた...";
        if (scoreMessages != null && scoreMessages.Length > 0)
        {
            var sortedMessages = scoreMessages.OrderByDescending(m => m.thresholdScore);
            foreach (var sm in sortedMessages)
            {
                if (data.score >= sm.thresholdScore)
                {
                    selectedMessage = sm.message;
                    break;
                }
            }
        }

        // 独自のメッセージに変更
        data.message = selectedMessage;

        // 基底クラスのUI反映を呼ぶ (Textへの反映など)
        base.SetResultData(data);

        if (clearTimeText != null) clearTimeText.text = "生存時間 " + FormatTime(data.clearTimeSeconds); // ※フォーマット変更しても可
    }

    public override void DisplayVictory(StageResultData data)
    {
        Debug.Log("EndlessResultPanelController: エンドレスリザルト表示");
        SetResultData(data);
        UpdateSpellBadge();
        UpdateWandBadge();

        // ここでエンドレスモード固有のUIの表示切り替えを自由に行えます。
        // （例：勝利/敗北オブジェクトを使わず、専用のテキスト等を表示するなど）
        // とりあえず今回は共通基底として「クリア（事実上の勝利だが敗北デザインでもOK）」として扱う

        // baseのDisplayVictoryを使うとネクストステージボタン等が表示されるため、
        // もしエンドレスモード特有の設定に変えたい場合はここを弄ります。
        // 今回の例では一旦基底を呼び出しつつ一部UIを調整します。

        base.DisplayVictory(data);

        // エンドレスモードでは次のステージはないためリトライボタンを表示する
        if (nextStageButton != null) nextStageButton.gameObject.SetActive(false);
        if (retryButton != null) retryButton.gameObject.SetActive(true);
    }
}
