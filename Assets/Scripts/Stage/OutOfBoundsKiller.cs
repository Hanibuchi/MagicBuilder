using UnityEngine;

/// <summary>
/// 指定されたエリア（トリガー）の外に出た生物を即死させるためのコンポーネント。
/// マップ全体を覆うようにコライダーを配置して使用します。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class OutOfBoundsKiller : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D collision)
    {
        // 衝突したオブジェクト、またはその親オブジェクトから CharacterHealth コンポーネントを探す
        CharacterHealth health = collision.GetComponent<CharacterHealth>();
        if (health == null)
        {
            health = collision.GetComponentInParent<CharacterHealth>();
        }

        if (health != null)
        {
            // 敵（EnemyControllerを持つオブジェクト）の場合は無音で死ぬように設定
            bool isEnemy = health.GetComponent<EnemyController>() != null || health.GetComponentInParent<EnemyController>() != null;
            
            // CharacterHealth.Kill メソッドを呼び出して即死させる
            health.Kill(isEnemy);
        }
    }
}
