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
        animator.SetTrigger(FIRE_TRIGGER);
    }

    /// <param name="angle">狙いの角度 (度)</param>
    public void SetAimRotation(float angle, bool reset = false)
    {
        if (animator == null || !animator.enabled) return;
        if (reset)
            return;

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

    public override void NotifyDie()
    {
        base.NotifyDie();
        StageManager.Instance.GameOver();
        PlayDieSound();
    }


    [SerializeField] AudioClip dieSound;
    [SerializeField] float dieSoundVolume = 1.0f;

    public void PlayDieSound()
    {
        if (SoundManager.Instance != null && dieSound != null)
            SoundManager.Instance.PlaySE(dieSound, dieSoundVolume);
    }
}