using UnityEngine;

[CreateAssetMenu(fileName = "AdSettings", menuName = "Settings/AdSettings")]
public class AdSettings : ScriptableObject
{
    public string iosGameId;
    public string androidGameId;

    [Header("Interstitial")]
    public string iosInterstitialId = "Interstitial_iOS";
    public string androidInterstitialId = "Interstitial_Android";

    [Header("Rewarded")]
    public string iosRewardedId = "Rewarded_iOS";
    public string androidRewardedId = "Rewarded_Android";

    [Header("Banner")]
    public string iosBannerId = "Banner_iOS";
    public string androidBannerId = "Banner_Android";
}