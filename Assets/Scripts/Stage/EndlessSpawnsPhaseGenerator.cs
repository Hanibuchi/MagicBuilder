using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 複数の盤面（敵のセット）を順番に切り替えながら、スポーン間隔を徐々に短くしつつ無限に敵を湧かせるフェーズを生成するクラス。
/// </summary>
[System.Serializable]
public class EndlessSpawnsPhaseGenerator : PhaseGeneratorBase
{
    [System.Serializable]
    public class BoardConfig
    {
        [Tooltip("この盤面の開始条件タイプ")]
        public EnemyPhaseConfig.PhaseConditionType startConditionType = EnemyPhaseConfig.PhaseConditionType.TimeElapsed;

        [Tooltip("この盤面の開始条件の値")]
        public float startConditionValue = 0f;

        [Tooltip("この盤面でのスポーン候補の敵と、その確率重み")]
        public List<RandomSpawnsPhaseGenerator.EnemySpawnWeight> enemies = new List<RandomSpawnsPhaseGenerator.EnemySpawnWeight>();

        [Tooltip("この盤面を継続する時間（秒）")]
        public float duration = 60.0f;
    }

    [Header("エンドレス設定")]
    [Tooltip("生成する盤面のリスト（上から順に実行され、最後まで行くと最初に戻ります）")]
    public List<BoardConfig> boards = new List<BoardConfig>();

    [Header("ループ設定")]
    [Tooltip("全Boardを一巡して、再び最初のBoardに戻る際の開始条件タイプ")]
    public EnemyPhaseConfig.PhaseConditionType loopStartConditionType = EnemyPhaseConfig.PhaseConditionType.TimeElapsed;

    [Tooltip("全Boardを一巡して、再び最初のBoardに戻る際の開始条件の値")]
    public float loopStartConditionValue = 0f;

    [Tooltip("最初のBoardのスポーン間隔（秒）")]
    public float initialSpawnInterval = 2.0f;

    [Tooltip("1Board進むごとに増加する、1秒あたりの敵の出現数（スポーン頻度の増加量）")]
    public float spawnFrequencyIncreasePerBoard = 0.1f;

    [Header("ドロップ設定")]
    [Tooltip("敵がドロップする可能性のあるSpellBaseのリスト")]
    public List<SpellBase> droppableSpells = new List<SpellBase>();

    [Tooltip("レア度ごとのドロップ確率")]
    public List<RandomSpawnsPhaseGenerator.RarityDropRate> rarityDropRates = new List<RandomSpawnsPhaseGenerator.RarityDropRate>();

    public override void GeneratePhases(List<EnemyPhaseConfig> phaseList)
    {
        // 互換性のため残すが、基本はGeneratePhasesEnumerableを使用する
        Debug.LogWarning("EndlessSpawnsPhaseGenerator: GeneratePhases()が呼ばれましたがエンドレスモードのため、安全のため1ループのみ生成します。フル機能はGeneratePhasesEnumerable()を利用してください。");
        foreach (var phase in GeneratePhasesEnumerable())
        {
            phaseList.Add(phase);
            // 互換性のため適当なところで打ち切る
            if (phaseList.Count > 1000) break;
        }
    }

    public override IEnumerable<EnemyPhaseConfig> GeneratePhasesEnumerable()
    {
        if (boards == null || boards.Count == 0)
        {
            Debug.LogWarning("EndlessSpawnsPhaseGenerator: 盤面（Board）が設定されていません。");
            yield break;
        }

        int totalBoardsPlayed = 0;
        int loopCount = 0;

        // 厳密な無限ループ
        while (true)
        {
            for (int bIndex = 0; bIndex < boards.Count; bIndex++)
            {
                var board = boards[bIndex];
                if (board.enemies == null || board.enemies.Count == 0) continue;

                // スポーン間隔の計算（頻度の増加）
                // 初期頻度 = 1 / 初期間隔
                // 現在の頻度 = 初期頻度 + (総実行Board数 * 頻度増加量)
                float initialFrequency = 1.0f / initialSpawnInterval;
                float currentFrequency = initialFrequency + (totalBoardsPlayed * spawnFrequencyIncreasePerBoard);
                float currentSpawnInterval = 1.0f / currentFrequency;

                // この盤面でのおよその出現回数を計算
                int spawnCount = Mathf.FloorToInt(board.duration / currentSpawnInterval);
                if (spawnCount < 1) spawnCount = 1; // 最低1体は出す

                for (int i = 0; i < spawnCount; i++)
                {
                    // 重み付きランダムで敵を選択
                    var selectedEnemy = GetRandomEnemy(board.enemies);

                    // ドロップアイテムの抽選
                    DroppableSpell[] customDrops = GenerateCustomDrops();

                    // 一様分布に従うランダムな間隔
                    float randomInterval = Random.Range(0f, currentSpawnInterval * 2f);

                    EnemyPhaseConfig.PhaseConditionType type = EnemyPhaseConfig.PhaseConditionType.TimeElapsed;
                    float value = randomInterval;

                    if (i == 0)
                    {
                        if (loopCount > 0 && bIndex == 0)
                        {
                            type = loopStartConditionType;
                            value = loopStartConditionValue;
                        }
                        else
                        {
                            type = board.startConditionType;
                            value = board.startConditionValue;
                        }
                    }

                    // フェーズを逐次発行（ここで遅延評価される）
                    yield return new EnemyPhaseConfig
                    {
                        conditionType = type,
                        conditionValue = value,
                        isBossPhase = false,
                        spawnerConfig = new EnemySpawnerConfig
                        {
                            enemyPrefab = selectedEnemy.enemyPrefab,
                            customDroppableSpells = customDrops
                        }
                    };
                }

                totalBoardsPlayed++;
            }
            
            loopCount++;
        }
    }

    private RandomSpawnsPhaseGenerator.EnemySpawnWeight GetRandomEnemy(List<RandomSpawnsPhaseGenerator.EnemySpawnWeight> enemies)
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
        if (droppableSpells == null || droppableSpells.Count == 0 || rarityDropRates == null) return new DroppableSpell[0];

        float randomValue = Random.value;
        float currentProb = 0f;
        SpellRarity? selectedRarity = null;

        foreach (var rate in rarityDropRates)
        {
            currentProb += rate.dropRate;
            if (randomValue <= currentProb)
            {
                selectedRarity = rate.rarity;
                break;
            }
        }

        if (selectedRarity.HasValue)
        {
            var candidates = droppableSpells.Where(s => s != null && s.rarity == selectedRarity.Value).ToList();
            if (candidates.Count > 0)
            {
                SpellBase selectedSpell = candidates[Random.Range(0, candidates.Count)];
                return new DroppableSpell[]
                {
                    new DroppableSpell
                    {
                        spellData = selectedSpell,
                        dropChance = 1.0f
                    }
                };
            }
        }

        return new DroppableSpell[0];
    }
}