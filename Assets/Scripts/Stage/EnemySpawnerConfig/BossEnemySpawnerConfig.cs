using UnityEngine;

/// <summary>
/// ステージクリアの条件となるボス敵を生成するための設定。
/// 生成時にBossClearNotifierコンポーネントを自動的にアタッチします。
/// </summary>
[CreateAssetMenu(fileName = "NewBossEnemySpawnerConfig", menuName = "GameConfig/Enemy Spawner Config/Boss Enemy")]
public class BossEnemySpawnerConfig : EnemySpawnerConfig
{
    /// <summary>
    /// 設定に基づきボス敵を生成し、そのGameObjectを返します。
    /// BossClearNotifierをアタッチする処理を追加しています。
    /// </summary>
    public override GameObject SpawnEnemy(Vector3 position)
    {
        // 基底クラスのSpawnEnemyを呼び出し、敵を生成
        GameObject enemyObject = base.SpawnEnemy(position);
        
        if (enemyObject == null)
        {
            return null;
        }

        // ボス敵なので、BossClearNotifierコンポーネントを追加
        enemyObject.AddComponent<BossClearNotifier>();
        Debug.Log($"BossEnemySpawnerConfig: ボス敵 '{enemyObject.name}' に BossClearNotifier をアタッチしました。");

        return enemyObject;
    }
}