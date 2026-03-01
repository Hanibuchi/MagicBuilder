using UnityEngine;

/// <summary>
/// 放射物が衝突した地点にプレイヤーをテレポートさせる呪文放射物クラス。
/// SpellProjectileDamageSource を継承し、衝突時にプレイヤーの移動とアニメーションの再生を行います。
/// </summary>
public class TeleportProjectile : SpellProjectileDamageSource
{
    private bool _hasTeleported = false;

    [Header("Teleport Sound")]
    [SerializeField] private AudioClip teleportSound;
    [SerializeField] private float teleportSoundVolume = 1.0f;

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        // 先に基底クラスの衝突時の標準的な処理（ヒットSE再生やエフェクト生成など）を実行します。
        base.OnCollisionEnter2D(collision);

        if (!_hasTeleported)
        {
            // 衝突地点（コンタクトポイントがある場合はその位置、なければ自身の位置）を取得
            Vector2 teleportPosition = collision.contacts.Length > 0 ? collision.contacts[0].point : (Vector2)transform.position;
            PerformTeleport(teleportPosition);
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // 基底クラスのトリガー処理（地面以外への接触時のエフェクト生成など）を実行します。
        base.OnTriggerEnter2D(other);

        if (!_hasTeleported)
        {
            // トリガー（敵や壁など）に接触した地点にテレポートを実行。
            // 地面レイヤーに当たった場合なども含めてテレポートさせたい場合は、ここでレイヤー判定を行わずに実行します。
            PerformTeleport(transform.position);
        }
    }

    /// <summary>
    /// プレイヤーを指定した位置にテレポートさせ、自身を破棄します。
    /// </summary>
    /// <param name="destination">テレポート先の座標</param>
    private void PerformTeleport(Vector2 destination)
    {
        if (PlayerController.Instance != null)
        {
            _hasTeleported = true;

            // アニメーションの再生
            PlayerController.Instance.PlayTeleportAnimation();

            // テレポート音の再生
            if (SoundManager.Instance != null && teleportSound != null)
            {
                SoundManager.Instance.PlaySE(teleportSound, teleportSoundVolume);
            }

            // プレイヤーを指定した座標に移動
            PlayerController.Instance.TeleportTo(destination);

            // 放射物を破棄してテレポートを完了
            Destroy();
        }
    }
}
