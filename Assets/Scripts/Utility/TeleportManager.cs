using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// プレイヤーのテレポートと、それに伴うアニメーション・フェードを管理するクラス。
/// 敵のグループ（場面）ごとの全滅を検知してテレポートを実行します。
/// </summary>
public class TeleportManager : MonoBehaviour
{
    public static TeleportManager Instance { get; private set; }

    [Header("フェード設定")]
    [SerializeField] private ScreenFader screenFader;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float waitTimeAtDark = 0.2f;

    [System.Serializable]
    public struct StageTeleportInfo
    {
        public string stageId;      // 場面の識別子
        public bool isClearTrigger; // これが最後の敵グループ（ステージクリア）かどうか
        public Transform targetTransform; // プレイヤーの移動先
        public Transform cameraTargetTransform; // カメラの移動先
        public Transform cameraLimitA; // カメラの制限範囲A
        public Transform cameraLimitB; // カメラの制限範囲B
    }

    [Header("場面ごとのテレポート設定")]
    [SerializeField] private List<StageTeleportInfo> stageTeleportInfos = new List<StageTeleportInfo>();

    // 各ステージIDに対応する生存している敵の数
    private Dictionary<string, int> enemyCounts = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 指定された場面の敵を1体登録します。
    /// </summary>
    public void RegisterEnemy(string stageId)
    {
        if (string.IsNullOrEmpty(stageId)) return;

        if (!enemyCounts.ContainsKey(stageId))
        {
            enemyCounts[stageId] = 0;
        }
        enemyCounts[stageId]++;
    }

    /// <summary>
    /// 指定された場面の敵が1体倒されたことを通知します。
    /// カウントが0になった場合、テレポートを開始します。
    /// </summary>
    public void UnregisterEnemy(string stageId)
    {
        if (string.IsNullOrEmpty(stageId)) return;

        if (enemyCounts.ContainsKey(stageId))
        {
            enemyCounts[stageId]--;
            if (enemyCounts[stageId] <= 0)
            {
                TriggerStageTeleport(stageId);
            }
        }
    }

    private void TriggerStageTeleport(string stageId)
    {
        // リストから該当するIDの移動先を探す
        var info = stageTeleportInfos.Find(x => x.stageId == stageId);

        if (string.IsNullOrEmpty(info.stageId))
        {
            Debug.LogWarning($"TeleportManager: Stage ID '{stageId}' が設定に見つかりません。");
            return;
        }

        // ステージクリア判定
        if (info.isClearTrigger)
        {
            if (StageManager.Instance != null)
            {
                StageManager.Instance.HandleStageClear();
            }
            return;
        }

        if (info.targetTransform != null)
        {
            TeleportPlayer(info);
        }
        else
        {
            Debug.LogWarning($"TeleportManager: Stage ID '{stageId}' に対応する Target Transform が設定されていません。");
        }
    }

    /// <summary>
    /// 指定されたステージ情報に基づいてプレイヤーとカメラをテレポートさせます。
    /// </summary>
    public void TeleportPlayer(StageTeleportInfo info)
    {
        StartCoroutine(TeleportSequence(info));
    }

    private IEnumerator TeleportSequence(StageTeleportInfo info)
    {
        // 1. プレイヤーのテレポート前アニメーション再生
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.PlayTeleportAnimation();
        }

        // 2. フェードアウト
        if (screenFader != null)
        {
            yield return StartCoroutine(screenFader.FadeOut(fadeDuration));
        }

        yield return new WaitForSeconds(waitTimeAtDark);

        // 3. プレイヤーとカメラの移動
        if (PlayerController.Instance != null && info.targetTransform != null)
        {
            PlayerController.Instance.transform.position = info.targetTransform.position;
        }

        if (CameraInputHandler.Instance != null && info.cameraTargetTransform != null)
        {
            CameraInputHandler.Instance.UpdateCameraBoundsAndPosition(
                info.cameraTargetTransform.position,
                info.cameraLimitA,
                info.cameraLimitB
            );
        }

        // 4. フェードイン
        if (screenFader != null)
        {
            yield return StartCoroutine(screenFader.FadeIn(fadeDuration));
        }
    }
}
