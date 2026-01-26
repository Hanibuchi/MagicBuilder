using UnityEngine;

/// <summary>
/// IDamageSourceの基本実装クラス。魔法弾などの基底として使用します。
/// </summary>
public class DamageSourceBase : MonoBehaviour, IDamageSource
{
    // --- インスペクタ設定 ---

    [Header("ダメージタイプ設定")]
    [Tooltip("このダメージ源の攻撃タイプ")]
    [SerializeField]
    private DamageSourceType damageSourceType = DamageSourceType.SingleHit;

    [Header("SingleHit 設定")]
    [Tooltip("単発ヒット時の貫通回数 (SingleHitの場合のみ有効)。貫通回数が0になるとオブジェクトは破壊されます。")]
    [SerializeField]
    private int maxPierceCount = 1;

    [Header("演出設定")]
    [SerializeField] protected bool enableImpulse = false;
    [SerializeField] protected float impulseForce = 1f; // 振動の強さ

    // --- 内部状態 ---

    // 実行時に使用する現在の貫通回数
    private int currentPierceCount;

    protected virtual void Awake()
    {
        // 貫通回数を初期化
        currentPierceCount = maxPierceCount;
    }

    /// <summary>
    /// カメラ振動を発生させます。
    /// </summary>
    public void GenerateImpulse()
    {
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.RequestImpulse(impulseForce);
        }
    }

    /// <summary>
    /// このダメージ源が持つダメージ情報を取得します。
    /// 継承先で具体的なダメージ値を設定します。（このクラスではデフォルト値を返す）
    /// </summary>
    public virtual Damage GetDamage()
    {
        return new();
    }

    /// <summary>
    /// このダメージ源の攻撃タイプを取得します。
    /// </summary>
    public DamageSourceType GetDamageSourceType()
    {
        return damageSourceType;
    }

    /// <summary>
    /// 単発ヒットのターゲットとしてヒット可能かどうかを判定し、可能であれば貫通回数を消費します。
    /// 貫通回数が0以下になった場合、オブジェクトを破壊します。
    /// </summary>
    public bool TryConsumePierceCount()
    {
        if (damageSourceType != DamageSourceType.SingleHit)
        {
            // SingleHit 以外は常に true を返す（ただし、CharacterHealth側でSingleHit以外では呼ばれない想定）
            return true;
        }

        if (currentPierceCount > 0)
        {
            currentPierceCount--;
            // Debug.Log($"{gameObject.name}がヒット。残り貫通回数: {currentPierceCount}");

            if (currentPierceCount <= 0)
            {
                // 貫通回数が尽きたらオブジェクトを破壊
                Destroy();
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// 単発ヒットの貫通回数を追加します。
    /// </summary>
    public void AddPierceCount(int count)
    {
        if (damageSourceType == DamageSourceType.SingleHit)
        {
            currentPierceCount += count;
            // Debug.Log($"{gameObject.name}に貫通回数({count})が追加されました。合計: {currentPierceCount}");
        }
    }

    /// <summary>
    /// このオブジェクトを破棄します。
    /// </summary>
    public virtual void Destroy()
    {
        Destroy(gameObject);
    }
}