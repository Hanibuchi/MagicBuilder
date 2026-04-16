using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq; // Added for FirstOrDefault
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

/// <summary>
/// Unity IAP v5 に対応した課金管理クラス（シングルトン）
/// </summary>
public class IAPManager : MonoBehaviour
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

    private StoreController storeController;

    // 広告削除のプロダクトID
    public const string REMOVE_ADS = "com.hanitech8686.magicBuilder.removeAds";
    // コーヒーをおごるのプロダクトID
    public const string BUY_COFFEE = "com.hanitech8686.magicBuilder.buyCoffee";

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
        InitializePurchasing();
    }

    /// <summary>
    /// IAPの初期化を行う
    /// </summary>
    public async void InitializePurchasing()
    {
        if (IsInitialized()) return;

        try
        {
            // ストアコントローラーの取得
            storeController = UnityIAPServices.StoreController();

            // イベントハンドラの登録
            storeController.OnPurchaseFailed += OnPurchaseFailed;
            storeController.OnPurchasePending += OnPurchasePending;
            storeController.OnStoreDisconnected += OnStoreDisconnected;
            storeController.OnProductsFetched += OnProductsFetched;
            storeController.OnPurchasesFetched += OnPurchasesFetched;
            
            // ストアへの接続
            await storeController.Connect();

            // 商品定義
            var initialProductsToFetch = new List<ProductDefinition>
            {
                new ProductDefinition(REMOVE_ADS, ProductType.NonConsumable),
                new ProductDefinition(BUY_COFFEE, ProductType.Consumable)
            };

            // 商品情報の取得
            storeController.FetchProducts(initialProductsToFetch);
        }
        catch (Exception e)
        {
            Debug.LogError($"[IAP] 初期化中にエラーが発生しました: {e}");
        }
    }

    private bool IsInitialized()
    {
        return storeController != null;
    }

    /// <summary>
    /// 広告削除の購入を開始するメソッド
    /// </summary>
    public void BuyRemoveAds()
    {
        BuyProductID(REMOVE_ADS);
    }

    /// <summary>
    /// コーヒーをおごる購入を開始するメソッド
    /// </summary>
    public void BuyCoffee()
    {
        BuyProductID(BUY_COFFEE);
    }

    /// <summary>
    /// 指定された商品IDのローカライズされた価格文字列（通貨記号付き）を取得する
    /// </summary>
    public string GetProductPriceString(string productId)
    {
        if (IsInitialized())
        {
            var products = storeController.GetProducts();
            var product = products?.FirstOrDefault(p => p.definition.id == productId);
            if (product != null)
            {
                return product.metadata.localizedPriceString;
            }
        }
        return ""; // 未初期化・取得不可の場合
    }

    /// <summary>
    /// 指定された商品IDの購入処理を開始する
    /// </summary>
    private void BuyProductID(string productId)
    {
        if (IsInitialized())
        {
            // 商品を取得
            // IAP v5 では GetProducts() がリストを返す
            // もしくは GetProduct(string productId) があると想定
            // 確実なのは GetProducts() から探すこと
            var products = storeController.GetProducts();
            Product product = null;
            if (products != null)
            {
               product = products.FirstOrDefault(p => p.definition.id == productId);
            }

            if (product != null && product.availableToPurchase)
            {
                Debug.Log($"[IAP] 購入処理開始: {product.definition.id}");
                // PurchaseProduct を使用
                storeController.PurchaseProduct(product);
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

    // --- イベントハンドラ ---

    private void OnProductsFetched(List<Product> products)
    {
        Debug.Log($"[IAP] {products.Count} 件の商品情報を取得しました。");
        // 商品取得後に購入済み情報を取得（リストアなど）
        storeController.FetchPurchases();
    }

    private void OnPurchasesFetched(Orders orders)
    {
        // 注文履歴が取得できたときの処理
        // orders.AllMap などでアクセスできる可能性があるが、API仕様に合わせて実装
        Debug.Log($"[IAP] 購入履歴を取得しました。");
    }

    private void OnStoreDisconnected(StoreConnectionFailureDescription failureDescription)
    {
        Debug.LogError($"[IAP] ストアとの接続が切断されました: {failureDescription}");
    }

    private void OnPurchasePending(PendingOrder pendingOrder)
    {
        var product = pendingOrder.CartOrdered.Items().FirstOrDefault()?.Product;
        string productId = product?.definition.id;

        Debug.Log($"[IAP] OnPurchasePending: {productId}");
        
        if (productId == REMOVE_ADS)
        {
            // 購入成功のフラグを保存
            PlayerPrefs.SetInt("AdsRemoved", 1);
            PlayerPrefs.Save();
        }
        else if (productId == BUY_COFFEE)
        {
            Debug.Log("[IAP] コーヒーをおごるの購入が完了しました。ありがとうございます！");
        }

        // コンファーム（確定）処理
        storeController.ConfirmPurchase(pendingOrder);
    }

    private void OnPurchaseFailed(FailedOrder failedOrder)
    {
        Debug.LogError($"[IAP] 購入に失敗しました: {failedOrder}");
    }
}
