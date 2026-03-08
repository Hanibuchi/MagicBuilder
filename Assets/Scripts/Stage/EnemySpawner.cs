using System.Collections;
using UnityEngine;

[System.Serializable]
public class EnemySpawnData
{
    [Tooltip("この敵がスポーンするまでの待機時間（秒）")]
    public float delayBeforeSpawn;
    public GameObject enemyPrefab;
}

public class EnemySpawner : MonoBehaviour
{
    [SerializeField, Tooltip("敵をスポーンさせる場所")]
    private Transform spawnPoint;

    [SerializeField, Tooltip("スポーンさせる敵の順番と時間")]
    private EnemySpawnData[] spawnSequence;

    /// <summary>
    /// スポーン処理を開始する
    /// </summary>
    public void StartSpawning()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        foreach (var spawnData in spawnSequence)
        {
            // 指定された時間待機
            yield return new WaitForSeconds(spawnData.delayBeforeSpawn);

            // プレハブとスポーン位置が設定されていれば生成
            if (spawnData.enemyPrefab != null && spawnPoint != null)
            {
                Instantiate(spawnData.enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            }
        }
    }
}
