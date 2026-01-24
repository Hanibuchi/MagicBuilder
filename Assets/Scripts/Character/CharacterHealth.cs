using UnityEngine;

/// <summary>
/// ダメージを受け、体力を管理するコンポーネント。
/// </summary>
public class CharacterHealth : MonoBehaviour
{
    // --- 外部参照用変数 (public/SerializeField) ---

    [Header("体力設定")]
    [Tooltip("現在の体力")]
    public float currentHealth;
    [Tooltip("最大体力")]
    public float maxHealth = 100f;

    [Header("属性と状態")]
    [Tooltip("このキャラクターの属性")]
    public CharacterElement characterElement = CharacterElement.None;
    [Tooltip("火ダメージによる完全停止の最大時間")]
    public float maxStopDurationOnFire = 10.0f;
    [Tooltip("氷ダメージによる減速/完全停止の判定基準となる、氷ダメージ/最大体力の値")]
    public float iceStopThreshold = 0.3f;
    [Tooltip("氷ダメージによる完全停止の最大時間")]
    public float iceStopDuration = 20.0f;
    [Tooltip("氷ダメージによる減速の最大時間")]
    public float maxSlowDurationOnIce = 10.0f;

    // ノックバック処理を委譲するためのインターフェース（ノックバック処理を行うコンポーネントが実装）
    private IKickbackHandler knockbackHandler;

    // ダメージ表示処理を委譲するためのインターフェース
    private IDamageNotifier damageNotifier;
    private IDieNotifier dieNotifier;
    private IHealthNotifier healthNotifier;

    private void Awake()
    {
        currentHealth = maxHealth;
        // ノックバック処理を担うコンポーネントを取得
        knockbackHandler = GetComponent<IKickbackHandler>();

        // ダメージ通知処理を担うコンポーネントを取得
        damageNotifier = GetComponent<IDamageNotifier>();
        dieNotifier = GetComponent<IDieNotifier>();
        healthNotifier = GetComponent<IHealthNotifier>();
    }

    private static int ALLAY_LAYER_INDEX = 9;
    private static int ENEMY_LAYER_INDEX = 10;
    private static int ALLAY_ATTACK_LAYER_INDEX = 11;
    private static int ENEMY_ATTACK_LAYER_INDEX = 12;

