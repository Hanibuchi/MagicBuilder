using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 敵の挙動を制御し、センサーからの通知を受け取り、攻撃を管理するクラス。
/// </summary>
public class EnemyController : CharacterController, ITriggerHandler, IEnemyAttackExecutor
{
    // --- インスペクタ設定 ---

    [Header("センサー設定")]
    [Tooltip("攻撃や感知に使うLayerSensorコンポーネントの配列")]
    [SerializeField]
    private LayerSensor[] layerSensors;

    [Header("攻撃モデル設定")]
    [Tooltip("この敵が持つ攻撃のデータ配列")]
    [SerializeField]
    private AttackData[] attackDatas;

    [SerializeField]
    EnemyMovementBase enemyMovement;

    [Header("攻撃発射設定")]
    [Tooltip("各攻撃IDに対応するAttackLauncher。AttackLauncherのIDと配列のインデックスは一致している必要はない。")]
    [SerializeField]
    private AttackLauncher attackLauncher1;
    [SerializeField]
    private AttackLauncher attackLauncher2;
    [SerializeField]
    private AttackLauncher attackLauncher3;

    // --- 内部メンバー ---

    private EnemyAttackModel _attackModel;

    /// <summary>最後にセンサーが感知したターゲットのワールド座標</summary>
    private Vector2 _lastSensedTargetPosition = Vector2.zero;

    // --- Unity イベント関数 ---

    protected override void Awake()
    {
        // CharacterController.Awakeを呼び出し、基本コンポーネントを取得
        base.Awake();

        // EnemyAttackModelの初期化
        _attackModel = new EnemyAttackModel(this);

        // 攻撃データの登録
        foreach (var data in attackDatas)
        {
            _attackModel.RegisterAttack(data);
        }

        // LayerSensorへのハンドラー設定
        if (layerSensors != null)
        {
            foreach (var sensor in layerSensors)
            {
                if (sensor != null)
                {
                    sensor.SetHandler(this);
                }
            }
        }
        else
        {
            Debug.LogWarning($"LayerSensors array is null or empty on {gameObject.name}");
        }
        if (TryGetComponent<EnemyMovementBase>(out var movement))
        {
            enemyMovement = movement;
        }
        else
        {
            Debug.LogError("EnemyMovementBase component is not attached to the GameObject.");
        }
    }

    protected override void Update()
    {
        // CharacterController.Updateを呼び出し、ステータス効果を更新
        base.Update();

        if (_isTargetSensed)
        {
            // クールダウンが終わるたびに、移動停止状態でも攻撃を再試行する
            _attackModel.RequestAttack(_currentTriggerID);
        }

        // 攻撃モデルのクールタイム・実行時間を更新
        _attackModel.Update(Time.deltaTime);
    }

    // --- ITriggerHandler の実装 ---
    private bool _isTargetSensed = false;
    private string _currentTriggerID = "";

    /// <summary>
    /// LayerSensorによってトリガーが感知された際に呼び出されます。
    /// </summary>
    /// <param name="triggerID">トリガーを区別するためのID（例: "attack1"）</param>
    /// <param name="target">感知された対象のワールド座標</param>
    public void OnTriggerSensed(string triggerID, Vector2 target)
    {
        // ターゲット座標をメンバ変数に格納
        _lastSensedTargetPosition = target;

        // ターゲットを感知した状態を記録
        _isTargetSensed = true;
        _currentTriggerID = triggerID;

        // 攻撃モデルに攻撃の発射を試行させる
        // LayerSensorのIDはEnemyAttackModelのAttackData IDと一致している必要があります
        _attackModel.RequestAttack(triggerID);

        enemyMovement?.StopMovement();
    }

