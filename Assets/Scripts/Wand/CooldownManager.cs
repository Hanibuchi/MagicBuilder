using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 魔術師（プレイヤー）のクールタイムを管理するシングルトンクラス。
/// 攻撃に使用された全呪文のクールタイムの合計を計測し、攻撃可能状態を制御します。
/// </summary>
public class CooldownManager : MonoBehaviour
{
    // --- シングルトンインスタンス ---
    private static CooldownManager instance;
    public static CooldownManager Instance => instance;

    // --- 状態 ---

    [Header("クールタイム情報")]
    [Tooltip("現在の合計クールタイム（秒）。この値が0になると攻撃可能になる。")]
    [SerializeField]
    private float currentCoolTime = 0f;

    [Header("UI表示用オーバーライド")]
    [Tooltip("UIのクールタイム表示が目標値に到達するまでの時間（秒）")]
    [SerializeField]
    private float transitionDuration = .5f;
    private bool isDisplayOverridden = false;
    private float currentDisplayValue = 0f;
    private Coroutine transitionCoroutine = null;

    /// <summary>
    /// 現在攻撃が可能かどうかを示すプロパティ。
    /// </summary>
    public bool CanAttack => currentCoolTime <= 0f;

    // --- 初期化 ---

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // シーンをまたいで保持したい場合は DontDestroyOnLoad(gameObject); を追加
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- メインロジック ---

    private void Update()
    {
        // 攻撃可能でない場合のみ、クールタイムを減らす
        if (!CanAttack)
        {
            currentCoolTime -= Time.deltaTime;
            NotifyCooldownChanged();

            // クールタイムが0以下になったら、正確に0に設定し、ターン終了の通知などを発行（オプション）
            if (currentCoolTime <= 0f)
            {
                currentCoolTime = 0f;
                Debug.Log("🧙 プレイヤーのクールタイムが終了しました。攻撃可能！");
                // ここでターン終了やUI更新などのイベントを発行できます
                // 例: GameManager.Instance.EndPlayerTurn();
            }
        }
    }

    /// <summary>
    /// 新しい攻撃によってクールタイムを追加します。
    /// 使用した全ての呪文のクールタイムの合計を、現在のクールタイムに加算します。
    /// </summary>
    /// <param name="totalCooldownDuration">今回使用された全呪文のクールタイム合計（秒）。</param>
    public void AddCooldown(float totalCooldownDuration)
    {
        if (totalCooldownDuration < 0) return;

        // クールタイム追加時は表示状態を内部の実際の値に戻す
        ResetDisplayToActualCooldown();

        currentCoolTime += totalCooldownDuration;
        NotifyCooldownChanged();

        Debug.Log($"⏳ クールタイムを追加しました: +{totalCooldownDuration:F2}秒。合計クールタイム: {currentCoolTime:F2}秒");
    }

    /// <summary>
    /// 現在のクールタイムの残り時間を取得します。
    /// </summary>
    /// <returns>クールタイムの残り時間（秒）。</returns>
    public float GetRemainingCooldown()
    {
        return currentCoolTime;
    }

    // --- UI表示オーバーライド用の機能 ---

    /// <summary>
    /// UIの表示を本来の実際のクールタイムに戻します。
    /// </summary>
    public void ResetDisplayToActualCooldown()
    {
        isDisplayOverridden = false;
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }
        NotifyCooldownChanged();
    }

    /// <summary>
    /// 内部的なクールタイムの値は変えずに、即座にUI表示用のクールタイム値を変更します。
    /// </summary>
    /// <param name="targetDisplayValue">変更後のUI表示値</param>
    public void SetDisplayCooldownInstant(float targetDisplayValue)
    {
        isDisplayOverridden = true;
        currentDisplayValue = targetDisplayValue;
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }
        NotifyCooldownChanged();
    }

    /// <summary>
    /// 内部的なクールタイムの値は変えずに、現在表示している値から徐々にターゲットの値へ変化させます。
    /// </summary>
    /// <param name="targetDisplayValue">最終的なUI表示値</param>
    public void SetDisplayCooldownGradual(float targetDisplayValue)
    {
        // 現在の表示開始値を決定
        float startValue = isDisplayOverridden ? currentDisplayValue : currentCoolTime;

        isDisplayOverridden = true;

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        transitionCoroutine = StartCoroutine(TransitionDisplayValue(startValue, targetDisplayValue, transitionDuration));
    }

    private IEnumerator TransitionDisplayValue(float startValue, float targetValue, float duration)
    {
        float elapsedTime = 0f;

        // durationが0以下の場合は即座に完了させる
        if (duration <= 0f)
        {
            currentDisplayValue = targetValue;
            NotifyCooldownChanged();
            transitionCoroutine = null;
            yield break;
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            currentDisplayValue = Mathf.Lerp(startValue, targetValue, t);
            NotifyCooldownChanged();
            yield return null; // 1フレーム待機
        }

        currentDisplayValue = targetValue;
        NotifyCooldownChanged();
        transitionCoroutine = null;
    }

    // ------------------------------------

    private ICooldownChangeListener listener;

    /// <summary>
    /// クールタイム変更を監視するリスナーを設定します（登録できるのは一つのみ）。
    /// </summary>
    public void SetListener(ICooldownChangeListener listener)
    {
        this.listener = listener;
        // 初期状態を通知
        NotifyCooldownChanged();
    }

    /// <summary>
    /// クールタイムの変更をリスナーに通知します。
    /// </summary>
    private void NotifyCooldownChanged()
    {
        float valueToDisplay = isDisplayOverridden ? currentDisplayValue : currentCoolTime;
        this.listener?.OnCooldownChanged(valueToDisplay);
    }
}

/// <summary>
/// クールタイムの残り時間が変更されたときに通知を受け取るためのインターフェース。
/// </summary>
public interface ICooldownChangeListener
{
    /// <summary>
    /// クールタイムが変更されたときに呼び出されます。
    /// </summary>
    /// <param name="remainingCooldown">現在のクールタイムの残り時間（秒）。</param>
    void OnCooldownChanged(float remainingCooldown);
}