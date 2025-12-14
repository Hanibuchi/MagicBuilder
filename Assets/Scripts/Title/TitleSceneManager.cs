using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// タイトルシーンの管理を担当します。
/// ゲーム開始ボタンからステージ選択画面へ遷移させます。
/// </summary>
public class TitleSceneManager : MonoBehaviour
{
    // インスペクタから設定されるゲーム開始ボタン
    [SerializeField] private Button startButton;

    // ボタンがクリックされた後に多重クリックを防ぐためのフラグ
    private bool isTransitioning = false;

    [SerializeField] AudioClip bGM;
    [SerializeField] float bgmStartTime = 1.0f;

    [Header("サウンド設定")]
    [Tooltip("ステージ選択画面へ遷移する時に鳴らすSE")]
    [SerializeField] AudioClip transitionSE;

    void Start()
    {
        // GameManagerの存在を確認（既にDontDestroyOnLoadされているはず）
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManagerが見つかりません。シーン遷移を行う前にGameManagerが正しく初期化されていることを確認してください。");
        }

        // ボタンのリスナーを設定
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
            // 念のため、初期状態ではボタンを有効化
            startButton.interactable = true;
        }
        else
        {
            Debug.LogError("Start Buttonが設定されていません。インスペクタで設定してください。");
        }

        if (SoundManager.Instance != null && bGM != null)
            SoundManager.Instance.PlayBGM(bGM, bgmStartTime);
    }

    /// <summary>
    /// ゲーム開始ボタンがクリックされたときに呼び出されます。
    /// </summary>
    private void OnStartButtonClicked()
    {
        // 既に遷移中の場合は処理を中断し、多重クリックを防ぐ
        if (isTransitioning)
        {
            return;
        }

        // 遷移フラグを立てる
        isTransitioning = true;

        // ボタンのインタラクションを無効にし、多重クリックを視覚的にも防ぐ
        if (startButton != null)
        {
            startButton.interactable = false;
        }

        if (SoundManager.Instance != null && transitionSE != null)
            SoundManager.Instance.PlaySE(transitionSE);
        if (SoundManager.Instance != null && bGM != null)
            SoundManager.Instance.StopBGMWithFade();

        Debug.Log("ゲーム開始ボタンがクリックされました。ステージ選択画面へ遷移します。");

        // GameManagerを介してステージ選択シーンをロード
        // GameManager.cs に LoadStageSelectScene() メソッドが既に存在します。
        GameManager.Instance.LoadStageSelectScene();
    }
}