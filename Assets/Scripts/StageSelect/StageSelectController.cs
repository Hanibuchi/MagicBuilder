// StageSelectController.cs
using UnityEngine;
using System.Linq;

/// <summary>
/// ステージ選択画面における操作を担い、GameManagerをStageStarterに登録し、
/// 選択されたステージの開始をStageStarterに要求します。
/// </summary>
public class StageSelectController : MonoBehaviour
{
    private void Start()
    {
        // StageStarterインスタンスを取得
        StageStarter starter = StageStarter.Instance;

        if (starter == null)
        {
            Debug.LogError("StageStarterがシーンに見つかりません。ステージ開始システムが機能しません。");
            return;
        }

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
    }
}