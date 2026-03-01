using UnityEngine;

/// <summary>
/// キャラクターが地形（"Ground"レイヤー）に埋まった際の窒息ダメージを管理するクラス。
/// </summary>
[RequireComponent(typeof(CharacterHealth))]
public class SuffocationDamage : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("埋まってからダメージを受け始めるまでの猶予時間（秒）")]
    [SerializeField] private float suffocationDelay = 0.5f;

    [Tooltip("1フレームあたりのダメージ量")]
    [SerializeField] private float damagePerFrame = 0.5f;

    [Tooltip("判定に使用するレイヤー")]
    [SerializeField] private LayerMask groundLayer;

    [Tooltip("埋まり判定を行うポイントのオフセット（キャラクターの胸のあたりを推奨）")]
    [SerializeField] private Vector2 checkOffset = new Vector2(0, 0.5f);

    private CharacterHealth _characterHealth;
    private float _buriedTimer = 0f;

    private void Awake()
    {
        _characterHealth = GetComponent<CharacterHealth>();
        
        // インスペクターで設定されていない場合、デフォルトで"Ground"レイヤーを取得
        if (groundLayer == 0)
        {
            groundLayer = LayerMask.GetMask("Ground");
        }
    }

    private void Update()
    {
        // キャラクターが死亡している場合は何もしない
        if (_characterHealth == null || _characterHealth.IsDead)
        {
            _buriedTimer = 0f;
            return;
        }

        // キャラクターの判定ポイントが地形に重なっているかをチェック
        Vector2 checkPosition = (Vector2)transform.position + checkOffset;
        bool isBuried = Physics2D.OverlapPoint(checkPosition, groundLayer);

        if (isBuried)
        {
            _buriedTimer += Time.deltaTime;

            // 一定時間（猶予時間）が経過したらダメージを適用
            if (_buriedTimer >= suffocationDelay)
            {
                ApplySuffocationDamage();
            }
        }
        else
        {
            // 地形から出たらタイマーをリセット
            _buriedTimer = 0f;
        }
    }

    /// <summary>
    /// CharacterHealthを介してダメージを適用します。
    /// </summary>
    private void ApplySuffocationDamage()
    {
        // 窒息ダメージのデータを作成
        Damage damage = new Damage
        {
            baseDamage = damagePerFrame,
            woodDamage = 0f,
            waterDamage = 0f,
            temperatureDamage = 0f,
            healing = 0f,
            knockback = 0f
        };

        // CharacterHealthにダメージを通知
        // 第2引数のGameObjectはダメージ源がないためnullを指定
        _characterHealth.ApplyDamage(damage, null);
    }

    /// <summary>
    /// エディタ上で判定ポイントを可視化します。
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 checkPosition = (Vector2)transform.position + checkOffset;
        Gizmos.DrawWireSphere(checkPosition, 0.1f);
    }
}
