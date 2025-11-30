using UnityEngine;

public class KnockbackEffector : MonoBehaviour
{
    [Header("ノックバックのベース方向")]
    [Tooltip("ノックバックの力がかかる基本方向 (例: 上方向: (0, 1), 右上: (1, 1))")]
    public Vector2 knockbackDirection = Vector2.right; // インスペクタから定義できるように public

    private Rigidbody2D rb2d;

    void Start()
    {
        // 必須: このコンポーネントがアタッチされているオブジェクトの Rigidbody2D を取得
        rb2d = GetComponent<Rigidbody2D>();
        if (rb2d == null)
        {
            Debug.LogError("KnockbackEffectorには Rigidbody2D が必要です。");
            enabled = false; // Rigidbody2Dがなければコンポーネントを無効化
        }
    }

    /// <summary>
    /// 指定された強さでオブジェクトをノックバックさせます。
    /// </summary>
    /// <param name="strength">ノックバックの強さ (Forceの大きさ)。</param>
    public void ApplyKnockback(float strength)
    {
        if (rb2d == null || strength <= 0) return;

        // 1. ノックバック方向を正規化（長さ1にする）
        //    これにより、strengthが純粋な力の大きさを表すようになります。
        Vector2 direction = knockbackDirection.normalized;

        // 2. 方向と強さを掛け合わせてノックバックの力を計算
        Vector2 knockbackForce = direction * strength;

        // 3. Rigidbody2Dに力を加える
        //    ForceMode2D.Impulse: 瞬間的な力を加える（衝突や爆発などに適している）
        rb2d.AddForce(knockbackForce, ForceMode2D.Impulse);

        Debug.Log(gameObject.name + "がノックバックしました。強さ: " + strength);
    }

    // ノックバックの方向を外部から動的に設定したい場合に追加するメソッド（オプション）
    public void SetKnockbackDirection(Vector2 newDirection)
    {
        knockbackDirection = newDirection;
    }

    // public float test_strength = 10f;
    // public void Test()
    // {
    //     ApplyKnockback(test_strength);
    // }
}