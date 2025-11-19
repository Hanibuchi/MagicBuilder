using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// AimInputReaderからの入力（角度・強さ）を受け取り、
/// AttackManagerを介して実際のゲームロジック（照準表示/発射）に変換するクラス。
/// </summary>
public class AimController : MonoBehaviour, IAimController
{
    [Header("設定")]
    [Tooltip("入力を受け取るためのAimInputReaderのインスタンス")]
    [SerializeField]
    private AimInputReader aimInputReader;

    [Tooltip("現在選択されている杖のインデックス")]
    [SerializeField]
    private int selectedWandIndex => AttackManager.Instance.GetCurrentWandIndex();

    [Tooltip("キャラクターのアニメーションコントローラー")]
    [SerializeField]
    private CharacterAnimatorController animatorController;

    private void Start()
    {
        // AimInputReaderが設定されているか確認
        if (aimInputReader == null)
        {
            Debug.LogError("AimInputReaderが設定されていません。インスペクタから設定してください。");
            return;
        }

        // AimInputReaderにこのインスタンス（IAimController）を渡す
        aimInputReader.SetAimController(this);

        Debug.Log("AimControllerがAimInputReaderに登録されました。");
    }

    /// <summary>
    /// IAimControllerの実装: エイム中、補助線を表示
    /// </summary>
    /// <param name="angle">発射角度 (Z軸回転、度)</param>
    /// <param name="power">発射強度 (0.0f ～ 1.0f)</param>
    public void UpdateAimLine(float angle, float power)
    {
        if (!CooldownManager.Instance.CanAttack)
            return;

        // AttackManagerの補助線表示メソッドを呼び出す
        // ClearLineはfalseで呼び出す（常に表示を更新するため）
        AttackManager.Instance.DisplayAimingLine(
            selectedWandIndex,
            angle,
            power,
            false
        );
        // Debug.Log($"AimController: UpdateAimLine - 角度={angle:F2}, 強さ={power:F2}");

        animatorController.SetAimRotation(angle);
    }

    /// <summary>
    /// IAimControllerの実装: 補助線を非表示
    /// </summary>
    public void ClearAimLine()
    {
        // 補助線をクリアするためのDisplayAimingLine呼び出し
        AttackManager.Instance.DisplayAimingLine(
            selectedWandIndex,
            0f, // 角度や強さは意味を持たない
            0f,
            true // clearLineをtrueで呼び出し、AttackManager側で非表示ロジックを実行させる
        );
        // Debug.Log("AimController: ClearAimLine");
        animatorController.SetAimRotation(0f, true);
    }

    /// <summary>
    /// IAimControllerの実装: ドラッグ解除時、魔法を発射
    /// </summary>
    /// <param name="angle">発射角度 (Z軸回転、度)</param>
    /// <param name="power">発射強度 (0.0f ～ 1.0f)</param>
    public void ReleaseMagic(float angle, float power)
    {
        if (!CooldownManager.Instance.CanAttack)
            return;

        var wand = AttackManager.Instance.GetCurrentWand();
        if (wand != null)
            CooldownManager.Instance.AddCooldown(wand.GetTotalCooldown());

        // AttackManagerの発射メソッドを呼び出す
        AttackManager.Instance.FireWand(
            selectedWandIndex,
            angle,
            power
        );
        Debug.Log($"AimController: ReleaseMagic - 杖({selectedWandIndex})を発射！ 角度={angle:F2}, 強さ={power:F2}");

        if (animatorController != null)
        {
            animatorController.NotifyFire(angle);
        }
    }
}