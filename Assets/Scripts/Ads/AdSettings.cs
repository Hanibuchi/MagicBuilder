using UnityEngine;

[CreateAssetMenu(fileName = "AdSettings", menuName = "Settings/AdSettings")]
public class AdSettings : ScriptableObject
{
    public string iosGameId;
    public string androidGameId;
    public string iosAdUnitId;
    public string androidAdUnitId;
}