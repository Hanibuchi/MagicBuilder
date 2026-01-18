using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 杖の開放状況を管理するシングルトンクラス。
/// 解放状況と解放演出の完了状況をPlayerPrefsで永続化します。
/// </summary>
public class WandUnlockManager : MonoBehaviour
{
    private static WandUnlockManager _instance;
    public static WandUnlockManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("WandUnlockManager");
                _instance = go.AddComponent<WandUnlockManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private WandDataAsset _wandDataAsset;
    private const string UnlockKeyPrefix = "WandUnlocked_";
    private const string PresentationKeyPrefix = "WandPresentation_";

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // WandTypeとWandを紐づけるScriptableObjectをResourcesからロード
        _wandDataAsset = Resources.Load<WandDataAsset>("WandDataAsset");
        if (_wandDataAsset == null)
        {
            Debug.LogError("Resources/WandDataAsset が配置されていません。WandDataAssetを作成し、Resourcesフォルダに配置してください。");
        }
    }

    /// <summary>
    /// 指定された種類の杖を開放します。
    /// </summary>
    /// <param name="type">開放する杖の種類</param>
    public void UnlockWand(WandType type)
    {
        PlayerPrefs.SetInt(UnlockKeyPrefix + type.ToString(), 1);
        PlayerPrefs.Save();
        // Debug.Log($"Wand Unlocked: {type}");
    }

    /// <summary>
    /// 指定された種類の杖が開放されているかを確認します。
    /// </summary>
    public bool IsWandUnlocked(WandType type)
    {
        // デフォルトの杖は常に開放されているものとする
        if (type == WandType.Default) return true;
        return PlayerPrefs.GetInt(UnlockKeyPrefix + type.ToString(), 0) == 1;
    }

    /// <summary>
    /// 解放されている全てのWandの配列を返します。
    /// </summary>
    /// <returns>解放済みWandの配列</returns>
    public Wand[] GetUnlockedWands()
    {
        if (_wandDataAsset == null) return new Wand[0];

        return _wandDataAsset.wands
            .Where(w => IsWandUnlocked(w.type))
            .Select(w => w.wand)
            .ToArray();
    }

    /// <summary>
    /// 指定された種類の杖の解放演出を行ったことを記録します。
    /// </summary>
    /// <param name="type">演出を完了した杖の種類</param>
    public void MarkPresentationPerformed(WandType type)
    {
        PlayerPrefs.SetInt(PresentationKeyPrefix + type.ToString(), 1);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 指定された種類の杖の解放演出が既に行われたかを確認します。
    /// </summary>
    public bool IsPresentationPerformed(WandType type)
    {
        return PlayerPrefs.GetInt(PresentationKeyPrefix + type.ToString(), 0) == 1;
    }

    /// <summary>
    /// 解放されているが、まだ解放演出を行っていないWandの配列を返します。
    /// </summary>
    /// <returns>未演出の解放済みWandの配列</returns>
    public Wand[] GetPendingPresentationWands()
    {
        if (_wandDataAsset == null) return new Wand[0];

        return _wandDataAsset.wands
            .Where(w => IsWandUnlocked(w.type) && !IsPresentationPerformed(w.type))
            .Select(w => w.wand)
            .ToArray();
    }
}
