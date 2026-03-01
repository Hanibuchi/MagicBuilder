using UnityEngine;

/// <summary>
/// 指定されたエリア（トリガー）に入った生物を即死させるためのコンポーネント。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
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
