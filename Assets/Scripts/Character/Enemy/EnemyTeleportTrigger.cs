using UnityEngine;

/// <summary>
/// 敵が属する場面の識別子を持ち、死亡時にテレポートマネージャーへ通知するコンポーネント。
/// </summary>
public class EnemyTeleportTrigger : MonoBehaviour
{
    [Tooltip("この敵が属する場面のID。TeleportManagerの設定と一致させる必要があります。")]
    [SerializeField] private string stageId;
    
    private EnemyController enemyController;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
    }

    private void Start()
    {
        // スポーン時に自分を登録
        if (TeleportManager.Instance != null)
        {
            TeleportManager.Instance.RegisterEnemy(stageId);
        }
    }

    private void OnEnable()
    {
        if (enemyController != null)
        {
            enemyController.OnDie.AddListener(OnEnemyDefeated);
        }
    }

    private void OnDisable()
    {
        if (enemyController != null)
        {
            enemyController.OnDie.RemoveListener(OnEnemyDefeated);
        }
    }

    private void OnEnemyDefeated()
    {
        // 死亡時に登録を解除
        if (TeleportManager.Instance != null)
        {
            TeleportManager.Instance.UnregisterEnemy(stageId);
        }
    }
}
