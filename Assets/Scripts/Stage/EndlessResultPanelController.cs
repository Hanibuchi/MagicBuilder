using UnityEngine;

/// <summary>
/// エンドレスモード専用のリザルトパネルコントローラー。
/// 継承元のResultPanelControllerのメソッドをオーバーライドし、
/// エンドレスモードの文脈（生存時間などに合わせた表示やボタン表示）に修正します。
/// </summary>
public class EndlessResultPanelController : ResultPanelController
{
    private const string ENDLESS_MESSAGE = "力尽きた...";

    protected override void SetResultData(StageResultData data)
    {
        // 独自のメッセージに変更
        data.message = ENDLESS_MESSAGE;

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
    }
}
