using System.Collections.Generic;
using UnityEngine; // Unityで利用することを想定

/// <summary>
/// 敵の状態異常を制御するモデルクラス
/// </summary>
public class StatusEffectModel
{
    // 状態異常の通知先（MonoBehaviourを実装した敵オブジェクトなど）
    private IStatusEffectReceiver _receiver;

    // 現在アクティブな状態異常
    private StatusEffectType _currentActiveEffect = StatusEffectType.None;

    // 全ての状態異常の残り時間を管理するディクショナリ
    // キー: 状態異常の種類, 値: 状態異常の情報
    private Dictionary<StatusEffectType, StatusEffectInfo> _effectTimes = new Dictionary<StatusEffectType, StatusEffectInfo>();

    // コンストラクタ
    public StatusEffectModel(IStatusEffectReceiver receiver)
    {
        _receiver = receiver;
        // 初期化：すべての状態異常を辞書に追加し、残り時間を0にする
        _effectTimes.Add(StatusEffectType.FireStun, new StatusEffectInfo { Type = StatusEffectType.FireStun, RemainingTime = 0f, Priority = 2 });
        _effectTimes.Add(StatusEffectType.FreezeStun, new StatusEffectInfo { Type = StatusEffectType.FreezeStun, RemainingTime = 0f, Priority = 3 });
        _effectTimes.Add(StatusEffectType.IceSlow, new StatusEffectInfo { Type = StatusEffectType.IceSlow, RemainingTime = 0f, Priority = 1 });
    }

    // --- 公開メソッド（状態異常の適用） ---

    /// <summary>
    /// FireStun状態を適用または延長します。
    /// </summary>
    /// <param name="duration">状態異常を受ける時間</param>
    public void FireStun(float duration)
    {
        ApplyEffect(StatusEffectType.FireStun, duration);
    }

    /// <summary>
    /// FreezeStun状態を適用または延長します。
    /// </summary>
    /// <param name="duration">状態異常を受ける時間</param>
    public void FreezeStun(float duration)
    {
        ApplyEffect(StatusEffectType.FreezeStun, duration);
    }

    /// <summary>
    /// IceSlow状態を適用または延長します。
    /// </summary>
    /// <param name="duration">状態異常を受ける時間</param>
    public void IceSlow(float duration)
    {
        ApplyEffect(StatusEffectType.IceSlow, duration);
    }

    // --- 内部ロジック ---

    /// <summary>
    /// 全ての状態異常の残り時間を更新し、状態の遷移を処理します。
    /// UnityのMonoBehaviourのUpdateで呼び出すことを想定。
    /// </summary>
    /// <param name="deltaTime">前回の呼び出しからの経過時間</param>
    public void Update(float deltaTime)
    {
        bool currentEffectExpired = false;

        // 1. 全ての状態異常の残り時間を減らす
        var keys = new List<StatusEffectType>(_effectTimes.Keys);
        foreach (var type in keys)
        {
            var info = _effectTimes[type];
            if (info.RemainingTime > 0)
            {
                info.RemainingTime -= deltaTime;
                if (info.RemainingTime <= 0)
                {
                    info.RemainingTime = 0;
                    // 現在アクティブな状態異常が終了した場合のフラグ
                    if (type == _currentActiveEffect)
                    {
                        currentEffectExpired = true;
                    }
                }
                _effectTimes[type] = info; // 構造体の変更を反映
            }
        }

        // 2. アクティブな状態異常の終了処理
        if (currentEffectExpired)
        {
            EndCurrentEffect();
            // 3. 次の状態異常の開始処理
            StartHighestPriorityEffect();
        }
    }

