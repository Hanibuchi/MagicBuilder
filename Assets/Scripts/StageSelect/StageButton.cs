// StageButton.cs

using TMPro;
using UnityEngine;
using UnityEngine.UI; // Buttonを使用

/// <summary>
/// ステージ選択UIに表示される個々のステージボタンを管理するクラス。
/// クリック時にStageStarterを呼び出します。
/// </summary>
public class StageButton : MonoBehaviour
{
    [Header("UI要素")]
    public TextMeshProUGUI stageNameText; // ステージ名を表示するTextコンポーネント
    public TextMeshProUGUI subStageNameText; // サブステージ名を表示するTextMeshProUGUIコンポーネント
    public Button button; // ボタンコンポーネント

    // 内部で保持するステージ情報
    private string stageIdentifier;

    /// <summary>
    /// ボタンにステージ情報を設定し、OnClickイベントを設定します。
    /// </summary>
    /// <param name="identifier">StageConfigに登録されているステージの識別名。</param>
    /// <param name="subName">UIに表示するサブステージ名。</param>
    public void Setup(string identifier, string displayName, string subName)
    {
        stageIdentifier = identifier;

        // UI表示名の設定
        if (stageNameText != null)
            stageNameText.text = displayName;

        if (subStageNameText != null)
            subStageNameText.text = subName;

        // ボタンクリック時の処理を設定
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnStageSelected);
        }
        else
        {
            Debug.LogError("StageButton: Buttonコンポーネントが設定されていません。", this);
        }
    }

    /// <summary>
    /// ステージボタンがクリックされたときに呼び出されます。
    /// </summary>
    private void OnStageSelected()
    {
        // StageStarterのシングルトンインスタンスを通じてステージを開始
        if (StageStarter.Instance != null)
        {
            Debug.Log($"ステージボタンクリック:  ({stageIdentifier})");
            // StageStarterのメソッドを呼び出す
            StageStarter.Instance.StartStageByName(stageIdentifier);
            // ステージ開始後はステージ選択UIを閉じるなどの処理が必要に応じて追加される
            // 例: StageSelectUI.Instance.NormalizeUI();
        }
        else
        {
            Debug.LogError("StageButton: StageStarter.Instanceが見つかりません！");
        }
    }
}