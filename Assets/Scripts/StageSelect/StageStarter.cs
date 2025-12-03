// StageStarter.cs
using UnityEngine;
using System.Linq;

/// <summary>
/// ステージ開始のトリガーとなるクラス。
/// StageListConfigを持ち、指定された名前のステージの開始をIStageStartListenerに通知します。
/// </summary>
public class StageStarter : MonoBehaviour
{
    public static StageStarter Instance { get; private set; }

    [Header("ステージ設定")]
    [Tooltip("インスペクタから全てのステージ情報を持つStageListConfigを渡す。")]
    [SerializeField]
    private StageListConfig stageListConfig;

    // ゲーム開始を通知するためのインスタンス（登録できるのは1つのみ）
    private IStageStartListener stageStartListener;

    private void Awake()
    {
        // シングルトンパターンの実装
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// IStageStartListenerを登録します。登録できるのは一つだけです。
    /// </summary>
    /// <param name="listener">登録するIStageStartListenerインスタンス (通常はGameManager)</param>
    public void SetStageStartListener(IStageStartListener listener)
    {
        if (listener == null)
        {
            Debug.LogError("登録しようとしたStageStartListenerがnullです。");
            return;
        }

        if (stageStartListener != null && stageStartListener != listener)
        {
            Debug.LogWarning("StageStartListenerは既に登録されています。新しいインスタンスに上書きします。");
        }

        stageStartListener = listener;
        Debug.Log($"StageStartListener ({listener.GetType().Name}) が登録されました。");
    }

    public StageConfig GetStageInfoByName(string stageName)
    {
        if (stageListConfig == null || stageListConfig.stages == null)
        {
            Debug.LogError("StageListConfigが設定されていないか、ステージリストが空です。");
            return null;
        }

        return stageListConfig.GetStageInfoByName(stageName);
    }

    /// <summary>
    /// 識別用の名前を引数に持ち、ゲーム開始のトリガーとなるメソッド。
    /// </summary>
    /// <param name="stageName">開始したいステージのStageConfigに設定された名前</param>
    public void StartStageByName(string stageName)
    {
        if (stageStartListener == null)
        {
            Debug.LogError("ステージ開始通知用のリスナー (IStageStartListener) が登録されていません！");
            return;
        }

        if (stageListConfig == null || stageListConfig.stages == null)
        {
            Debug.LogError("StageListConfigが設定されていないか、ステージリストが空です。");
            return;
        }

        // 1. 名前でStageConfigを探す
        StageConfig targetConfig = stageListConfig.GetStageInfoByName(stageName);

        if (targetConfig == null)
        {
            Debug.LogError($"指定されたステージ名 '{stageName}' に一致するStageConfigが見つかりませんでした。");
            return;
        }

        // 2. リスナーに通知
        Debug.Log($"ステージ '{stageName}' の開始を通知します。");
        stageStartListener.OnStageStart(targetConfig);
    }


    // public string test_StageName;
    // public void Test()
    // {
    //     StartStageByName(test_StageName);
    // }
}

// IStageStartListener.cs
/// <summary>
/// ステージ開始の通知を受け取るためのインターフェース。
/// </summary>
public interface IStageStartListener
{
    /// <summary>
    /// 指定されたステージコンフィグでゲーム開始処理を実行します。
    /// </summary>
    /// <param name="config">開始するステージのStageConfig</param>
    void OnStageStart(StageConfig config);
}

