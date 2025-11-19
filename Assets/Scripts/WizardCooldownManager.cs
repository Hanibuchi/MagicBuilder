using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 魔術師（プレイヤー）のクールタイムを管理するシングルトンクラス。
/// 攻撃に使用された全呪文のクールタイムの合計を計測し、攻撃可能状態を制御します。
/// </summary>
public class WizardCooldownManager : MonoBehaviour
{
    // --- シングルトンインスタンス ---
    private static WizardCooldownManager instance;
    public static WizardCooldownManager Instance => instance;

    // --- 状態 ---

    [Header("クールタイム情報")]
    [Tooltip("現在の合計クールタイム（秒）。この値が0になると攻撃可能になる。")]
    [SerializeField]
    private float currentCooldown = 0f;

    /// <summary>
    /// 現在攻撃が可能かどうかを示すプロパティ。
    /// </summary>
    public bool CanAttack => currentCooldown <= 0f;

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
            currentCooldown -= Time.deltaTime;

            // クールタイムが0以下になったら、正確に0に設定し、ターン終了の通知などを発行（オプション）
            if (currentCooldown <= 0f)
            {
                currentCooldown = 0f;
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
        
        currentCooldown += totalCooldownDuration;
        
        Debug.Log($"⏳ クールタイムを追加しました: +{totalCooldownDuration:F2}秒。合計クールタイム: {currentCooldown:F2}秒");
    }

    /// <summary>
    /// 現在のクールタイムの残り時間を取得します。
    /// </summary>
    /// <returns>クールタイムの残り時間（秒）。</returns>
    public float GetRemainingCooldown()
    {
        return currentCooldown;
    }
}