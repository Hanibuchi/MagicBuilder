using UnityEngine;
using UnityEngine.Advertisements;

public class AdManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public static AdManager Instance { get; private set; }

    [SerializeField] private AdSettings settings; // 先ほど作ったScriptableObjectをアサイン
    [SerializeField] private bool testMode = true;

    private string _gameId;
    private string _interstitialId;
    private string _rewardedId;
    private string _bannerId;

    private System.Action _onRewardedComplete;
    private System.Action _onRewardedFailed;
    private System.Action _onInterstitialComplete;

    private AdNetworkErrorUI _errorUIInstance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAds();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAds()
    {
        // プラットフォームごとにIDを切り替え
#if UNITY_IOS
        _gameId = settings.iosGameId;
        _interstitialId = settings.iosInterstitialId;
        _rewardedId = settings.iosRewardedId;
        _bannerId = settings.iosBannerId;
#elif UNITY_ANDROID
        _gameId = settings.androidGameId;
        _interstitialId = settings.androidInterstitialId;
        _rewardedId = settings.androidRewardedId;
        _bannerId = settings.androidBannerId;
#else
        _gameId = "unused"; // 他プラットフォーム用
#endif

        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            Advertisement.Initialize(_gameId, testMode, this);
        }
    }

    // --- 広告のロード (プレロード用) ---

    public void LoadInterstitial() => Advertisement.Load(_interstitialId, this);
    public void LoadRewarded() => Advertisement.Load(_rewardedId, this);
    public void LoadBanner() => Advertisement.Banner.Load(_bannerId);

    // --- 広告の表示 ---

    /// <summary>
    /// インタースティシャル広告を表示します。
    /// </summary>
    public void ShowInterstitial(System.Action onComplete = null)
    {
        _onInterstitialComplete = onComplete;
        Advertisement.Show(_interstitialId, this);
    }

    /// <summary>
    /// リワード広告を表示します。
    /// </summary>
    public void ShowRewarded(System.Action onComplete = null, System.Action onFailed = null)
    {
        _onRewardedComplete = onComplete;
        _onRewardedFailed = onFailed;
        Advertisement.Show(_rewardedId, this);
    }

    /// <summary>
    /// 以前の共通メソッド(互換用)。リワード広告として動作させます。
    /// </summary>
    public void ShowAd(System.Action onComplete = null, System.Action onFailed = null)
    {
        ShowRewarded(onComplete, onFailed);
    }

    // --- バナー広告の制御 ---

    public void ShowBanner()
    {
        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
        BannerOptions options = new BannerOptions
        {
            showCallback = () => Debug.Log("Banner showing"),
            hideCallback = () => Debug.Log("Banner hidden")
        };
        Advertisement.Banner.Show(_bannerId, options);
    }

    public void HideBanner()
    {
        Advertisement.Banner.Hide();
    }

    private void ShowNetworkErrorUI(string placementId)
    {
        if (_errorUIInstance == null)
        {
            var prefab = Resources.Load<AdNetworkErrorUI>("Ads/AdNetworkErrorUI");
            if (prefab == null) { Debug.LogWarning("AdNetworkErrorUI prefab not found in Resources."); return; }
            _errorUIInstance = Instantiate(prefab);
            DontDestroyOnLoad(_errorUIInstance.gameObject);
        }

        _errorUIInstance.Setup(() =>
        {
            _errorUIInstance.Hide();
            if (placementId == _bannerId)
            {
                LoadBanner();
            }
            else
            {
                Advertisement.Load(placementId, this);
            }
        });
        _errorUIInstance.Show();
    }

    // --- インターフェースの実装 ---

    public void OnInitializationComplete()
    {
        Debug.Log("Ads Init Complete");
        // 初期化完了時にプレロード
        LoadInterstitial();
        LoadRewarded();
        LoadBanner();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message) => Debug.LogError($"Init Failed: {message}");

    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log($"Ad Loaded: {placementId}");
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"Load Failed: {message}");

        _onRewardedFailed?.Invoke();

        ShowNetworkErrorUI(placementId);
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"Show Failed: {message}");
        if (placementId == _rewardedId)
        {
            _onRewardedFailed?.Invoke();
            _onRewardedFailed = null;
            _onRewardedComplete = null;
        }
    }

    public void OnUnityAdsShowStart(string placementId) { }
    public void OnUnityAdsShowClick(string placementId) { }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (placementId == _rewardedId)
        {
            if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
            {
                Debug.Log("[AdManager] リワード広告の視聴が完了しました。");
                _onRewardedComplete?.Invoke();
            }
            else
            {
                Debug.Log($"[AdManager] リワード広告がスキップまたは中断されました: {showCompletionState}");
                _onRewardedFailed?.Invoke();
            }

            _onRewardedComplete = null;
            _onRewardedFailed = null;

            // 次回のためにロード
            LoadRewarded();
        }
        else if (placementId == _interstitialId)
        {
            Debug.Log("[AdManager] インタースティシャル広告の表示が終了しました。");
            _onInterstitialComplete?.Invoke();
            _onInterstitialComplete = null;

            // 次回のためにロード
            LoadInterstitial();
        }
    }

    public void Test()
    {
        AdController.Instance.ShowContinueAdUI(() =>
        {
            Debug.Log("Reward granted from Test.");
        }, () =>
        {
            Debug.Log("Ad cancelled or expired from Test.");
        });
    }

    public void Test2()
    {
        AdController.Instance.ShowRemoveAdsPurchaseUI("200");
    }

    public void Test3()
    {
        AdController.Instance.ShowStageEndAd();
    }

    public void Test4()
    {
        ShowNetworkErrorUI("test");
    }
}