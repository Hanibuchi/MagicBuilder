using UnityEngine;

/// <summary>
/// 各種interfaceを実装してアニメーションを再生するためのクラス
/// </summary>
public class PlayerController : MyCharacterController
{
    public static PlayerController Instance { get; private set; }
    private const string FIRE_DEGREE_PARAM = "attack_degree";
    private const string AIM_TRIGGER = "aim";
    private const string AIM_DEGREE_PARAM = "aim_degree";
    private const string VICTORY_TRIGGER = "victory";
    private const string TELEPORT_TRIGGER = "teleport";

    [Header("Screen Shake Settings")]
    [SerializeField] private float damageShakeForce = 0.5f;

    [SerializeField] public Transform aimStartPoint;

    protected override void Awake()
    {
        base.Awake();
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        if (aimStartPoint == null)
        {
            Debug.LogError("AimStartPointが設定されていません。インスペクタから設定してください。");
        }
    }

    /// <summary>
    /// 魔法発射を通知し、攻撃アニメーションのトリガーを設定します。
    /// </summary>
    public void NotifyFire(float angle)
    {
        if (animator == null || !animator.enabled) return;

        // 1. 角度を0度から90度の範囲にクランプ（制限）
        // 例: -45 -> 0, 125 -> 90, 45 -> 45
        float clampedAngle = Mathf.Clamp(angle, 0f, 90f);

        // 2. クランプされた角度を0.0から1.0の値に正規化
        // Mathf.InverseLerp(最小値, 最大値, 現在の値) は、最小値〜最大値の範囲内で
        // 現在の値がどの位置にあるかを0.0〜1.0で返します。
        // 例: 0 -> 0.0, 90 -> 1.0, 45 -> 0.5
        float normalizedValue = Mathf.InverseLerp(0f, 90f, clampedAngle);
        // 3. Animatorのfloatパラメーターに設定
        animator.SetFloat(FIRE_DEGREE_PARAM, normalizedValue);
        animator.SetTrigger(PARAM_ATTACK_TRIGGER);
    }

    /// <param name="angle">狙いの角度 (度)</param>
    public void SetAimRotation(float angle, bool reset = false)
    {
        if (animator == null || !animator.enabled) return;
        if (reset)
        {
            animator.ResetTrigger(AIM_TRIGGER);
            return;
        }

        // 1. 角度を0度から90度の範囲にクランプ（制限）
        // 例: -45 -> 0, 125 -> 90, 45 -> 45
        float clampedAngle = Mathf.Clamp(angle, 0f, 90f);

        // 2. クランプされた角度を0.0から1.0の値に正規化
        // Mathf.InverseLerp(最小値, 最大値, 現在の値) は、最小値〜最大値の範囲内で
        // 現在の値がどの位置にあるかを0.0〜1.0で返します。
        // 例: 0 -> 0.0, 90 -> 1.0, 45 -> 0.5
        float normalizedValue = Mathf.InverseLerp(0f, 90f, clampedAngle);
        // 3. Animatorのfloatパラメーターに設定
        animator.SetFloat(AIM_DEGREE_PARAM, normalizedValue);
        animator.SetTrigger(AIM_TRIGGER);
    }

    public void Victory()
    {
        if (animator == null || !animator.enabled) return;
        animator.SetTrigger(VICTORY_TRIGGER);
    }

    public void PlayTeleportAnimation()
    {
        if (animator == null || !animator.enabled) return;
        animator.SetTrigger(TELEPORT_TRIGGER);
    }

    /// <summary>
    /// プレイヤーを指定されたワールド座標にテレポートさせます。
    /// </summary>
    /// <param name="position">移動先のワールド座標</param>
    public void TeleportTo(Vector2 position)
    {
        transform.position = position;
    }

    /// <summary>
    /// ダメージを受け取ったことを通知し、カメラを揺らします。
    /// </summary>
    public override void NotifyDamage(DamageType damageType, float damageValue)
    {
        base.NotifyDamage(damageType, damageValue);

        // ダメージを受けた際（回復以外）に画面を揺らす
        if (damageType != DamageType.Heal && CameraManager.Instance != null)
        {
            CameraManager.Instance.RequestImpulse(damageShakeForce);
        }
    }

    public override void NotifyDie(bool silent = false)
    {
        base.NotifyDie(silent);
        StageManager.Instance.GameOver();
    }

    /// <summary>
    /// プレイヤーを復活させます。
    /// </summary>
    public void Revive()
    {
        if (characterHealth != null)
        {
            characterHealth.Revive();
        }

        if (animator != null && animator.enabled)
        {
            // 死亡アニメーションから復帰するために、トリガーをリセットまたは初期状態に戻す
            animator.Rebind();
            // animator.Rebind() は animator を初期状態(エントリー状態)に戻します。
            // これにより、死亡アニメーションで止まっている場合に強制的にアイドル状態に戻せます。
        }
    }
}