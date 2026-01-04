using UnityEngine;
using UnityEngine.Advertisements;

public class AdManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] private AdSettings settings; // 先ほど作ったScriptableObjectをアサイン
    [SerializeField] private bool testMode = true;

    private string _gameId;
    private string _adUnitId;

    void Awake()
    {
        InitializeAds();
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

    public void ShowAd()
    {
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
    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message) => Debug.LogError($"Load Failed: {message}");
    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message) => Debug.LogError($"Show Failed: {message}");
    public void OnUnityAdsShowStart(string placementId) { }
    public void OnUnityAdsShowClick(string placementId) { }
    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        // 広告視聴完了後の報酬処理などをここに書く
    }
}