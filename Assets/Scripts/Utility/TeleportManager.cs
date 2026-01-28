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
    [SerializeField] private float teleportAnimationDelay = 0.5f;

    [Header("SE設定")]
    [SerializeField] private AudioClip teleportStartSE;
    [SerializeField] private AudioClip teleportArrivalSE;

    [System.Serializable]
    public struct StageTeleportInfo
    {
        public string stageId;          // この場面の敵の識別子（例: "1"）
        public Transform targetTransform; // この場面クリア後のプレイヤー移動先
        public Transform cameraTargetTransform; // この場面クリア後のカメラ移動先
        public Transform cameraLimitA; // この場面クリア後のカメラ制限範囲A
        public Transform cameraLimitB; // この場面クリア後のカメラ制限範囲B
    }

    [Header("場面ごとのテレポート設定")]
    [SerializeField] private List<StageTeleportInfo> stageTeleportInfos = new List<StageTeleportInfo>();

    // 各ステージIDに対応する生存している敵の数
    private Dictionary<string, int> enemyCounts = new Dictionary<string, int>();
    private int currentStageIndex = 0;
    private bool isTeleporting = false;

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
    /// リストの進行状況に応じて次のテレポートまたはクリア判定を行います。
    /// </summary>
    public void UnregisterEnemy(string stageId)
    {
        if (isTeleporting || string.IsNullOrEmpty(stageId)) return;

        if (enemyCounts.ContainsKey(stageId))
        {
            enemyCounts[stageId]--;

            // 現在の進行度（currentStageIndex）に対応する敵グループが全滅したかチェック
            if (currentStageIndex < stageTeleportInfos.Count &&
                stageId == stageTeleportInfos[currentStageIndex].stageId &&
                enemyCounts[stageId] <= 0)
            {
                AdvanceStage();
            }
        }
    }

    private void AdvanceStage()
    {
        // 最後のステージの設定をクリアし終えた場合（＝全ての敵グループを倒した）
        if (currentStageIndex >= stageTeleportInfos.Count - 1)
        {
            isTeleporting = true;
            if (StageManager.Instance != null)
            {
                StageManager.Instance.HandleStageClear();
            }
            return;
        }

        // 進行度を進める
        currentStageIndex++;

        // 次のエリアがある場合、現在の設定（目的地）を使用してテレポートを実行
        var info = stageTeleportInfos[currentStageIndex];
        if (info.targetTransform != null)
        {
            TeleportPlayer(info);
        }
        else
        {
            Debug.LogWarning($"TeleportManager: Index {currentStageIndex} ({info.stageId}) のテレポート先が未設定です。");
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
        isTeleporting = true;

        // 1. プレイヤーのテレポート前アニメーション再生
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.PlayTeleportAnimation();
        }

        if (SoundManager.Instance != null && teleportStartSE != null)
        {
            SoundManager.Instance.PlaySE(teleportStartSE);
        }

        yield return new WaitForSeconds(teleportAnimationDelay);

        // 2. フェードアウト
        if (screenFader != null)
        {
            yield return StartCoroutine(screenFader.FadeOut(fadeDuration));
        }

        if (SoundManager.Instance != null && teleportArrivalSE != null)
        {
            SoundManager.Instance.PlaySE(teleportArrivalSE);
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

        isTeleporting = false;
    }
}