    /// <summary>
    /// 新しい状態異常を適用または延長します。
    /// </summary>
    private void ApplyEffect(StatusEffectType newType, float duration)
    {
        if (duration <= 0) return;

        // 1. 新しい状態の残り時間を更新（延長処理）
        var newInfo = _effectTimes[newType];
        newInfo.RemainingTime = Mathf.Max(newInfo.RemainingTime, duration);
        _effectTimes[newType] = newInfo;

        // 2. 現在アクティブな状態異常との比較と遷移の判定
        if (_currentActiveEffect == StatusEffectType.None)
        {
            // アクティブな状態がない場合は、すぐに新しい状態を開始
            StartEffect(newType);
            return;
        }

        var currentInfo = _effectTimes[_currentActiveEffect];

        // 優先度チェック（新しい状態 vs. 現在の状態）
        if (newInfo.Priority > currentInfo.Priority)
        {
            // 新しい状態の優先度が高い場合：現在の状態を終了し、新しい状態を開始
            EndCurrentEffect();
            StartEffect(newType);
        }
        else if (newInfo.Priority == currentInfo.Priority)
        {
            EndCurrentEffect();
            StartEffect(newType);
            // 同じ状態異常が再度適用された場合は、残り時間が更新されるだけで、Start/Endメソッドは呼ばれない（上記1.で残り時間更新済み）
        }
        else // newInfo.Priority < currentInfo.Priority
        {
            // 新しい状態の優先度が低い場合（例: FireStun中にIceSlow）：何もしない。新しい状態は待機状態として残り時間が減り続ける。
        }
    }

    /// <summary>
    /// 指定された状態異常を開始し、レシーバーに通知します。
    /// </summary>
    private void StartEffect(StatusEffectType type)
    {
        _currentActiveEffect = type;

        switch (type)
        {
            case StatusEffectType.FireStun:
                _receiver.OnFireStunStart();
                break;
            case StatusEffectType.FreezeStun:
                _receiver.OnFreezeStunStart();
                break;
            case StatusEffectType.IceSlow:
                _receiver.OnIceSlowStart();
                break;
        }
    }

    /// <summary>
    /// 現在アクティブな状態異常を終了し、レシーバーに通知します。
    /// </summary>
    private void EndCurrentEffect()
    {
        if (_currentActiveEffect == StatusEffectType.None) return;

        StatusEffectType endedType = _currentActiveEffect;
        _currentActiveEffect = StatusEffectType.None; // アクティブ状態を解除

        switch (endedType)
        {
            case StatusEffectType.FireStun:
                _receiver.OnFireStunEnd();
                break;
            case StatusEffectType.FreezeStun:
                _receiver.OnFreezeStunEnd();
                break;
            case StatusEffectType.IceSlow:
                _receiver.OnIceSlowEnd();
                break;
        }
    }

    /// <summary>
    /// 残り時間が残っている状態異常の中で、最も優先度の高いものを開始します。
    /// </summary>
    private void StartHighestPriorityEffect()
    {
        if (_currentActiveEffect != StatusEffectType.None) return; // 既にアクティブな状態がある場合は何もしない

        StatusEffectType nextType = StatusEffectType.None;
        int maxPriority = -1;

        // 残り時間が > 0 の状態で、最も優先度の高い状態を見つける
        foreach (var pair in _effectTimes)
        {
            var info = pair.Value;
            if (info.RemainingTime > 0)
            {
                if (info.Priority > maxPriority)
                {
                    maxPriority = info.Priority;
                    nextType = info.Type;
                }
            }
        }

        // 次の状態異常があれば開始
        if (nextType != StatusEffectType.None)
        {
            StartEffect(nextType);
        }
    }
}

/// <summary>
/// 状態異常の種類
/// </summary>
public enum StatusEffectType
{
    None,
    FireStun,
    FreezeStun,
    IceSlow
}

/// <summary>
/// 状態異常の情報（残り時間と優先度）
/// </summary>
public struct StatusEffectInfo
{
    public StatusEffectType Type;
    public float RemainingTime;
    public int Priority; // 優先度
}

/// <summary>
/// 敵の状態異常の開始・終了を通知するためのインターフェース
/// </summary>
public interface IStatusEffectReceiver
{
    // FireStun
    void OnFireStunStart();
    void OnFireStunEnd();

    // FreezeStun
    void OnFreezeStunStart();
    void OnFreezeStunEnd();

    // IceSlow
    void OnIceSlowStart();
    void OnIceSlowEnd();
}