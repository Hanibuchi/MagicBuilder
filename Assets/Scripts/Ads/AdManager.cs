using UnityEngine;
using UnityEngine.Advertisements;

public class AdManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public static AdManager Instance { get; private set; }

    [SerializeField] private AdSettings settings; // 先ほど作ったScriptableObjectをアサイン
    [SerializeField] private bool testMode = true;

    private string _gameId;
    private string _adUnitId;
    private System.Action _onShowComplete;
    private System.Action _onShowFailed;

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
        _adUnitId = settings.iosAdUnitId;
#elif UNITY_ANDROID
        _gameId = settings.androidGameId;
        _adUnitId = settings.androidAdUnitId;
#else
        _gameId = "unused"; // 他プラットフォーム用
#endif

        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            Advertisement.Initialize(_gameId, testMode, this);
        }
    }

    public void ShowAd(System.Action onComplete = null, System.Action onFailed = null)
    {
        _onShowComplete = onComplete;
        _onShowFailed = onFailed;
        // 広告を表示する前にロードが必要
        Advertisement.Load(_adUnitId, this);
    }

    // --- インターフェースの実装 ---

    public void OnUnityAdsAdLoaded(string placementId)
    {
        // ロード完了後に表示
        if (placementId == _adUnitId)
        {
            Advertisement.Show(_adUnitId, this);
        }
    }

    public void OnInitializationComplete() => Debug.Log("Ads Init Complete");
    public void OnInitializationFailed(UnityAdsInitializationError error, string message) => Debug.LogError($"Init Failed: {message}");
    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"Load Failed: {message}");
        if (placementId == _adUnitId)
        {
            _onShowFailed?.Invoke();
            _onShowFailed = null;
            _onShowComplete = null;
        }
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"Show Failed: {message}");
        if (placementId == _adUnitId)
        {
            _onShowFailed?.Invoke();
            _onShowFailed = null;
            _onShowComplete = null;
        }
    }

    public void OnUnityAdsShowStart(string placementId) { }
    public void OnUnityAdsShowClick(string placementId) { }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (placementId == _adUnitId)
        {
            if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
            {
                Debug.Log("[AdManager] 広告の視聴が完了しました。");
                _onShowComplete?.Invoke();
            }
            else
            {
                Debug.Log($"[AdManager] 広告がスキップまたは中断されました: {showCompletionState}");
                _onShowFailed?.Invoke();
            }

            _onShowComplete = null;
            _onShowFailed = null;
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
}