    // 衝突時にダメージ源からダメージを受け取る。（単発/範囲）
    public void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollisionEnter(collision.gameObject);
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollisionEnter(collision.gameObject);
    }

    // 接触中にダメージ源からダメージを受け取る。（多段）
    public void OnCollisionStay2D(Collision2D collision)
    {
        HandleCollisionStay(collision.gameObject);
    }
    public void OnTriggerStay2D(Collider2D collision)
    {
        HandleCollisionStay(collision.gameObject);
    }

    // 衝突処理を統合する新しいメソッド
    void HandleCollisionEnter(GameObject obj)
    {
        // 衝突したオブジェクト、またはその親や子からIDamageSourceインターフェースを持つコンポーネントを取得
        IDamageSource damageSource = obj.GetComponentInParent<IDamageSource>();
        if (damageSource == null)
        {
            damageSource = obj.GetComponentInChildren<IDamageSource>();
        }

        if (damageSource != null)
        {
            // *** Layer Filtering: 敵味方/同陣営の弾丸を無視する処理を追加 ***
            int selfLayer = gameObject.layer;
            int otherLayer = obj.layer;

            // 1. 自分(Allay)が味方弾(Allay_Projectile)に当たった場合
            // 2. 自分(Enemy)が敵弾(Enemy_Projectile)に当たった場合
            if ((selfLayer == ALLAY_LAYER_INDEX && otherLayer == ALLAY_ATTACK_LAYER_INDEX) ||
                (selfLayer == ENEMY_LAYER_INDEX && otherLayer == ENEMY_ATTACK_LAYER_INDEX))
                return; // 処理を無視して終了

            DamageSourceType sourceType = damageSource.GetDamageSourceType();

            switch (sourceType)
            {
                case DamageSourceType.SingleHit:
                    // 単発ヒット: Enter時のみ、かつ貫通回数が残っているかチェック
                    if (damageSource.TryConsumePierceCount())
                        ApplyDamage(damageSource.GetDamage(), obj);
                    break;

                case DamageSourceType.AreaOfEffect:
                    // 範囲攻撃: Enter時のみダメージ適用（範囲内の敵すべてに当たる）
                    ApplyDamage(damageSource.GetDamage(), obj);
                    break;
            }
        }
    }


    // 衝突処理を統合する新しいメソッド
    void HandleCollisionStay(GameObject obj)
    {
        // 衝突したオブジェクト、またはその親や子からIDamageSourceインターフェースを持つコンポーネントを取得
        IDamageSource damageSource = obj.GetComponentInParent<IDamageSource>();
        if (damageSource == null)
        {
            damageSource = obj.GetComponentInChildren<IDamageSource>();
        }

        if (damageSource != null)
        {
            // *** Layer Filtering: 敵味方/同陣営の弾丸を無視する処理を追加 ***
            int selfLayer = gameObject.layer;
            int otherLayer = obj.layer;

            // 1. 自分(Allay)が味方弾(Allay_Projectile)に当たった場合
            // 2. 自分(Enemy)が敵弾(Enemy_Projectile)に当たった場合
            if ((selfLayer == ALLAY_LAYER_INDEX && otherLayer == ALLAY_ATTACK_LAYER_INDEX) ||
                (selfLayer == ENEMY_LAYER_INDEX && otherLayer == ENEMY_ATTACK_LAYER_INDEX))
                return; // 処理を無視して終了
            DamageSourceType sourceType = damageSource.GetDamageSourceType();

            if (sourceType == DamageSourceType.MultiHit)
            {
                // 多段ヒット: Stay時のみダメージ適用（一定時間ごとにダメージを与える処理は、IDamageSourceの実装側で管理する必要がある）
                // 注意: 多段ヒットのダメージ頻度（クールタイム）は、IDamageSourceを実装するコンポーネント側で制御することが一般的です。
                // ここでは衝突が継続していることのみを判定しています。
                ApplyDamage(damageSource.GetDamage(), obj);
            }
        }
    }

    /// <summary>
    /// ダメージを計算し、適用するメインメソッド。
    /// </summary>
    /// <param name="damage">受けた生のダメージデータ</param>
    /// <param name="other">ぶつかってきたオブジェクト</param>
    public void ApplyDamage(Damage damage, GameObject other)
    {
        if (currentHealth <= 0) return; // 既に死んでいる場合は処理しない
        float previousHealth = currentHealth;
        // 1. 属性に基づくダメージ補正
        Damage modifiedDamage = ApplyElementModifier(damage);

        // 2. 体力減少
        float totalElementalDamage = modifiedDamage.woodDamage + modifiedDamage.waterDamage + modifiedDamage.FireDamage + modifiedDamage.IceDamage;
        float finalDamage = modifiedDamage.baseDamage + totalElementalDamage;

        currentHealth -= finalDamage;
        Debug.Log($"{gameObject.name}は{finalDamage}のダメージを受けました。残り体力: {currentHealth}");

        // 4. ノックバック処理の委譲
        HandleKnockback(modifiedDamage.knockback, other);

        healthNotifier?.NotifyHealthChange(maxHealth, previousHealth, currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }

        // 3. 受けたダメージによる状態異常処理
        HandleFireAndIceEffects(modifiedDamage);

        // 4. ダメージ表示通知 (単一メソッドで属性ごとに通知)
        if (damageNotifier != null)
        {
            // 基本ダメージ
            if (modifiedDamage.baseDamage > 0)
            {
                damageNotifier.NotifyDamage(DamageType.Base, modifiedDamage.baseDamage);
            }
            // 火ダメージ
            if (modifiedDamage.FireDamage > 0)
            {
                damageNotifier.NotifyDamage(DamageType.Fire, modifiedDamage.FireDamage);
            }
            // 氷ダメージ
            if (modifiedDamage.IceDamage > 0)
            {
                damageNotifier.NotifyDamage(DamageType.Ice, modifiedDamage.IceDamage);
            }
            // 木ダメージ
            if (modifiedDamage.woodDamage > 0)
            {
                damageNotifier.NotifyDamage(DamageType.Wood, modifiedDamage.woodDamage);
            }
            // 水ダメージ
            if (modifiedDamage.waterDamage > 0)
            {
                damageNotifier.NotifyDamage(DamageType.Water, modifiedDamage.waterDamage);
            }
        }
    }

    /// <summary>
    /// 属性に応じたダメージ補正を計算します。
    /// </summary>
    /// <param name="damage">生のダメージデータ</param>
    /// <returns>補正後のダメージデータ</returns>
    private Damage ApplyElementModifier(Damage damage)
    {
        Damage modDamage = damage;
        float fireMod = 1f;
        float woodMod = 1f;
        float waterMod = 1f;
        float iceMod = 1f;

        // 火ダメージと氷ダメージは温度ダメージとして、fireDamageとiceDamageプロパティで処理
        // modDamage.temperatureDamageはそのまま

        switch (characterElement)
        {
            case CharacterElement.Fire:
                // 火属性の場合: 水ダメージ3倍、木ダメージ0.5倍
                fireMod = 0f;
                waterMod *= 3.0f;
                woodMod *= 0.5f;
                iceMod *= 5.0f;
                break;
            case CharacterElement.Wood:
                // 木属性の場合: 火ダメージ3倍、水ダメージ0.5倍
                fireMod *= 3.0f; // FireDamageに適用
                woodMod = 0f;
                waterMod *= 0.5f;
                break;
            case CharacterElement.Water:
                // 水属性の場合: 木ダメージ3倍、火ダメージ0.5倍
                fireMod *= 0.5f; // FireDamageに適用
                woodMod *= 3.0f;
                waterMod = 0f;
                break;
            case CharacterElement.Ice:
                fireMod *= 5.0f;
                iceMod = 0f;
                break;
            default:
                break;
        }

        // temperatureDamageは一つの変数なので、補正をかける際はFireDamageとIceDamageに分けて計算します
        if (modDamage.temperatureDamage > 0)
        {
            // 火ダメージの場合
            modDamage.temperatureDamage *= fireMod;
        }
        else if (modDamage.temperatureDamage < 0)
        {
            modDamage.temperatureDamage *= iceMod;
        }

        modDamage.woodDamage *= woodMod;
        modDamage.waterDamage *= waterMod;

        return modDamage;
    }

    /// <summary>
    /// 火・氷ダメージによる状態異常を処理し、interfaceを通じて通知します。
    /// </summary>
    /// <param name="modifiedDamage">補正後のダメージデータ</param>
    private void HandleFireAndIceEffects(Damage modifiedDamage)
    {
        float fireDamage = modifiedDamage.FireDamage;
        float iceDamage = modifiedDamage.IceDamage;

        // --- 火ダメージによる停止処理 ---
        if (fireDamage > 0f && characterElement != CharacterElement.Fire)
        {
            // 火属性ではない場合にのみ処理
            float stopRatio = fireDamage / maxHealth;
            float stopDuration = Mathf.Min(stopRatio * maxStopDurationOnFire, maxStopDurationOnFire);

            // 引数: 停止時間 (float)
            damageNotifier.NotifyFireStun(stopDuration);
            Debug.Log($"火ダメージ({fireDamage})により、{stopDuration}秒間停止します。");
        }

        // --- 氷ダメージによる減速/完全停止処理 ---
        if (iceDamage > 0f && characterElement != CharacterElement.Ice)
        {
            // 氷属性ではない場合にのみ処理
            float stopFactor = iceDamage / maxHealth;

            if (stopFactor >= iceStopThreshold)
            {
                // 完全停止
                damageNotifier.NotifyFreezeStun(iceStopDuration);
                Debug.Log($"氷ダメージ({iceDamage})が大きいため、{iceStopDuration}秒間完全に停止します。");
            }
            else
            {
                // 減速                
                float slowDuration = stopFactor * maxSlowDurationOnIce;
                damageNotifier.NotifyIceSlow(slowDuration);
                Debug.Log($"氷ダメージ({iceDamage})により、{slowDuration}秒間減速します。");
            }
        }
    }

    /// <summary>
    /// ノックバック処理をIKickbackHandlerに委譲します。
    /// </summary>
    /// <param name="knockbackValue">ノックバック量</param>
    /// <param name="other">ノックバック方向の参考にできる情報</param>
    private void HandleKnockback(float knockbackValue, GameObject other)
    {
        if (knockbackValue > 0f && knockbackHandler != null)
        {
            // ノックバック処理を実装コンポーネントに任せる
            knockbackHandler.ApplyKickback(knockbackValue, other);
        }
    }
    /// <summary>
    /// キャラクターが死亡しているかどうかを取得します。
    /// </summary>
    public bool IsDead => isDead;
    // --- 追加: 死亡状態フラグ ---
    private bool isDead = false;
    /// <summary>
    /// キャラクターが死亡した時の処理。
    /// </summary>
    private void Die()
    {
        Kill(false);
    }

    /// <summary>
    /// キャラクターを即死させます。
    /// </summary>
    /// <param name="silent">無音（エフェクトなし）で死亡させるかどうか</param>
    public void Kill(bool silent = false)
    {
        if (isDead) return; // 既に死亡済みなら重複処理を避ける
        isDead = true;
        currentHealth = 0;
        Debug.Log($"{gameObject.name}は倒れた。");
        // 死亡時のアニメーションやゲームオーバー処理などを記述
        dieNotifier?.NotifyDie(silent);
    }

    /// <summary>
    /// キャラクターを復活させ、体力を最大まで回復します。
    /// </summary>
    public void Revive()
    {
        isDead = false;
        float previousHealth = currentHealth;
        currentHealth = maxHealth;
        healthNotifier?.NotifyHealthChange(maxHealth, previousHealth, currentHealth);
        Debug.Log($"{gameObject.name}が復活しました！");
    }
}

