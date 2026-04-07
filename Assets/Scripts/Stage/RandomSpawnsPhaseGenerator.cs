using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 指定した設定に基づいて、ランダムな敵を一定間隔でスポーンさせるフェーズを生成するクラス。
/// </summary>
[System.Serializable]
public class RandomSpawnsPhaseGenerator : PhaseGeneratorBase
{
    [System.Serializable]
    public struct EnemySpawnWeight
    {
        public GameObject enemyPrefab;
        [Tooltip("スポーンする確率の相対的な重み（大きいほど出やすい）")]
        public float weight;
    }

    [System.Serializable]
    public struct RarityDropRate
    {
        public SpellRarity rarity;
        [Tooltip("このレア度の呪文がドロップする確率")]
        [Range(0f, 1f)]
        public float dropRate;
    }

    [Header("敵のスポーン設定")]
    [Tooltip("スポーン候補の敵と、その確率重み")]
    public List<EnemySpawnWeight> enemies = new List<EnemySpawnWeight>();

    [Tooltip("敵がスポーンする頻度（秒間隔の平均値）。実際のスポーン間隔は指数分布に従うランダムな値になります。")]
    public float spawnInterval = 1.0f;

    [Tooltip("スポーンを続ける時間（秒）。0より大きい場合有効。")]
    public float duration = 60.0f;

    [Tooltip("スポーンする最大数。0より大きい場合有効。")]
    public int maxSpawnCount = 0;

    [Header("ドロップ設定")]
    [Tooltip("敵がドロップする可能性のあるSpellBaseのリスト")]
    public List<SpellBase> droppableSpells = new List<SpellBase>();

    [Tooltip("レア度ごとのドロップ確率")]
    public List<RarityDropRate> rarityDropRates = new List<RarityDropRate>();

    public override void GeneratePhases(List<EnemyPhaseConfig> phaseList)
    {
        if (enemies == null || enemies.Count == 0)
        {
            Debug.LogWarning("RandomSpawnsPhaseGenerator: スポーン候補の敵が設定されていません。");
            return;
        }

        // 生成するフェーズ数を計算
        int spawnCount = 0;
        if (maxSpawnCount > 0 && duration > 0)
        {
            spawnCount = Mathf.Min(maxSpawnCount, Mathf.FloorToInt(duration / spawnInterval));
        }
        else if (maxSpawnCount > 0)
        {
            spawnCount = maxSpawnCount;
        }
        else if (duration > 0)
        {
            spawnCount = Mathf.FloorToInt(duration / spawnInterval);
        }
        else
        {
            spawnCount = 1; // どちらも設定されていない場合の安全策
        }

        for (int i = 0; i < spawnCount; i++)
        {
            // 1. 重み付きランダムで敵を選択
            EnemySpawnWeight selectedEnemy = GetRandomEnemy();

            // 2. カスタムドロップを生成
            DroppableSpell[] customDrops = GenerateCustomDrops();

            // 一様分布に従うランダムな間隔を計算（範囲: 0 ～ 2*spawnInterval、平均値: spawnInterval）
            float randomInterval = Random.Range(0f, spawnInterval * 2f);

            // 3. EnemyPhaseConfigを生成して追加
            EnemyPhaseConfig config = new EnemyPhaseConfig
            {
                conditionType = EnemyPhaseConfig.PhaseConditionType.TimeElapsed,
                conditionValue = randomInterval,
                spawnerConfig = new EnemySpawnerConfig
                {
                    enemyPrefab = selectedEnemy.enemyPrefab,
                    customDroppableSpells = customDrops
                }
            };

            phaseList.Add(config);
        }
    }

    private EnemySpawnWeight GetRandomEnemy()
    {
        float totalWeight = enemies.Sum(e => e.weight);
        float randomValue = Random.Range(0, totalWeight);
        float currentSum = 0;

        foreach (var enemy in enemies)
        {
            currentSum += enemy.weight;
            if (randomValue <= currentSum)
            {
                return enemy;
            }
        }

        return enemies.Last();
    }

    private DroppableSpell[] GenerateCustomDrops()
    {
        if (droppableSpells == null || droppableSpells.Count == 0) return null;

        List<DroppableSpell> drops = new List<DroppableSpell>();

        foreach (var spell in droppableSpells)
        {
            if (spell == null) continue;

            float prob = GetDropRateForRarity(spell.rarity);

            // ★ TODO: DroppableSpell 構造体の実際のフィールド名に合わせて以下のコードを修正・有効化してください。
            // （例として「spell」および「dropRate」フィールドが存在すると仮定しています）
            DroppableSpell drop = new DroppableSpell
            {
                spellData = spell,            // SpellBase を設定するフィールド
                dropChance = prob          // ドロップ確率を設定するフィールド
            };
            drops.Add(drop);
        }

        return drops.ToArray();
    }

    private float GetDropRateForRarity(SpellRarity rarity)
    {
        var rateEntry = rarityDropRates.FirstOrDefault(r => r.rarity == rarity);
        return rateEntry.dropRate; // 未設定のレア度はデフォルトの0になります
    }
}
