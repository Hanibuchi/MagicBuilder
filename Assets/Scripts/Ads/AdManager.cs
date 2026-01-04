using UnityEngine;
using UnityEngine.Advertisements;

public class AdManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    private string gameId;
    private string adUnitId;

    void Start()
    {
#if UNITY_IOS
        gameId = "YourGameID";
        adUnitId = "YourGameID";
#elif UNITY_ANDROID
        gameId = "YourGameID";
        adUnitId = "YourGameID";
#endif

        Advertisement.Initialize(gameId, true, this);
    }

    public void ShowAd()
    {
        Advertisement.Load(adUnitId, this);
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"Unity Ads Initialization Failed: {error} - {message}");
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        if (placementId == adUnitId)
        {
            Advertisement.Show(adUnitId, this);
        }
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"Failed to load Ad Unit {placementId}: {error} - {message}");
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"Failed to show Ad Unit {placementId}: {error} - {message}");
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Debug.Log("Started showing Ad Unit: " + placementId);
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("Clicked Ad Unit: " + placementId);
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log("Completed showing Ad Unit: " + placementId);
    }
}