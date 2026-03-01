using UnityEngine;

public class KnockbackEffector : MonoBehaviour
{
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
    /// 指定されたベクトルでオブジェクトをノックバックさせます。
    /// </summary>
    /// <param name="force">ノックバックの力（方向と強さを含むベクトル）。</param>
    public void ApplyKnockback(Vector2 force)
    {
        if (rb2d == null) return;

        // Rigidbody2Dに力を加える
        // ForceMode2D.Impulse: 瞬間的な力を加える
        rb2d.AddForce(force, ForceMode2D.Impulse);

        Debug.Log($"{gameObject.name}がノックバックしました。力: {force}");
    }
}