/// <summary>
/// ノックバック処理を担うコンポーネントが実装すべきインターフェース。
/// </summary>
public interface IKickbackHandler
{
    /// <summary>
    /// ノックバックを適用します。
    /// </summary>
    /// <param name="knockbackValue">ノックバックの強さ</param>
    /// <param name="other">ぶつかってきた放射物</param>
    void ApplyKickback(float knockbackValue, GameObject other);
}

/// <summary>
/// キャラクターの属性。今後の拡張に対応するため、enumとして定義。
/// </summary>
public enum CharacterElement
{
    None,
    Fire,   // 火
    Wood,   // 木
    Water,  // 水
    Ice     // 氷
}

/// <summary>
/// ダメージ源の攻撃タイプ。
/// </summary>
public enum DamageSourceType
{
    SingleHit,    // 単発ヒット（貫通可能）
    AreaOfEffect, // 範囲攻撃
    MultiHit      // 多段ヒット
}

/// <summary>
/// ダメージを与えるコンポーネントが実装すべきインターフェース。
/// </summary>
public interface IDamageSource
{
    /// <summary>
    /// このダメージ源が持つダメージ情報を取得します。
    /// </summary>
    /// <returns>Damage構造体。</returns>
    Damage GetDamage();

    /// <summary>
    /// このダメージ源の攻撃タイプを取得します。
    /// </summary>
    DamageSourceType GetDamageSourceType();

