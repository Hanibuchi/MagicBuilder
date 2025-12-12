// StageButton.cs

using TMPro;
using UnityEngine;
using UnityEngine.UI; 

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
    [Tooltip("新しく解放されたステージであることを示すUI要素 (例: 'NEW!'マーク)")]
    [SerializeField] // 新規追加
    private GameObject newStageIndicator; // 新ステージであることを示すUI要素

    // 内部で保持するステージ情報
    private string stageIdentifier;
    StageSelectUI stageSelectUI;

    /// <summary>
    /// ボタンにステージ情報を設定し、OnClickイベントを設定します。
    /// </summary>
    /// <param name="identifier">StageConfigに登録されているステージの識別名。</param>
    /// <param name="subName">UIに表示するサブステージ名。</param>
    public void Setup(StageSelectUI stageSelectUI, string identifier, string displayName, string subName)
    {
        this.stageSelectUI = stageSelectUI;
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

        // 初期状態としてインジケータは非表示にしておく
        if (newStageIndicator != null) // 新規追加
            newStageIndicator.SetActive(false); // 新規追加
    }

    [SerializeField] AudioClip clickSound;
    [SerializeField] float clickSoundVolume = 1.0f;
    bool clicked = false;
    /// <summary>
    /// ステージボタンがクリックされたときに呼び出されます。
    /// </summary>
    private void OnStageSelected()
    {
        if (clicked) return;
        clicked = true;

        // StageStarterのシングルトンインスタンスを通じてステージを開始
        if (StageStarter.Instance != null)
        {
            if (SoundManager.Instance != null && clickSound != null)
                SoundManager.Instance.PlaySE(clickSound, clickSoundVolume);

            Debug.Log($"ステージボタンクリック:  ({stageIdentifier})");
            // StageStarterのメソッドを呼び出す
            StageStarter.Instance.StartStageByName(stageIdentifier);
            // ステージ開始後はステージ選択UIを閉じるなどの処理が必要に応じて追加される
            stageSelectUI.OnIslandDeselected();
        }
        else
        {
            Debug.LogError("StageButton: StageStarter.Instanceが見つかりません！");
        }
    }

    // --- 新規追加メソッド ---

    /// <summary>
    /// ボタンを無効化し、クリックできないようにします。
    /// </summary>
    public void DisableButton() // 新規追加
    {
        if (button != null)
        {
            button.interactable = false;
            // 視覚的に無効であることが分かるような処理をここに追加することもできます
            // 例: ボタンのColorBlockを変更する
        }
    }

    /// <summary>
    /// ボタンを有効化し、クリックできるようにします。
    /// </summary>
    public void EnableButton() // 新規追加
    {
        if (button != null)
        {
            button.interactable = true;
        }
    }

    /// <summary>
    /// 新しく解放されたことを示すインジケータを表示します。
    /// </summary>
    public void ShowNewStageIndicator() // 新規追加
    {
        if (newStageIndicator != null)
        {
            newStageIndicator.SetActive(true);
        }
    }

    /// <summary>
    /// 新しく解放されたことを示すインジケータを非表示にします。
    /// </summary>
    public void HideNewStageIndicator() // 新規追加
    {
        if (newStageIndicator != null)
        {
            newStageIndicator.SetActive(false);
        }
    }
}