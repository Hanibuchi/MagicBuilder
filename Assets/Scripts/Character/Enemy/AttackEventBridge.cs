using UnityEngine;

/// <summary>
/// アニメーションイベントとEnemyControllerの間の仲介役（ブリッジ）となるクラス。
/// Animatorから直接EnemyControllerを呼び出せない場合に、このクラスをAnimatorと
/// 同じGameObjectにアタッチし、EnemyControllerのメソッドを呼び出します。
/// </summary>
public class AttackEventBridge : MonoBehaviour
{
    [Header("ターゲット")]
    [Tooltip("実際に攻撃ロジックを持つEnemyControllerを割り当ててください。")]
    [SerializeField]
    private EnemyController targetController;

    private void Awake()
    {
        if (targetController == null)
        {
            Debug.LogError("AttackEventBridge: ターゲットとなる EnemyController が設定されていません。インスペクタを確認してください。", this);
        }
    }

    // --- アニメーションイベントから呼び出される公開メソッド ---

    /// <summary>
    /// アニメーションイベントから呼び出され、Attack1を実行します。
    /// </summary>
    public void OnAttack1()
    {
        if (targetController != null)
        {
            // EnemyControllerのAttack1メソッドを呼び出す
            targetController.Attack1();
        }
    }

    /// <summary>
    /// アニメーションイベントから呼び出され、Attack2を実行します。
    /// </summary>
    public void OnAttack2()
    {
        if (targetController != null)
        {
            // EnemyControllerのAttack2メソッドを呼び出す
            targetController.Attack2();
        }
    }

    /// <summary>
    /// アニメーションイベントから呼び出され、Attack3を実行します。
    /// </summary>
    public void OnAttack3()
    {
        if (targetController != null)
        {
            // EnemyControllerのAttack3メソッドを呼び出す
            targetController.Attack3();
        }
    }
}