    /// <summary>
    /// LayerSensorからトリガー対象が離脱した際に呼び出されます。（OnTriggerExit2Dに対応）
    /// </summary>
    /// <param name="triggerID">離脱したトリガーのID。</param>
    public void OnTriggerExited(string triggerID)
    {
        // ここに、対象がセンサー範囲から外れた際の処理を実装します。
        // 例: 追跡や攻撃を停止するロジックを呼び出すなど。
        Debug.Log($"Target exited sensor: {triggerID}");
        _isTargetSensed = false;

        enemyMovement?.ResumeMovement();
    }

    // --- IEnemyAttackExecutor の実装 ---

    /// <summary>
    /// EnemyAttackModelから攻撃の実行を指示された際に呼び出されます。
    /// </summary>
    /// <param name="attackId">攻撃を識別するための文字列</param>
    public void ExecuteAttack(string attackId)
    {
        // アニメーションのトリガーを設定して攻撃開始アニメーションを再生
        // アニメーション内で、後述のAttackX()メソッドをイベントとして呼び出す
        if (animator != null && animator.enabled)
        {
            // IDを直接トリガー名として使用する設計を想定
            // 例: attackId="attack1" -> animator.SetTrigger("attack1")
            animator.SetTrigger(attackId);
        }
    }

    // --- アニメーションイベントから呼び出すメソッド ---

    /// <summary>
    /// アニメーションイベントから呼び出される最初の攻撃の発射メソッド。
    /// </summary>
    public void Attack1()
    {
        if (attackLauncher1 != null)
        {
            // 最後に感知したターゲット座標に向かって発射
            attackLauncher1.LaunchAttack(_lastSensedTargetPosition);
        }
        else
        {
            Debug.LogWarning("AttackLauncher1が設定されていません。");
        }
    }

    /// <summary>
    /// アニメーションイベントから呼び出される2番目の攻撃の発射メソッド。
    /// </summary>
    public void Attack2()
    {
        if (attackLauncher2 != null)
        {
            attackLauncher2.LaunchAttack(_lastSensedTargetPosition);
        }
        else
        {
            Debug.LogWarning("AttackLauncher2が設定されていません。");
        }
    }

    /// <summary>
    /// アニメーションイベントから呼び出される3番目の攻撃の発射メソッド。
    /// </summary>
    public void Attack3()
    {
        if (attackLauncher3 != null)
        {
            attackLauncher3.LaunchAttack(_lastSensedTargetPosition);
        }
        else
        {
            Debug.LogWarning("AttackLauncher3が設定されていません。");
        }
    }

    public override void OnFireStunStart()
    {
        base.OnFireStunStart();
        // FireStun開始時は攻撃を停止
        _attackModel.StopAttack();
        // 動きも停止
        enemyMovement?.ApplyFireStun();
    }

    public override void OnFireStunEnd()
    {
        base.OnFireStunEnd();
        // FireStun終了時は攻撃を再開
        _attackModel.ResumeAttack();
        // 動きも再開
        enemyMovement?.ResumeMovement();
    }

    public override void OnFreezeStunStart()
    {
        base.OnFreezeStunStart();
        // FreezeStun開始時は攻撃を停止
        _attackModel.StopAttack();
        // 動きも停止
        enemyMovement?.ApplyFreezeStun();
    }

    public override void OnFreezeStunEnd()
    {
        base.OnFreezeStunEnd();
        // FreezeStun終了時は攻撃を再開
        _attackModel.ResumeAttack();
        // 動きも再開。減速（Slow）状態もリセットされる。
        enemyMovement?.ResumeMovement();
    }

    public override void OnIceSlowStart()
    {
        base.OnIceSlowStart();
        // 減速状態を移動コンポーネントに適用
        enemyMovement?.ApplyIceSlow();
        // ★ 追加: 攻撃頻度を半分にするため、クールタイムを2.0倍に設定
        _attackModel.SetCooldownMultiplier(2.0f);
    }

    public override void OnIceSlowEnd()
    {
        base.OnIceSlowEnd();
        // 減速状態を解除
        enemyMovement?.ResumeMovement();
        // ★ 追加: クールタイムの倍率をリセット (1.0倍)
        _attackModel.ResetCooldownMultiplier();
    }
}