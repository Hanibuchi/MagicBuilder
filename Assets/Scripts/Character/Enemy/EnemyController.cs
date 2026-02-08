using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

/// <summary>
/// 敵の挙動を制御し、センサーからの通知を受け取り、攻撃を管理するクラス。
/// </summary>
public class EnemyController : MyCharacterController, ITriggerHandler, IEnemyAttackExecutor, IKickbackHandler
{
    // --- インスペクタ設定 ---

    [Header("センサー設定")]
    [Tooltip("攻撃や感知に使うLayerSensorコンポーネントの配列")]
    [SerializeField]
    private LayerSensor[] layerSensors;

    [Header("イベント設定")]
    public UnityEvent OnDie;

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

    [Header("表示設定")]
    [Tooltip("キャラクターのメインSpriteRenderer")]
    [SerializeField]
    private SpriteRenderer mainSpriteRenderer;
    [Tooltip("状態異常表示用のSpriteRenderer")]
    [SerializeField]
    private SpriteRenderer statusEffectSpriteRenderer;

    // --- 内部メンバー ---

    private EnemyAttackModel _attackModel;

    /// <summary>最後にセンサーが感知したターゲットのワールド座標</summary>
    private Vector2 _lastSensedTargetPosition = Vector2.zero;

    // --- 反転クールダウン設定 ---
    private float _lastFlipTime = -1f;
    [SerializeField, Tooltip("向きを反転させる際のクールダウン時間")]
    private float flipCooldown = 1.0f;

    // ★ 追加: ノックバック処理のためのEffector
    private KnockbackEffector _knockbackEffector;
    // ★ 追加: Rigidbody2D（ノックバック時間計算に必要）
    private Rigidbody2D _rb2d;

    private Coroutine _kickbackStunCoroutine;

    // ★ 追加: 状態異常の発生数をカウントするフィールド
    private int _activeStatusEffectCount = 0;


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

        // ★ 追加: KnockbackEffectorの取得
        if (TryGetComponent<KnockbackEffector>(out var effector))
        {
            _knockbackEffector = effector;
        }
        else
        {
            Debug.LogWarning("KnockbackEffector component is not attached to the GameObject. Knockback will not be applied.");
        }

