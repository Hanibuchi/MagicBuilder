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

    private void Start()
    {
        // StageStarterインスタンスを取得
        StageStarter starter = StageStarter.Instance;

        if (starter == null)
        {
            Debug.LogError("StageStarterがシーンに見つかりません。ステージ開始システムが機能しません。");
            return;
        }

        starter.SetStageStartListener(this);

        // GameManagerをStageStarterのリスナーとして登録
        if (GameManager.Instance != null)
        {
            starter.SetStageStartListener(GameManager.Instance);
            Debug.Log("StageSelectController: GameManagerをStageStarterに登録しました。");
        }
        else
        {
            Debug.LogError("GameManagerインスタンスが見つかりません。登録できませんでした。");
        }


        if (SoundManager.Instance != null && bGM != null)
        {
            SoundManager.Instance.PlayBGM(bGM);
        }
    }

    public void OnStageStart(StageConfig config)
    {
        SoundManager.Instance.StopBGMWithFade(0.5f);
    }
}