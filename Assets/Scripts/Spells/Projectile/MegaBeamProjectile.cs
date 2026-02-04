using UnityEngine;

/// <summary>
/// メガビーム呪文の実装クラス。
/// 直線状に伸びるビームを生成し、壁で止まるか最大距離まで到達します。
/// SpellProjectileDamageSourceを継承し、LineRendererとEdgeCollider2Dを同期させます。
/// </summary>
public class MegaBeamProjectile : SpellProjectileDamageSource
{
    [Header("Mega Beam Settings")]
    [Tooltip("ビームの最大長")]
    [SerializeField] private float maxLength = 20f;

    [Tooltip("ビームが衝突する（止まる）レイヤー。通常はGroundレイヤーを含めます")]
    [SerializeField] private LayerMask collisionLayers;

    [Tooltip("ビームの見た目を表現するLineRenderer")]
    [SerializeField] private LineRenderer lineRenderer;

    [Tooltip("ビームの当たり判定を表現するEdgeCollider2D")]
    [SerializeField] private EdgeCollider2D edgeCollider;

    [Tooltip("ビームの先端に配置するオブジェクト（パーティクルや発光体など）")]
    [SerializeField] private Transform tipObject;

    [Tooltip("先端オブジェクトの位置オフセット")]
    [SerializeField] private Vector3 tipOffset;

    private float initialWidth;
    private float initialRadius;

    protected override void Awake()
    {
        base.Awake();
        if (lineRenderer != null) initialWidth = lineRenderer.widthMultiplier;
        if (edgeCollider != null) initialRadius = edgeCollider.edgeRadius;
    }

    /// <summary>
    /// 初期化処理。
    /// </summary>
    public override void Initialize(float strength, SpellContext spellContext)
    {
        base.Initialize(strength, spellContext);

        // Groundレイヤーをデフォルトで設定（もしインスペクタで設定されていなければ）
        if (collisionLayers == 0)
        {
            collisionLayers = LayerMask.GetMask("Ground");
        }

        // 初期状態を即座に反映
        UpdateBeam();
    }

    private void Update()
    {
        UpdateBeam();
    }

    /// <summary>
    /// ビームの長さ、LineRenderer、EdgeCollider2D、および先端オブジェクトの位置を更新します。
    /// </summary>
    private void UpdateBeam()
    {
        // ビームの方向（このオブジェクトの右方向(x軸正)を正面とする）
        Vector2 direction = transform.right;
        Vector3 startPos = transform.position;

        // レイキャストで衝突地点を確認
        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, maxLength, collisionLayers);

        Vector3 endPos;
        if (hit.collider != null)
        {
            endPos = hit.point;
        }
        else
        {
            endPos = startPos + (Vector3)(direction * maxLength);
        }

        // LineRendererの更新
        if (lineRenderer != null)
        {
            // 初期値にオブジェクトのスケールを掛け合わせて太さに適用
            lineRenderer.widthMultiplier = initialWidth * transform.localScale.y;

            // 頂点数が2つであることを確認
            if (lineRenderer.positionCount != 2)
            {
                lineRenderer.positionCount = 2;
            }
            // LineRendererの設定がUse World Spaceかどうかに依存するが、
            // 一般的にはワールド座標で指定するか、ローカル座標で (0,0,0) と localEndPos にする
            if (lineRenderer.useWorldSpace)
            {
                lineRenderer.SetPosition(0, startPos);
                lineRenderer.SetPosition(1, endPos);
            }
            else
            {
                lineRenderer.SetPosition(0, Vector3.zero);
                lineRenderer.SetPosition(1, transform.InverseTransformPoint(endPos));
            }
        }

        // EdgeCollider2Dの更新
        if (edgeCollider != null)
        {
            // 初期半径にオブジェクトのYスケールを掛け合わせて適用
            edgeCollider.edgeRadius = initialRadius * transform.localScale.y;

            // EdgeCollider2Dのpointsはローカル座標系
            Vector2 localEndPoint = transform.InverseTransformPoint(endPos);
            edgeCollider.points = new Vector2[] { Vector2.zero, localEndPoint };
        }

        // 先端オブジェクトの更新
        if (tipObject != null)
        {
            // 回転を考慮したオフセットの適用
            tipObject.position = endPos + transform.TransformDirection(tipOffset);
            // 先端オブジェクトの向きをビームの進行方向に合わせる
            tipObject.right = direction;
        }
    }

    // ビームの場合は着弾で消える必要がない場合が多いため、
    // 基底クラスの衝突時挙動（貫通回数消費など）を必要に応じてオーバーライドします。

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        // 地面に当たった時のエフェクト生成などは UpdateBeam の Raycast 結果に基づいて 
        // 別の場所（tipObjectのパーティクルなど）で行うのが一般的。
        // ここでは基底の「Groundに当たったら消える」挙動を抑制したい場合は base を呼ばない。
        
        // もしビームが CharacterHealth 以外（壁など）に当たった時の処理が必要ならここに記述。
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // 敵などのトリガーに接触した際の処理。
        // SpellProjectileDamageSource では衝突エフェクトを生成するが、
        // ビームの場合は連続的に接触するため、ここで毎回生成すると過剰になる可能性がある。
        
        // 何もしない、または特定の条件でのみエフェクトを出すように制限。
    }
}