        // ★ 追加: Rigidbody2Dの取得
        if (TryGetComponent<Rigidbody2D>(out var rb))
        {
            _rb2d = rb;
        }
        else
        {
            Debug.LogError("Rigidbody2D component is required for Knockback on " + gameObject.name);
        }
    }

    private void Start()
    {
        EnemyCounter.Instance?.AddEnemy();
        StageManager.OnStageClearForceDie += ForceDie;
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

        // --- アニメーションパラメータの更新 ---
        if (animator != null && animator.enabled)
        {
            bool isStunned = _kickbackStunCoroutine != null || (enemyMovement != null && enemyMovement.IsStunned);
            bool isMoving = enemyMovement != null && enemyMovement.IsMoving && !isStunned && !isDead;

            animator.SetBool(PARAM_IS_STUNNED, isStunned);
            animator.SetBool(PARAM_IS_RUNNING, isMoving);
            animator.SetBool(PARAM_IS_IDLE, !isMoving && !isStunned && !isDead);
        }

        // 状態異常中の時のみ、メインのスプライトと同期させる
        if (_activeStatusEffectCount > 0 && statusEffectSpriteRenderer != null && mainSpriteRenderer != null)
        {
            statusEffectSpriteRenderer.sprite = mainSpriteRenderer.sprite;
        }
    }

    /// <summary>
    /// 状態異常表示用のSpriteRendererの表示・非表示を切り替えます。
    /// </summary>
    private void UpdateStatusEffectRenderer()
    {
        if (statusEffectSpriteRenderer != null)
        {
            statusEffectSpriteRenderer.enabled = _activeStatusEffectCount > 0 && !isDead;
        }
    }

    // --- ITriggerHandler の実装 ---
    private bool _isTargetSensed = false;
    private string _currentTriggerID = "";
    private const string TRIGGER_WALL = "Wall";

    /// <summary>
    /// LayerSensorによってトリガーが感知された際に呼び出されます。
    /// </summary>
    /// <param name="triggerID">トリガーを区別するためのID（例: "attack1"）</param>
    /// <param name="target">感知された対象のワールド座標</param>
    public void OnTriggerSensed(string triggerID, Vector2 target)
    {
        // 壁を感知した場合の反転処理
        if (triggerID == TRIGGER_WALL)
        {
            FlipScale();
            return;
        }

        // ターゲット座標をメンバ変数に格納
        _lastSensedTargetPosition = target;

        // ターゲットを感知した状態を記録
        _isTargetSensed = true;
        _currentTriggerID = triggerID;

        // 攻撃モデルに攻撃の発射を試行させる
        // LayerSensorのIDはEnemyAttackModel of AttackData IDと一致している必要があります
        _attackModel.RequestAttack(triggerID);

        enemyMovement?.StopMovement();
    }

    /// <summary>
    /// キャラクターの向き（scale.x）を反転させます。クールダウン中は実行されません。
    /// </summary>
    private void FlipScale()
    {
        if (Time.time < _lastFlipTime + flipCooldown) return;
        _lastFlipTime = Time.time;

        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
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
        enemyMovement?.ApplyStun();

        _activeStatusEffectCount++;
    }

    public override void OnFireStunEnd()
    {
        base.OnFireStunEnd();
        // FireStun終了時は攻撃を再開
        _attackModel.ResumeAttack();
        // 動きも再開
        enemyMovement?.RemoveStun();

        _activeStatusEffectCount--;
    }

    public override void OnFreezeStunStart()
    {
        base.OnFreezeStunStart();
        // FreezeStun開始時は攻撃を停止
        _attackModel.StopAttack();
        // 動きも停止
        enemyMovement?.ApplyStun();

        _activeStatusEffectCount++;
    }

    public override void OnFreezeStunEnd()
    {
        base.OnFreezeStunEnd();
        // FreezeStun終了時は攻撃を再開
        _attackModel.ResumeAttack();
        // 動きも再開。減速（Slow）状態もリセットされる。
        enemyMovement?.RemoveStun();

        _activeStatusEffectCount--;
    }

    public override void OnIceSlowStart()
    {
        base.OnIceSlowStart();
        // 減速状態を移動コンポーネントに適用
        enemyMovement?.ApplyIceSlow();
        // ★ 追加: 攻撃頻度を半分にするため、クールタイムを2.0倍に設定
        _attackModel.SetCooldownMultiplier(2.0f);

        _activeStatusEffectCount++;
    }

    public override void OnIceSlowEnd()
    {
        base.OnIceSlowEnd();
        // 減速状態を解除
        enemyMovement?.RemoveSlow();
        // ★ 追加: クールタイムの倍率をリセット (1.0倍)
        _attackModel.ResetCooldownMultiplier();

        _activeStatusEffectCount--;
    }


    /// <summary>
    /// ノックバックを適用します。
    /// ぶつかってきたオブジェクトからこの敵が受けるノックバック処理をKnockbackEffectorに委譲します。
    /// </summary>
    /// <param name="knockbackValue">ノックバックの強さ</param>
    /// <param name="other">ぶつかってきた放射物（ダメージ源）</param>
    public void ApplyKickback(float knockbackValue, GameObject other)
    {
        if (_knockbackEffector == null || _rb2d == null || knockbackValue <= 0f || _kickbackStunCoroutine != null || other == null) return;

        // 放射物(other)との位置関係を確認
        // 敵のX座標 - 放射物のX座標
        float diffX = transform.position.x - other.transform.position.x;
        
        // 放射物が右側(diffX < 0)なら左上(-1, 1)、左側(diffX >= 0)なら右上(1, 1)へ飛ばす
        Vector2 direction = new Vector2(diffX < 0 ? -1f : 1f, 1f).normalized;
        Vector2 force = direction * knockbackValue;

        enemyMovement?.ApplyStun();

        _knockbackEffector.ApplyKnockback(force);

        // 2. 移動を停止し、ノックバックによるスタン処理を開始

        _kickbackStunCoroutine = StartCoroutine(KickbackStunCoroutine(knockbackValue));
    }

    /// <summary>
    /// ノックバックによるスタン時間を処理するコルーチン。
    /// </summary>
    /// <param name="knockbackValue">ノックバックの強さ（コルーチン内でスタン時間を計算するために使用）</param>
    private IEnumerator KickbackStunCoroutine(float knockbackValue)
    {
        float stunDuration = 1.41421356237f * knockbackValue / (_rb2d.mass * Physics2D.gravity.magnitude);

        // スタン時間の下限・上限を設定しても良い
        // stunDuration = Mathf.Clamp(stunDuration, 0.1f, 2.0f); 

        Debug.Log($"ノックバックによるスタン時間: {stunDuration}秒 (ノックバック値: {knockbackValue}, 質量: {_rb2d.mass})");

        // スタン時間だけ待機
        yield return new WaitForSeconds(stunDuration);

        // スタン終了後、移動を再開
        if (!characterHealth.IsDead)
            enemyMovement?.RemoveStun();
        _kickbackStunCoroutine = null; // コルーチンが完了したことをマーク

        Debug.Log("ノックバックスタン終了。移動を再開します。");
    }

    /// <summary>
    /// ステージクリア時などに外部から強制的に死亡させるために呼び出されます。
    /// </summary>
    public void ForceDie()
    {
        // ノックバックスタン中のコルーチンが実行中であれば停止
        if (_kickbackStunCoroutine != null)
        {
            StopCoroutine(_kickbackStunCoroutine);
            _kickbackStunCoroutine = null;
        }

        // 死亡処理を呼び出す
        // 既に死んでいる場合は、NotifyDie() 内で処理がスキップされることを期待します
        NotifyDie(true);
    }

    bool isDead = false; // ボス敵が複数回死亡通知される可能性があるため、フラグで制御
    public override void NotifyDie(bool silent = false)
    {
        if (isDead) return;
        isDead = true;
        UpdateStatusEffectRenderer();

        enemyMovement?.ApplyStun();
        if (characterHealth != null)
            ScoreManager.Instance?.AddScore(characterHealth.maxHealth);

        base.NotifyDie(silent);

        GetComponent<SpellDropper>()?.DropSpells();
        GetComponent<BossClearNotifier>()?.NotifyDefeated();
        EnemyCounter.Instance?.RemoveEnemy();
        OnDie?.Invoke();
    }

    private void OnDestroy()
    {
        StageManager.OnStageClearForceDie -= ForceDie;
    }
}