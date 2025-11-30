using UnityEngine;

/// <summary>
/// ドロップする呪文の設定をカスタマイズできるEnemySpawnerConfigの子クラス。
/// 生成時に敵のSpellDropperの設定を上書きします。
/// </summary>
[CreateAssetMenu(fileName = "NewCustomDropSpawnerConfig", menuName = "GameConfig/Enemy Spawner Config/Custom Drop")]
public class CustomDropEnemySpawnerConfig : EnemySpawnerConfig
{
    [Header("カスタムドロップ設定")]
    [Tooltip("生成した敵に適用する、上書き用のドロップ呪文リスト")]
    public DroppableSpell[] customDroppableSpells;

    /// <summary>
    /// 設定に基づき敵を生成し、そのSpellDropperにカスタムドロップ設定を適用します。
    /// </summary>
    public override GameObject SpawnEnemy(Vector3 position)
    {
        // 基底クラスのメソッドで敵を生成
        GameObject enemyObject = base.SpawnEnemy(position);

        if (enemyObject == null)
        {
            return null;
        }

        // SpellDropperコンポーネントを取得
        SpellDropper dropper = enemyObject.GetComponent<SpellDropper>();

        if (dropper != null)
        {
            // カスタムドロップ設定をSpellDropperに適用
            dropper.droppableSpells = customDroppableSpells;

            // 適用されたか確認用のログ
            Debug.Log($"CustomDropEnemySpawnerConfig: 敵 '{enemyObject.name}' のSpellDropperにカスタム設定を適用しました。ドロップアイテム数: {customDroppableSpells.Length}");
        }
        else
        {
            Debug.LogWarning($"CustomDropEnemySpawnerConfig: 敵 '{enemyObject.name}' に SpellDropper コンポーネントが見つかりませんでした。ドロップ設定の上書きはスキップされました。");
        }

        return enemyObject;
    }
}