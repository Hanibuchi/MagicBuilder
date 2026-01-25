using UnityEngine;

/// <summary>
/// 敵の生成設定を保持するセリアライズ可能なクラス。
/// 実行時に敵のGameObjectを生成するメソッドを提供します。
/// </summary>
[System.Serializable]
public class EnemySpawnerConfig
{
    [Header("敵の設定")]
    [Tooltip("生成する敵のプレハブ")]
    public GameObject enemyPrefab;

    [Header("ボス設定")]
    [Tooltip("ボス敵として扱うか（基底のクリア条件に関連する場合）")]
    public bool isBoss;

    [Header("カスタムドロップ設定")]
    [Tooltip("生成した敵に適用する、上書き用のドロップ呪文リスト")]
    public DroppableSpell[] customDroppableSpells;

    /// <summary>
    /// 設定に基づき敵を生成し、設定に応じてコンポーネント追加やドロップ上書きを行います。
    /// </summary>
    /// <param name="position">敵を生成する位置</param>
    /// <returns>生成された敵のGameObject</returns>
    public virtual GameObject SpawnEnemy(Vector3 position)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError($"EnemySpawnerConfig: enemyPrefabが設定されていません。");
            return null;
        }

        // プレハブをInstantiate
        GameObject enemyObject = Object.Instantiate(enemyPrefab, position, Quaternion.identity);
        Debug.Log($"EnemySpawnerConfig: 敵 '{enemyPrefab.name}' を座標 {position} に生成しました。");

        // ボスの場合、BossClearNotifierを追加
        if (isBoss)
        {
            enemyObject.AddComponent<BossClearNotifier>();
            Debug.Log($"EnemySpawnerConfig: ボス敵 '{enemyObject.name}' に BossClearNotifier をアタッチしました。");
        }

        // カスタムドロップが設定されている場合、SpellDropperを上書き
        if (customDroppableSpells != null && customDroppableSpells.Length > 0)
        {
            SpellDropper dropper = enemyObject.GetComponent<SpellDropper>();
            if (dropper != null)
            {
                dropper.droppableSpells = customDroppableSpells;
                Debug.Log($"EnemySpawnerConfig: 敵 '{enemyObject.name}' のSpellDropperにカスタム設定を適用しました。ドロップアイテム数: {customDroppableSpells.Length}");
            }
            else
            {
                Debug.LogWarning($"EnemySpawnerConfig: 敵 '{enemyObject.name}' に SpellDropper コンポーネントが見つかりませんでした。");
            }
        }

        return enemyObject;
    }
}