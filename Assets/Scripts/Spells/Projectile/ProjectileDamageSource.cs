using UnityEngine;

/// <summary>
/// 衝突時にダメージ情報をCharacterHealthコンポーネントに提供するコンポーネント。
/// IDamageSourceインターフェースを実装しています。
/// </summary>
public class ProjectileDamageSource : MonoBehaviour, IDamageSource
{
    // --- 外部参照用変数 (public/SerializeField) ---

    [Header("ダメージ設定")]
    [Tooltip("このダメージ源が与える詳細なダメージ情報")]
    // publicかつ[System.Serializable]な構造体であるDamageを直接SerializeFieldとして定義することで、
    // インスペクタと外部スクリプトの両方から編集可能になります。
    Damage damageData;
    const string HIT_TRIGGER = "hit";

    // --- IDamageSourceの実装 ---
    public void Initialize(float strength, SpellContext spellContext)
    {
        damageData = spellContext.damage;
    }

    /// <summary>
    /// このダメージ源が持つダメージ情報を取得します。
    /// </summary>
    /// <returns>設定されたDamage構造体。</returns>
    public Damage GetDamage()
    {
        return damageData;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Destroy();
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}