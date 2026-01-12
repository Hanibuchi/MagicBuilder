using System;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

/// <summary>
/// Unity IAP v5 に対応した課金管理クラス（シングルトン）
/// </summary>
public class IAPManager : MonoBehaviour, IDetailedStoreListener
{
    private static IAPManager _instance;
    public static IAPManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // なければ生成
                GameObject go = new GameObject("IAPManager");
                _instance = go.AddComponent<IAPManager>();
            }
            return _instance;
        }
    }

    private IStoreController storeController;
    private IExtensionProvider storeExtensionProvider;

    // 広告削除のプロダクトID（Unity Dashboardで設定したIDに合わせてください）
    public const string REMOVE_ADS = "com.hanitech8686.magicBuilder.removeAds";

    /// <summary>
    /// 広告非表示が購入済みかどうかを返します。
    /// </summary>
    public bool IsAdsRemoved => PlayerPrefs.GetInt("AdsRemoved", 0) == 1;

    private void Awake()
    {
        // シングルトンの設定
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // 初期化を開始
        InitializePurchasing();
    }

    /// <summary>
    /// IAPの初期化を行う
    /// </summary>
    public void InitializePurchasing()
    {
        if (IsInitialized()) return;

        // IAPの設定を構築
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // 広告削除（非消耗品）を登録
        builder.AddProduct(REMOVE_ADS, ProductType.NonConsumable);

        // Unity Purchasingを初期化
        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialized()
    {
        return storeController != null && storeExtensionProvider != null;
    }

    /// <summary>
    /// 広告削除の購入を開始するメソッド
    /// 外部（他のスクリプトなど）から IAPManager.Instance.BuyRemoveAds() で呼び出せます
    /// </summary>
    public void BuyRemoveAds()
    {
        BuyProductID(REMOVE_ADS);
    }

    /// <summary>
    /// 指定された商品IDの購入処理を開始する
    /// </summary>
    private void BuyProductID(string productId)
    {
        if (IsInitialized())
        {
            Product product = storeController.products.WithID(productId);

            if (product != null && product.availableToPurchase)
            {
                Debug.Log($"[IAP] 購入処理開始: {product.definition.id}");
                storeController.InitiatePurchase(product);
            }
            else
            {
                Debug.LogError("[IAP] 商品が見つからないか、購入不可の状態です。");
            }
        }
        else
        {
            Debug.LogError("[IAP] 初期化されていないため、購入を開始できません。");
        }
    }

    // --- IDetailedStoreListener の実装 ---

    /// <summary>
    /// 初期化成功時のコールバック
    /// </summary>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        storeExtensionProvider = extensions;
        Debug.Log("[IAP] 初期化に成功しました。");
    }

    /// <summary>
    /// 初期化失敗時のコールバック（旧形式との互換性のため）
    /// </summary>
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        OnInitializeFailed(error, null);
    }

    /// <summary>
    /// 初期化失敗時のコールバック（詳細メッセージ付き）
    /// </summary>
    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"[IAP] 初期化に失敗しました。理由: {error}. メッセージ: {message}");
    }

    /// <summary>
    /// 購入成功時の処理（レシート検証などを行う場所）
    /// </summary>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        var productId = purchaseEvent.purchasedProduct.definition.id;

        if (productId == REMOVE_ADS)
        {
            Debug.Log("[IAP] 広告削除の購入が完了しました。");
            // 購入成功のフラグを保存
            PlayerPrefs.SetInt("AdsRemoved", 1);
            PlayerPrefs.Save();

            // TODO: ここで実際に広告を消す処理などを呼ぶ
        }

        return PurchaseProcessingResult.Complete;
    }

    /// <summary>
    /// 購入失敗時のコールバック
    /// </summary>
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogWarning($"[IAP] 購入に失敗しました: {product.definition.id}. 理由: {failureReason}");
    }

    /// <summary>
    /// IAP v5 で追加された詳細な購入失敗コールバック
    /// </summary>
    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.LogError($"[IAP] 購入に失敗しました詳細: {product.definition.id}. 理由: {failureDescription.reason}. 詳細: {failureDescription.message}");
    }
}
