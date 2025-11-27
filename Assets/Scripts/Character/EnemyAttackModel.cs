using System.Collections.Generic;
using UnityEngine; // MonoBehaviourを継承しないため不要ですが、Unityプロジェクトを想定して。

/// <summary>
/// 敵の攻撃パターンとクールタイムを制御するモデルクラス
/// </summary>
public class EnemyAttackModel
{
    // --- 内部データ構造 ---

    /// <summary>
    /// 攻撃の種類ごとの設定データ
    /// </summary>
    private readonly Dictionary<string, AttackData> _attackSettings;

    /// <summary>
    /// 攻撃の種類ごとの残りクールタイム
    /// </summary>
    private readonly Dictionary<string, float> _remainingCoolDowns;

    /// <summary>
    /// 攻撃の実行ロジックを持つインターフェース
    /// </summary>
    private readonly IEnemyAttackExecutor _executor;

    // --- 状態変数 ---

    /// <summary>現在実行中の攻撃の実行時間（秒）</summary>
    private float _currentExecutionTimer = 0f;

    /// <summary>現在実行中の攻撃の識別子</summary>
    private string _currentExecutingAttackId = null;

    /// <summary>攻撃を停止する状態フラグ（状態異常など）</summary>
    private bool _isStopped = false;

    // --- プロパティ ---

    /// <summary>現在攻撃の実行中か？</summary>
    public bool IsExecutingAttack => _currentExecutionTimer > 0f;

    // --- コンストラクタ ---

    /// <summary>
    /// EnemyAttackModelの新しいインスタンスを初期化する
    /// </summary>
    /// <param name="executor">攻撃実行のためのインターフェース実装</param>
    public EnemyAttackModel(IEnemyAttackExecutor executor)
    {
        if (executor == null)
        {
            throw new System.ArgumentNullException(nameof(executor), "Executor must not be null.");
        }
        _executor = executor;
        _attackSettings = new Dictionary<string, AttackData>();
        _remainingCoolDowns = new Dictionary<string, float>();
    }

    // --- 公開メソッド ---

    /// <summary>
    /// 攻撃データを登録する
    /// </summary>
    public void RegisterAttack(AttackData data)
    {
        if (_attackSettings.ContainsKey(data.Id))
        {
            Debug.LogWarning($"Attack ID '{data.Id}' is already registered and will be overwritten.");
        }
        _attackSettings[data.Id] = data;
        // 初回登録時はクールダウンは0
        _remainingCoolDowns[data.Id] = 0f;
    }

    /// <summary>
    /// 攻撃の発射を試みる
    /// </summary>
    /// <param name="attackId">試みる攻撃の識別子</param>
    /// <returns>攻撃が実行されたらtrue、実行されなかったらfalse</returns>
    public bool RequestAttack(string attackId)
    {
        // 1. 攻撃が登録されているかチェック
        if (!_attackSettings.ContainsKey(attackId))
        {
            Debug.LogError($"Attack ID '{attackId}' is not registered.");
            return false;
        }

        // 2. 攻撃を止めるよう言われているかチェック（状態異常など）
        if (_isStopped)
        {
            // Debug.Log($"Cannot attack because the model is stopped.");
            return false;
        }

        // 3. 他の攻撃の実行中かチェック
        if (IsExecutingAttack)
        {
            // Debug.Log($"Cannot attack because '{_currentExecutingAttackId}' is currently executing.");
            return false;
        }

        // 4. クールタイム中かチェック
        if (_remainingCoolDowns.TryGetValue(attackId, out float remainingCD) && remainingCD > 0f)
        {
            // Debug.Log($"Cannot attack '{attackId}' because it is on cool down for {remainingCD:F2}s.");
            return false;
        }

        // 5. すべての条件をクリア: 攻撃を実行
        var data = _attackSettings[attackId];

        // 状態を更新
        _currentExecutingAttackId = attackId;
        _currentExecutionTimer = data.ExecutionTime;
        _remainingCoolDowns[attackId] = data.CoolDownTime;

        // 外部に実行を依頼
        _executor.ExecuteAttack(attackId);

        // Debug.Log($"Executing attack '{attackId}'. Execution time: {data.ExecutionTime:F2}s. Cool down: {data.CoolDownTime:F2}s.");

        return true;
    }

    /// <summary>
    /// 攻撃を停止させる（状態異常など）
    /// </summary>
    public void StopAttack()
    {
        _isStopped = true;
        // 攻撃実行中でも強制的に停止させる場合は、ここで _currentExecutionTimer = 0f; を設定
        // しかし、ここでは「状態異常中は新しい攻撃を開始できない」制御に留めます。
        // （実行中の攻撃はアニメーションの都合などで最後まで実行させるケースもあるため）
    }

    /// <summary>
    /// 攻撃を再開させる
    /// </summary>
    public void ResumeAttack()
    {
        _isStopped = false;
    }

    /// <summary>
    /// 時間の経過を更新し、実行時間とクールタイムを管理する
    /// </summary>
    /// <param name="deltaTime">前回の呼び出しからの経過時間（秒）</param>
    public void Update(float deltaTime)
    {
        // 1. 攻撃実行時間の更新
        if (IsExecutingAttack)
        {
            _currentExecutionTimer -= deltaTime;

            if (_currentExecutionTimer <= 0f)
            {
                // 攻撃実行が終了した
                _currentExecutionTimer = 0f;

                _currentExecutingAttackId = null; // 実行中の攻撃をクリア
            }
        }

        // 2. クールタイムの更新
        var keys = new List<string>(_remainingCoolDowns.Keys); // コレクション変更エラー回避
        foreach (var id in keys)
        {
            if (_remainingCoolDowns[id] > 0f)
            {
                _remainingCoolDowns[id] -= deltaTime;
                if (_remainingCoolDowns[id] < 0f)
                {
                    _remainingCoolDowns[id] = 0f;
                }
            }
        }
    }
}

/// <summary>
/// 攻撃の実行ロジックを外部に委譲するためのインターフェース
/// </summary>
public interface IEnemyAttackExecutor
{
    /// <summary>
    /// 指定された種類の攻撃を実行する
    /// </summary>
    /// <param name="attackId">攻撃を識別するための文字列</param>
    void ExecuteAttack(string attackId);
}

/// <summary>
/// 攻撃ごとの設定データを保持する構造体
/// </summary>
public struct AttackData
{
    /// <summary>攻撃の識別子</summary>
    public string Id { get; }

    /// <summary>攻撃のクールタイム（秒）</summary>
    public float CoolDownTime { get; }

    /// <summary>攻撃の実行時間（秒）</summary>
    public float ExecutionTime { get; }

    public AttackData(string id, float coolDownTime, float executionTime)
    {
        Id = id;
        CoolDownTime = coolDownTime;
        ExecutionTime = executionTime;
    }
}