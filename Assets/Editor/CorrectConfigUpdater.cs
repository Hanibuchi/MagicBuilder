using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ConfigUpdater : EditorWindow
{
    private static string NormalizeEnemyName(string name)
    {
        if (name == "ゴブリン") return "斧ゴブリン";
        if (name == "いのしし") return "イノシシ";
        if (name == "でかいのしし") return "でかイノシシ";
        if (name == "デカスライム") return "でかスライム";
        return name;
    }

    [MenuItem("Tools/Update Even Stage Configs")]
    public static void UpdateConfigs()
    {
        string enemyCsvPath = "Assets/EnemyPrefabList.csv";
        string spawnCsvPath = "Assets/SpawnConfig.csv";

        if (!File.Exists(enemyCsvPath) || !File.Exists(spawnCsvPath))
        {
            Debug.LogError("CSV files not found.");
            return;
        }

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
                else Debug.LogWarning($"Prefab not found: {fullPath}");
            }
        }

        // 2. Load SpawnConfig
        var spawnLines = File.ReadAllLines(spawnCsvPath);
        Dictionary<string, Dictionary<string, string>> spawnConfig = new Dictionary<string, Dictionary<string, string>>();
        
        for (int i = 1; i < spawnLines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(spawnLines[i])) continue;
            string[] split = spawnLines[i].Split(',');
            if (split.Length >= 3)
            {
                string stageRaw = split[0].Trim(); // e.g., "1-2 (樹海)"
                string phase = split[1].Trim();   // e.g., "序盤"
                string composition = split[2].Trim();

                string stageName = stageRaw.Split(' ')[0].Trim(); // e.g., "1-2"
                
                if (!spawnConfig.ContainsKey(stageName))
                {
                    spawnConfig[stageName] = new Dictionary<string, string>();
                }
                spawnConfig[stageName][phase] = composition;
            }
        }

        // 3. Process Stages
        string[] worldDirs = { "1_forest", "2_desert", "3_volcanic", "4_ice", "5_sky", "6_magic" };

        for (int w = 1; w <= 6; w++)
        {
            for (int s = 2; s <= 6; s += 2)
            {
                int L = (w - 1) * 3 + s / 2; // 1 to 18
                string stageName = $"{w}-{s}";
                string targetPath = $"Assets/Stage/{w}_{worldDirs[w - 1].Substring(2)}/{stageName}_StageConfig.asset";
                
                var stageConfigObj = AssetDatabase.LoadAssetAtPath<StageConfig>(targetPath);
                if (stageConfigObj == null)
                {
                    Debug.LogWarning($"StageConfig not found: {targetPath}");
                    continue;
                }

                if (!spawnConfig.ContainsKey(stageName))
                {
                    Debug.LogWarning($"SpawnConfig data not found for stage: {stageName}");
                    continue;
                }

                var stageData = spawnConfig[stageName];
                stageConfigObj.phaseGenerators.Clear();

                float[] weights = { 4f, 3f, 2f, 0.8f, 0.2f };
                string[] phaseKeys = { "序盤", "中盤", "終盤" };

                // RandomSpawnsPhaseGenerator for 序盤, 中盤, 終盤
                for (int p = 0; p < phaseKeys.Length; p++)
                {
                    string phaseKey = phaseKeys[p];
                    if (!stageData.ContainsKey(phaseKey)) continue;

                    var randomPhase = new RandomSpawnsPhaseGenerator();
                    randomPhase.startConditionType = EnemyPhaseConfig.PhaseConditionType.TimeElapsed;
                    randomPhase.startConditionValue = (p == 0) ? 0f : 5f;
                    randomPhase.duration = 20f + (L - 1);
                    randomPhase.spawnInterval = (8f - 6f * (L - 1) / 17f) * (1f - 0.25f * p);
                    
                    randomPhase.enemies = new List<RandomSpawnsPhaseGenerator.EnemySpawnWeight>();
                    
                    string[] enemiesStr = stageData[phaseKey].Split(new string[] { " / " }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < enemiesStr.Length && i < weights.Length; i++)
                    {
                        string eName = NormalizeEnemyName(enemiesStr[i].Trim());
                        if (enemyPrefabs.TryGetValue(eName, out GameObject prefab))
                        {
                            randomPhase.enemies.Add(new RandomSpawnsPhaseGenerator.EnemySpawnWeight
                            {
                                enemyPrefab = prefab,
                                weight = weights[i]
                            });
                        }
                    }

                    stageConfigObj.phaseGenerators.Add(randomPhase);
                }

                // SimplePhaseGenerator for ボス
                if (stageData.TryGetValue("ボス", out string bossStr))
                {
                    string[] bossParts = bossStr.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                    
                    List<string> bossSequence = new List<string>();
                    foreach (var part in bossParts)
                    {
                        string bName = part.Trim();
                        int count = 1;

                        if (bName.Contains("*"))
                        {
                            var starSplit = bName.Split('*');
                            bName = starSplit[0].Trim();
                            if (starSplit.Length > 1 && int.TryParse(starSplit[1], out int parsedCount))
                            {
                                count = parsedCount;
                            }
                        }

                        bName = NormalizeEnemyName(bName);

                        for (int c = 0; c < count; c++)
                        {
                            bossSequence.Add(bName);
                        }
                    }

                    for (int i = 0; i < bossSequence.Count; i++)
                    {
                        string bName = bossSequence[i];
                        if (enemyPrefabs.TryGetValue(bName, out GameObject bossPrefab))
                        {
                            bool isFirst = (i == 0);
                            bool isLast = (i == bossSequence.Count - 1);

                            var bossPhase = new SimplePhaseGenerator();
                            bossPhase.phaseConfig = new EnemyPhaseConfig
                            {
                                conditionType = EnemyPhaseConfig.PhaseConditionType.TimeElapsed,
                                conditionValue = 3f,
                                isBossPhase = isFirst,
                                spawnerConfig = new EnemySpawnerConfig
                                {
                                    enemyPrefab = bossPrefab,
                                    isBoss = isLast,
                                    customDroppableSpells = new DroppableSpell[0]
                                }
                            };
                            
                            stageConfigObj.phaseGenerators.Add(bossPhase);
                        }
                        else
                        {
                            Debug.LogWarning($"Boss prefab not found: {bName}");
                        }
                    }
                }

                EditorUtility.SetDirty(stageConfigObj);
                Debug.Log($"Updated: {targetPath}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("All Even Stage Configs Succesfully Updated!");
    }
}
