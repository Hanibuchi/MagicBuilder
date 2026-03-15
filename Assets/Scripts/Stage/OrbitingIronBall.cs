using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class OrbitingIronBall : MonoBehaviour
{
    [Header("Orbit Settings")]
    [SerializeField, Tooltip("中心となるオブジェクトのTransform")]
    private Transform centerObject;

    [SerializeField, Tooltip("回転速度（スピード）")]
    private float moveSpeed = 10f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (centerObject == null) return;

        Vector2 currentPos = rb.position;
        Vector2 centerPos = centerObject.position;
        Vector2 offset = currentPos - centerPos;
        
        Vector2 directionFromCenter = offset.normalized;

        // 中心の方向に対して直角（接線方向）のベクトルを計算
        Vector2 tangentDirection = new Vector2(-directionFromCenter.y, directionFromCenter.x);

        // 直角方向に速度を設定
        rb.linearVelocity = tangentDirection * moveSpeed;
    }
}
