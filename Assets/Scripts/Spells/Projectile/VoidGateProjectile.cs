using UnityEngine;

/// <summary>
/// 触れた場所のタイルを一時的に削除し、一定時間後に元に戻す「ヴォイドゲート」呪文の放射物クラス。
/// SpellProjectileDamageSourceを継承し、トリガー判定を利用します。
/// </summary>
public class VoidGateProjectile : SpellProjectileDamageSource
{
    [Header("ヴォイドゲート設定")]
    [Tooltip("削除されたタイルが復元されるまでの時間（秒）")]
    [SerializeField] private float restoreDelay = 3.0f;

    [Header("サウンド設定")]
    [SerializeField] private AudioClip gateActiveSound;
    [SerializeField] private float gateSoundVolume = 1.0f;

    [SerializeField] private Collider2D projectileCollider;

    protected override void Awake()
    {
        base.Awake();
        if (projectileCollider == null)
            projectileCollider = GetComponentInChildren<Collider2D>();
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // 基地クラスの衝突判定ロジック（他のオブジェクトへのダメージやエフェクト生成など）
        base.OnTriggerEnter2D(other);
        ProcessTileInteraction(other);
    }

    protected virtual void OnTriggerStay2D(Collider2D other)
    {
        ProcessTileInteraction(other);
    }

    private void ProcessTileInteraction(Collider2D other)
    {
        // Groundレイヤー（タイルマップを想定）に触れた場合の処理
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (TilemapManager.Instance != null)
            {
                // 自身の（または子オブジェクトの）コライダーがCircleCollider2Dなら半径、そうでなければBoundsを使って処理
                if (projectileCollider is CircleCollider2D circle)
                {
                    // CircleCollider2Dの半径とスケール、およびワールド座標の中心を考慮して円形範囲で処理
                    float radius = circle.radius * Mathf.Max(circle.transform.lossyScale.x, circle.transform.lossyScale.y);
                    TilemapManager.Instance.HandleTilesInCircle(circle.bounds.center, radius, restoreDelay);
                }
                else if (projectileCollider != null)
                {
                    TilemapManager.Instance.HandleTilesInBounds(projectileCollider.bounds, restoreDelay);
                }
                else
                {
                    TilemapManager.Instance.HandleTileEraseAndRestore(transform.position, restoreDelay);
                }
            }
        }
    }

    /// <summary>
    /// ヴォイドゲート活動中の音を再生する（アニメーションイベントから呼び出し可能）
    /// </summary>
    public void PlayGateActiveSound()
    {
        if (SoundManager.Instance != null && gateActiveSound != null)
        {
            SoundManager.Instance.PlaySE(gateActiveSound, gateSoundVolume);
        }
    }
}
