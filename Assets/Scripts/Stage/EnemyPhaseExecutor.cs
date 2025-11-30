// EnemyPhaseExecutor.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// EnemyPhaseConfigの木構造を読み込み、敵の出現を制御するクラス。
/// </summary>
public class EnemyPhaseExecutor : MonoBehaviour
{
    public static EnemyPhaseExecutor Instance { get; private set; }

    [Tooltip("敵をスポーンさせる座標")]
    Vector3 spawnPosition = Vector3.zero;

    /// <summary>
    /// シングルトンの初期化処理
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // シーンを跨いで保持したい場合は DontDestroyOnLoad(gameObject); を追加
        }
        else if (Instance != this)
        {
            // 既にインスタンスが存在する場合、自分自身を破棄
            Debug.LogWarning("EnemyPhaseExecutorがシーンに複数存在します。新しいインスタンスを破棄します。");
            Destroy(gameObject);
        }
    }

    public void SetSpawnPoint(Vector3 position)
    {
        spawnPosition = position;
    }

    // 現在実行中のフェーズと、次の処理待ちのフェーズを管理するスタック
    private Stack<EnemyPhaseConfig> phaseStack = new Stack<EnemyPhaseConfig>();

    public void StartPhase(EnemyPhaseConfig[] phases, Action callback = null)
    {
        // 初期フェーズをスタックに追加
        for (int i = phases.Length - 1; i >= 0; i--)
        {
            phaseStack.Push(phases[i]);
        }
        StartCoroutine(ExecutePhases(callback));
    }

    /// <summary>
    /// フェーズを深さ優先探索で実行するコルーチン。
    /// </summary>
    private IEnumerator ExecutePhases(Action callback)
    {
        Debug.Log("EnemyPhaseExecutor: フェーズ実行開始");

        // スタックが空になるまでループ
        while (phaseStack.Count > 0)
        {
            EnemyPhaseConfig currentPhase = phaseStack.Pop();
            if (currentPhase == null)
                continue;

            Debug.Log($"--- New Phase: {currentPhase.name} ---");

            // 1. 条件が満たされるまで待機
            yield return StartCoroutine(WaitForCondition(currentPhase));

            // 2. 敵の生成を実行
            if (currentPhase.spawnerConfig != null)
            {
                currentPhase.spawnerConfig.SpawnEnemy(spawnPosition);
            }

            // 3. 行きがけ順の深さ優先探索のため、次のフェーズをスタックに追加
            // 配列の逆順でPushすることで、スタックからPopする際に行きがけ順になる
            if (currentPhase.nextPhases != null)
            {
                for (int i = currentPhase.nextPhases.Length - 1; i >= 0; i--)
                {
                    phaseStack.Push(currentPhase.nextPhases[i]);
                }
            }
        }

        Debug.Log("EnemyPhaseExecutor: すべてのフェーズの実行が完了しました。");
        callback?.Invoke();
    }

    /// <summary>
    /// 指定されたフェーズの条件が満たされるまで待機します。
    /// </summary>
    private IEnumerator WaitForCondition(EnemyPhaseConfig phase)
    {
        switch (phase.conditionType)
        {
            case EnemyPhaseConfig.PhaseConditionType.TimeElapsed:
                Debug.Log($"条件: {phase.conditionValue} 秒待機...");
                yield return new WaitForSeconds(phase.conditionValue);
                break;

            case EnemyPhaseConfig.PhaseConditionType.None:
                // 即時実行
                yield break;

            // 今後の拡張性のためのプレースホルダー
            // case EnemyPhaseConfig.PhaseConditionType.AllEnemiesDefeated:
            //     while (/* 敵が残っている場合 */)
            //     {
            //         yield return null;
            //     }
            //     break;

            default:
                Debug.LogWarning($"未定義の条件タイプ: {phase.conditionType}");
                yield break;
        }
    }
}