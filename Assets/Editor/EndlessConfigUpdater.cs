using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class EndlessConfigUpdater : EditorWindow
{
    private static string NormalizeEnemyName(string name)
    {
        if (name == "ゴブリン") return "斧ゴブリン";
        if (name == "いのしし") return "イノシシ";
        if (name == "でかいのしし") return "でかイノシシ";
        if (name == "デカスライム") return "でかスライム";
        return name;
    }

    [MenuItem("Tools/Update Endless Stage Configs")]
    public static void UpdateConfigs()
    {
        string enemyCsvPath = "Assets/EnemyPrefabList.csv";
        string spawnCsvPath = "Assets/EndlessSpawnConfig.csv";

        if (!File.Exists(enemyCsvPath) || !File.Exists(spawnCsvPath))
        {
            Debug.LogError("CSV files not found.");
            return;
        }

        // 0. Get SpellBase entries from SpellDatabase
        SpellDatabase spellDatabase = SpellDatabase.Instance;
        if (spellDatabase == null)
        {
            Debug.LogError("SpellDatabase couldn't be loaded.");
            return;
        }

        var defaultDropRates = new List<RandomSpawnsPhaseGenerator.RarityDropRate>
        {
            new RandomSpawnsPhaseGenerator.RarityDropRate { rarity = SpellRarity.Common, dropRate = 0.10f },
            new RandomSpawnsPhaseGenerator.RarityDropRate { rarity = SpellRarity.Uncommon, dropRate = 0.075f },
            new RandomSpawnsPhaseGenerator.RarityDropRate { rarity = SpellRarity.Rare, dropRate = 0.05f },
            new RandomSpawnsPhaseGenerator.RarityDropRate { rarity = SpellRarity.Epic, dropRate = 0.025f }
        };

        // 1. Load EnemyPrefabs
        var enemyLines = File.ReadAllLines(enemyCsvPath);
        Dictionary<string, GameObject> enemyPrefabs = new Dictionary<string, GameObject>();
        for (int i = 1; i < enemyLines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(enemyLines[i])) continue;
            string[] split = enemyLines[i].Split(',');
            if (split.Length >= 2)
            {
                string name = split[0].Trim();
                string path = split[1].Trim();
                string fullPath = "Assets/Prefabs/Enemy/" + path;
                GameObject p = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
                if (p != null) enemyPrefabs[name] = p;
            }
        }

        // 2. Load EndlessSpawnConfig
        var spawnLines = File.ReadAllLines(spawnCsvPath);
        
        // stageName -> List of Boards
        var endlessConfigData = new Dictionary<string, List<EndlessSpawnsPhaseGenerator.BoardConfig>>();
        var stageDropMap = new Dictionary<string, HashSet<string>>(); // stageName -> HashSet of spells

        for (int i = 1; i < spawnLines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(spawnLines[i])) continue;
            string[] split = spawnLines[i].Split(',');
            if (split.Length >= 5)
            {
                string stageRaw = split[0].Trim(); // e.g., "7-1 (エンドレス)"
                string conditionTypeStr = split[1].Trim();
                string conditionValueStr = split[2].Trim();
                string durationStr = split[3].Trim();
                string composition = split[4].Trim();
                string dropSpellsStr = split.Length >= 6 ? split[5].Trim() : "";

                string stageName = stageRaw.Split(' ')[0].Trim(); // e.g., "7-1"

                if (!endlessConfigData.ContainsKey(stageName))
                {
                    endlessConfigData[stageName] = new List<EndlessSpawnsPhaseGenerator.BoardConfig>();
                }
                if (!stageDropMap.ContainsKey(stageName))
                {
                    stageDropMap[stageName] = new HashSet<string>();
                }
                
                // Parse Drop Spells
                if (!string.IsNullOrEmpty(dropSpellsStr))
                {
                    string[] drops = dropSpellsStr.Split(new string[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var d in drops)
                    {
                        stageDropMap[stageName].Add(d.Trim());
                    }
                }
                
                Enum.TryParse(conditionTypeStr, out EnemyPhaseConfig.PhaseConditionType cType);
                float.TryParse(conditionValueStr, out float cValue);
                float.TryParse(durationStr, out float duration);

                float[] weights = { 4f, 3f, 2f, 0.8f, 0.2f };
                var enemiesList = new List<RandomSpawnsPhaseGenerator.EnemySpawnWeight>();
                string[] enemiesStr = composition.Split(new string[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
                for (int eIdx = 0; eIdx < enemiesStr.Length && eIdx < weights.Length; eIdx++)
                {
                    string eName = NormalizeEnemyName(enemiesStr[eIdx].Trim());
                    if (enemyPrefabs.TryGetValue(eName, out GameObject prefab))
                    {
                        enemiesList.Add(new RandomSpawnsPhaseGenerator.EnemySpawnWeight
                        {
                            enemyPrefab = prefab,
                            weight = weights[eIdx]
                        });
                    }
                }

                endlessConfigData[stageName].Add(new EndlessSpawnsPhaseGenerator.BoardConfig
                {
                    startConditionType = cType,
                    startConditionValue = cValue,
                    duration = duration,
                    enemies = enemiesList
                });
            }
        }

        // 3. Process Endless Stages (e.g. 7-1)
        string stageToUpdate = "7-1";
        string targetPath = $"Assets/Stage/7_extra/{stageToUpdate}_StageConfig.asset";

        var stageConfigObj = AssetDatabase.LoadAssetAtPath<StageConfig>(targetPath);
        if (stageConfigObj == null)
        {
            Debug.LogWarning($"StageConfig not found: {targetPath}");
            return;
        }

        if (!endlessConfigData.ContainsKey(stageToUpdate))
        {
            Debug.LogWarning($"SpawnConfig data not found for endless stage: {stageToUpdate}");
            return;
        }

        var boardsData = endlessConfigData[stageToUpdate];
        stageConfigObj.phaseGenerators.Clear();
        stageConfigObj.stageType = StageType.Rush;

        var endlessGenerator = new EndlessSpawnsPhaseGenerator
        {
            boards = boardsData,
            loopStartConditionType = EnemyPhaseConfig.PhaseConditionType.TimeElapsed,
            loopStartConditionValue = 10f,
            initialSpawnInterval = 3.0f,
            spawnFrequencyIncreasePerBoard = 0.2f,
            droppableSpells = new List<SpellBase>(),
            rarityDropRates = new List<RandomSpawnsPhaseGenerator.RarityDropRate>(defaultDropRates)
        };

        if (stageDropMap.TryGetValue(stageToUpdate, out HashSet<string> dropTypes))
        {
            if (dropTypes.Contains("ALL") || dropTypes.Contains("すべて"))
            {
                // Set all available spells
                foreach (SpellType sType in spellDatabase.GetAllRegisteredSpellTypes())
                {
                    SpellBase sb = spellDatabase.GetSpellAsset(sType);
                    if (sb != null)
                    {
                        endlessGenerator.droppableSpells.Add(sb);
                    }
                }
            }
            else
            {
                foreach (string sTypeStr in dropTypes)
                {
                    if (Enum.TryParse(sTypeStr, out SpellType sType))
                    {
                        SpellBase sb = spellDatabase.GetSpellAsset(sType);
                        if (sb != null)
                        {
                            endlessGenerator.droppableSpells.Add(sb);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Unknown spell type in CSV: {sTypeStr}");
                    }
                }
            }
        }

        stageConfigObj.phaseGenerators.Add(endlessGenerator);

        EditorUtility.SetDirty(stageConfigObj);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Endless Stage Config Succesfully Updated: {targetPath}");
    }
}
