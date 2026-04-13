using UnityEngine;

public class BombDamageSource : SpellProjectileDamageSource
{
    [Header("Bomb Settings")]
    [Tooltip("爆発の威力（強さ）")]
    [SerializeField] private float bombStrength = 1.0f;

    [Tooltip("自身の攻撃対象のレイヤー")]
    [SerializeField] private SpellLayer bombSpellLayer = SpellLayer.Attack_Enemy;

    [Tooltip("衝突時に生成するオブジェクトの攻撃対象のレイヤー")]
    [SerializeField] private SpellLayer spawnedSpellLayer = SpellLayer.Attack_Both;

    [Tooltip("自身が与えるダメージ情報")]
    [SerializeField] private Damage bombDamage;

    [Tooltip("生存時間（-1で無限）")]
    [SerializeField] private float bombDuration = 3f;

    protected override void Awake()
    {
        // 最低限の引数でSpellContextを構築
        SpellContext context = new SpellContext(bombSpellLayer)
        {
            damage = bombDamage,
            duration = bombDuration
        };

        // 自身をInitialize
        Initialize(bombStrength, context);

        // 基底クラスのAwake（Launch処理など）を呼ぶ
        base.Awake();
    }

    protected override void SpawnCollisionPrefab(Vector2 spawnPos)
    {
        if (cachedContext != null)
        {
            // 衝突時に生成するオブジェクトのレイヤーに書き換えてから生成
            cachedContext.layer = spawnedSpellLayer;
        }
        base.SpawnCollisionPrefab(spawnPos);
    }
}
