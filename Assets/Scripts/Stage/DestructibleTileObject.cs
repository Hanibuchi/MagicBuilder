using UnityEngine;

/// <summary>
/// タイルと一緒に生成され、攻撃を感知してタイルを削除するコンポーネント。
/// CharacterHealthを参考に極力シンプルに実装しています。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DestructibleTileObject : MonoBehaviour
{
    [Header("破壊設定")]
    [Tooltip("このタイルを破壊できるダメージの属性を指定します")]
    [SerializeField] private DamageType targetDamageType = DamageType.Ice;

    [Tooltip("破壊されてから復活するまでの時間（秒）。0より大きい場合は復活します")]
    [SerializeField] private float restoreDelay = 9999f;

    private bool _isDestroyed = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void HandleCollision(GameObject obj)
    {
        if (_isDestroyed) return;

        // CharacterHealthと同様に、親や子からIDamageSourceを取得して攻撃を判定
        IDamageSource damageSource = obj.GetComponentInParent<IDamageSource>();
        if (damageSource == null)
        {
            damageSource = obj.GetComponentInChildren<IDamageSource>();
        }

        if (damageSource != null)
        {
            // まず属性のチェックを行う
            Damage incomingDamage = damageSource.GetDamage();
            if (!HasTargetDamageType(incomingDamage))
            {
                // 指定された属性のダメージが含まれていなければ何もしない
                return;
            }

            // 単発攻撃の場合は貫通回数を消費する
            DamageSourceType sourceType = damageSource.GetDamageSourceType();
            if (sourceType == DamageSourceType.SingleHit)
            {
                damageSource.TryConsumePierceCount();
            }

            // 攻撃を感知したらタイルを破壊
            DestroyTile();
        }
    }

    /// <summary>
    /// 受けたダメージに指定された属性が含まれているか判定します
    /// </summary>
    private bool HasTargetDamageType(Damage incomingDamage)
    {
        switch (targetDamageType)
        {
            case DamageType.Base:
                return incomingDamage.baseDamage > 0;
            case DamageType.Fire:
                return incomingDamage.FireDamage > 0;
            case DamageType.Wood:
                return incomingDamage.woodDamage > 0;
            case DamageType.Water:
                return incomingDamage.waterDamage > 0;
            case DamageType.Ice:
                return incomingDamage.IceDamage > 0;
            case DamageType.Heal:
                return incomingDamage.healing > 0;
            // InstantDeathなどの他の属性の判定が必要であれば追加してください
            default:
                return false;
        }
    }

    private void DestroyTile()
    {
        _isDestroyed = true;

        if (TilemapManager.Instance != null)
        {
            // インスペクタで設定した restoreDelay を渡し、HazardTilemapを対象にするよう true を指定
            TilemapManager.Instance.HandleTileEraseAndRestore(transform.position, restoreDelay, true);
        }

        //感知用のオブジェクト自身も不要になるため破棄
        Destroy(gameObject);
    }
}
