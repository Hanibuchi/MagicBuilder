using UnityEngine;

/// <summary>
/// 敵の投射物の移動処理を行います。
/// ローカルのX軸の正の方向に設定された速度で移動します。
/// </summary>
public class EnemyProjectileMovement : MonoBehaviour
{
    // [SerializeField]属性により、private変数でもUnityのエディタ（インスペクタ）から値を設定できるようになります。
    [SerializeField]
    private float moveSpeed = 5.0f; // 移動速度（インスペクタから設定）

    private Rigidbody2D rb;

    void Awake()
    {
        // Rigidbody2Dコンポーネントを取得します。
        // これがないと物理ベースの移動はできません。
        rb = GetComponent<Rigidbody2D>();

        // Rigidbody2Dがアタッチされているかチェック
        if (rb == null)
        {
            Debug.LogError("EnemyProjectileMovement requires a Rigidbody2D component on the same GameObject.");
            // スクリプトの実行を停止
            enabled = false;
            return;
        }

    }

    void Start()
    {
        // 速度をローカル座標の右方向（ローカルのX軸の正の方向）に設定します。
        // transform.right はワールド座標系でのローカルX軸の正の方向を返します。
        Vector2 velocity = transform.right * moveSpeed;

        // Rigidbody2Dに速度を設定します。
        rb.linearVelocity = velocity;

        // 【補足】
        // 攻撃オブジェクトが生成される前に、適切な角度に回転させておく必要があります。
        // 例: 敵がターゲットの方向を向くように回転させてから、このスクリプトを持つ投射物を生成する。
    }

    // FixedUpdateは物理演算のフレームレートで呼ばれるため、物理ベースの移動処理に適しています。
    // 今回はStart()で一度velocityを設定するだけなので、このメソッドは必須ではありませんが、
    // 将来的に移動中に何かを調整する場合のために残しておきます。
    /*
    void FixedUpdate()
    {
        // もし何らかの理由で速度が変更された場合、ここで再度設定することも可能です。
        // 現在はStart()で一回設定するだけで十分なため、コメントアウトしておきます。
        // Vector2 velocity = transform.right * moveSpeed;
        // rb.velocity = velocity;
    }
    */
}