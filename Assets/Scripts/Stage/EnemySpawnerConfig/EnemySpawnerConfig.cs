using UnityEngine;

/// <summary>
/// 敵の生成設定を保持するScriptableObjectの基底クラス。
/// 実行時に敵のGameObjectを生成するメソッドを提供します。
/// </summary>
[CreateAssetMenu(fileName = "NewEnemySpawnerConfig", menuName = "GameConfig/Enemy Spawner Config/Default")]
public class EnemySpawnerConfig : ScriptableObject
{
    [Header("敵の設定")]
    [Tooltip("生成する敵のプレハブ")]
    public GameObject enemyPrefab;

    /// <summary>
    /// 設定に基づき敵を生成し、そのGameObjectを返します。
    /// </summary>
    /// <param name="position">敵を生成する位置</param>
    /// <returns>生成された敵のGameObject</returns>
    public virtual GameObject SpawnEnemy(Vector3 position)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError($"EnemySpawnerConfig: enemyPrefabが設定されていません。Config名: {name}");
            return null;
        }

        // 単純にプレハブをInstantiateして返す
        GameObject enemyObject = Instantiate(enemyPrefab, position, Quaternion.identity);
        
        Debug.Log($"EnemySpawnerConfig: 敵 '{enemyPrefab.name}' を座標 {position} に生成しました。");
        return enemyObject;
    }
}