    // --- 単発ヒット（貫通あり）専用 ---

    /// <summary>
    /// 単発ヒットのターゲットとしてヒット可能かどうかを判定します。
    /// 呼び出すたびに内部でヒット回数/貫通回数をカウントダウンし、
    /// 0以下になったらfalseを返します。
    /// </summary>
    /// <returns>ヒット可能であればtrue、そうでなければfalse。</returns>
    bool TryConsumePierceCount();

    /// <summary>
    /// 単発ヒットの貫通回数を設定/追加します。
    /// </summary>
    /// <param name="count">追加する貫通回数。</param>
    void AddPierceCount(int count);
}


/// <summary>
/// ダメージ表示 (フローティングテキストなど) を担当するコンポーネントが実装するインターフェース。
/// </summary>
public interface IDamageNotifier
{
    /// <summary>
    /// ダメージを受け取ったことを通知し、表示処理を委譲します。
    /// </summary>
    /// <param name="damageType">ダメージの属性 (Base, Fire, Iceなど)</param>
    /// <param name="damageValue">その属性のダメージ値</param>
    void NotifyDamage(DamageType damageType, float damageValue);

    // 火傷によるスタン（停止）を通知するメソッド
    void NotifyFireStun(float duration);

    // 凍結による完全停止を通知するメソッド
    void NotifyFreezeStun(float duration);

    // 氷ダメージによる減速を通知するメソッド
    void NotifyIceSlow(float duration);
}

public interface IDieNotifier
{
    void NotifyDie(bool silent = false);
}


/// <summary>
/// HPの変化を通知するコンポーネントが実装すべきインターフェース。
/// </summary>
public interface IHealthNotifier
{
    /// <summary>
    /// HPの変化を通知し、表示処理を委譲します。
    /// </summary>
    /// <param name="maxHP">最大HP</param>
    /// <param name="previousHP">ダメージ/回復前のHP</param>
    /// <param name="currentHP">ダメージ/回復後の現在のHP</param>
    void NotifyHealthChange(float maxHP, float previousHP, float currentHP);
}