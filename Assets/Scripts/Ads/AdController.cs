using UnityEngine;
using System;

/// <summary>
/// 広告の表示制御や広告非表示フラグの判定を行うコントローラークラス
/// </summary>
public class AdController : MonoBehaviour
{
    private static AdController _instance;

    /// <summary>
    /// シングルトンインスタンス。存在しない場合は自動生成します。
    /// </summary>
    public static AdController Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("AdController");
                _instance = go.AddComponent<AdController>();
            }
            return _instance;
        }
    }

    [Header("UI References")]
    [SerializeField] private ContinueAdUI continueAdUI;
    [SerializeField] private RemoveAdsPurchaseUI removeAdsPurchaseUI;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// ContinueAdUIを初期化し、表示します。
    /// 広告非表示を購入済みの場合は、広告を表示せずに報酬処理（onReward）を即座に実行します。
    /// </summary>
    /// <param name="onReward">報酬付与時のコールバック</param>
    /// <param name="onCancel">キャンセルまたは時間切れ時のコールバック</param>
    public void ShowContinueAdUI(Action onReward, Action onCancel)
    {
        // プレハブをResourcesからロードして生成（存在しない場合）
        if (continueAdUI == null)
        {
            ContinueAdUI prefab = Resources.Load<ContinueAdUI>("Ads/ContinueAdUI");
            if (prefab != null)
            {
                continueAdUI = Instantiate(prefab);
            }
        }

        if (continueAdUI != null)
        {
            continueAdUI.Init(
                onAdRequested: () =>
                {
                    // IAPManagerを参照して広告非表示かチェック
                    if (IAPManager.Instance.IsAdsRemoved)
                    {
                        Debug.Log("[AdController] 広告非表示が有効なため、広告を表示せずに報酬を付与します。");
                        onReward?.Invoke();
                    }
                    else
                    {
                        // 広告の表示を開始し、完了時に報酬を付与、失敗/スキップ時にキャンセル処理を実行
                        Debug.Log("[AdController] 動画広告の表示をリクエストします。");
                        AdManager.Instance.ShowAd(
                            onComplete: () =>
                            {
                                Debug.Log("[AdController] 動画広告の視聴が完了しました。報酬を付与します。");
                                onReward?.Invoke();
                            },
                            onFailed: () =>
                            {
                                Debug.Log("[AdController] 動画広告がスキップまたは失敗しました。");
                                onCancel?.Invoke();
                            }
                        );
                    }
                },
                onTimeExpired: onCancel
            );
            continueAdUI.Show();
        }
        else
        {
            Debug.LogWarning("[AdController] ContinueAdUIが見つかりません。");
        }
    }

    /// <summary>
    /// RemoveAdsPurchaseUIを初期化し、表示します。
    /// </summary>
    /// <param name="price">表示する価格文字列（例: "200"）</param>
    public void ShowRemoveAdsPurchaseUI(string price = "200")
    {
        if (removeAdsPurchaseUI == null)
        {
            RemoveAdsPurchaseUI prefab = Resources.Load<RemoveAdsPurchaseUI>("Ads/RemoveAdsPurchaseUI");
            if (prefab != null)
            {
                removeAdsPurchaseUI = Instantiate(prefab);
            }
        }

        if (removeAdsPurchaseUI != null)
        {
            removeAdsPurchaseUI.Init(price, () =>
            {
                Debug.Log("[AdController] 広告削除アイテムの購入をリクエストします。");
                IAPManager.Instance.BuyRemoveAds();
            });
            removeAdsPurchaseUI.Show();
        }
        else
        {
            Debug.LogWarning("[AdController] RemoveAdsPurchaseUIが見つかりません。Resources/Ads/RemoveAdsPurchaseUI を確認してください。");
        }
    }

    /// <summary>
    /// ステージ終了時などに呼び出され、通常の広告を表示します。
    /// 広告非表示を購入済みの場合は何も行いません。
    /// </summary>
    public void ShowStageEndAd()
    {
        if (IAPManager.Instance.IsAdsRemoved)
        {
            Debug.Log("[AdController] 広告非表示が有効なため、広告表示をスキップします。");
            return;
        }

        Debug.Log("[AdController] インタースティシャル広告の表示をリクエストします。");
        AdManager.Instance.ShowAd();
    }
}
