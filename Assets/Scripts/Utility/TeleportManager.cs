using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// テレポート対象となる敵（または敵グループの管理者）のためのインターフェース。
/// ForceClear は、呼び出された際にそのステージが完了し、次のステージへ移行することを保証する必要があります。
/// </summary>
public interface ITeleportEnemy
{
    void ForceClear(string stageId);
}

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
    [SerializeField] private float preTeleportDelay = 1.0f;

    [Header("SE設定")]
    [SerializeField] private AudioClip waveClearSE;
    [SerializeField] private float waveClearDelay = 0.5f;
    [SerializeField] private AudioClip teleportStartSE;
    [SerializeField] private AudioClip teleportArrivalSE;

    [System.Serializable]
    public struct StageTeleportInfo
    {
        public string stageId;          // この場面の敵の識別子（例: "1"）
        public Transform targetTransform; // この場面クリア後のプレイヤー移動先
        public Transform cameraTargetTransform; // この場面クリア後のカメラ移動先
        public float cameraRelativeSize; // この場面でのカメラの相対サイズ（1.0が標準）
        public Transform cameraLimitA; // この場面クリア後のカメラ制限範囲A
        public Transform cameraLimitB; // この場面クリア後のカメラ制限範囲B
    }

    [Header("場面ごとのテレポート設定")]
    [SerializeField] private List<StageTeleportInfo> stageTeleportInfos = new List<StageTeleportInfo>();

    // 各ステージIDに対応する生存している敵の数
    private Dictionary<string, int> enemyCounts = new Dictionary<string, int>();
    // 各ステージIDに対応する敵のハンドラー（デバッグ用）
    private Dictionary<string, List<ITeleportEnemy>> enemyHandlers = new Dictionary<string, List<ITeleportEnemy>>();

    private int currentStageIndex = 0;

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
    public void RegisterEnemy(string stageId, ITeleportEnemy enemy)
    {
        if (string.IsNullOrEmpty(stageId)) return;

        if (!enemyCounts.ContainsKey(stageId))
        {
            enemyCounts[stageId] = 0;
            enemyHandlers[stageId] = new List<ITeleportEnemy>();
        }
        enemyCounts[stageId]++;

        if (enemy != null && !enemyHandlers[stageId].Contains(enemy))
        {
            enemyHandlers[stageId].Add(enemy);
        }
    }

    /// <summary>
    /// 指定された場面の敵が1体倒されたことを通知します。
    /// リストの進行状況に応じて次のテレポートまたはクリア判定を行います。
    /// </summary>
    public void UnregisterEnemy(string stageId, ITeleportEnemy enemy)
    {
        if (string.IsNullOrEmpty(stageId)) return;

        if (enemyCounts.ContainsKey(stageId))
        {
            enemyCounts[stageId]--;

            if (enemy != null && enemyHandlers.ContainsKey(stageId))
            {
                enemyHandlers[stageId].Remove(enemy);
            }

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


    /// <summary>
    /// 【デバッグ用】指定されたステージインデックスまでスキップし、そこへ移動します。
    /// 各ステージの遷移の間にわずかな待機時間を設けます。
    /// </summary>
    public void DebugSkipToStage(int targetIndex)
    {
        StartCoroutine(DebugSkipToStageRoutine(targetIndex));
    }

    private IEnumerator DebugSkipToStageRoutine(int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= stageTeleportInfos.Count)
        {
            Debug.LogWarning($"TeleportManager: 無効なステージインデックス {targetIndex} です。");
            yield break;
        }

        while (currentStageIndex < targetIndex)
        {
            string sId = stageTeleportInfos[currentStageIndex].stageId;
            if (enemyHandlers.TryGetValue(sId, out var handlers) && handlers.Count > 0)
            {
                // ForceClear 内で UnregisterEnemy が呼ばれリストが変動する可能性があるためコピーを使用
                var copy = new List<ITeleportEnemy>(handlers);
                foreach (var handler in copy)
                {
                    handler.ForceClear(sId);
                }
            }
            else
            {
                // 敵が登録されていない、あるいは既にいない場合は直接次のステージへ
                AdvanceStage();
            }

            // 次のステージのスキップ処理へ移る前に少し待機
            yield return new WaitForSeconds(0.2f);
        }
    }

    /// <summary>
    /// 【デバッグ用】指定されたステージIDまでスキップします。
    /// </summary>
    public void DebugSkipToStage(string stageId)
    {
        int index = stageTeleportInfos.FindIndex(x => x.stageId == stageId);
        if (index != -1)
        {
            DebugSkipToStage(index);
        }
        else
        {
            Debug.LogWarning($"TeleportManager: stageId '{stageId}' が見つかりません。");
        }
    }

    private IEnumerator TeleportSequence(StageTeleportInfo info)
    {
        // 最後の敵を倒してから SE を鳴らすまでの待ち
        yield return new WaitForSeconds(waveClearDelay);

        if (SoundManager.Instance != null && waveClearSE != null)
        {
            SoundManager.Instance.PlaySE(waveClearSE);
        }

        yield return new WaitForSeconds(preTeleportDelay);

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
            PlayerController.Instance.TeleportTo(info.targetTransform.position);
        }

        if (CameraInputHandler.Instance != null && info.cameraTargetTransform != null)
        {
            CameraInputHandler.Instance.UpdateCameraBoundsAndPosition(
                info.cameraTargetTransform.position,
                info.cameraLimitA,
                info.cameraLimitB,
                info.cameraRelativeSize <= 0 ? 1.0f : info.cameraRelativeSize
            );
        }

        // 4. フェードイン
        if (screenFader != null)
        {
            yield return StartCoroutine(screenFader.FadeIn(fadeDuration));
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (stageTeleportInfos == null) return;

        foreach (var info in stageTeleportInfos)
        {
            // プレイヤーの想定位置を緑の球体で表示
            if (info.targetTransform != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(info.targetTransform.position, 0.5f);
            }

            // カメラの移動可能範囲を水色の枠で表示
            if (info.cameraLimitA != null && info.cameraLimitB != null)
            {
                Gizmos.color = Color.cyan;
                Vector3 posA = info.cameraLimitA.position;
                Vector3 posB = info.cameraLimitB.position;
                Vector3 center = (posA + posB) / 2f;
                Vector3 size = new Vector3(Mathf.Abs(posA.x - posB.x), Mathf.Abs(posA.y - posB.y), 0);
                Gizmos.DrawWireCube(center, size);
            }

            // カメラの初期表示範囲を黄色の枠で表示
            if (info.cameraTargetTransform != null)
            {
                Gizmos.color = Color.yellow;
                float relSize = info.cameraRelativeSize <= 0 ? 1.0f : info.cameraRelativeSize;

                // カメラ情報の取得（エディタ上でも可能な限り取得）
                float orthoSize = 5f;
                float aspect = 1.77f; // 16:9

                Camera cam = Camera.main;
                if (cam != null)
                {
                    orthoSize = cam.orthographicSize;
                    aspect = cam.aspect;
                }

                // CameraManagerが動作中ならそちらのデフォルト値を優先
                if (Application.isPlaying && CameraManager.Instance != null)
                {
                    orthoSize = CameraManager.Instance.DefaultOrthographicSize;
                }

                float h = orthoSize * relSize * 2f;
                float w = h * aspect;
                Gizmos.DrawWireCube(info.cameraTargetTransform.position, new Vector3(w, h, 0));
                Gizmos.DrawSphere(info.cameraTargetTransform.position, 0.3f);
            }
        }
    }
}
