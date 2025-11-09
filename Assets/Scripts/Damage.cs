/// <summary>
/// 魔法や衝突によって与えられるダメージの詳細。
/// </summary>
[System.Serializable]
public struct Damage
{
    public float baseDamage;        // 基本ダメージ
    public float woodDamage;        // 木ダメージ
    public float waterDamage;       // 水ダメージ
    public float temperatureDamage; // 温度ダメージ (正: 火、負: 氷)
    public float knockback;         // ノックバック量

    // 便宜上のプロパティ
    public float FireDamage => temperatureDamage > 0 ? temperatureDamage : 0f;
    public float IceDamage => temperatureDamage < 0 ? -temperatureDamage : 0f;
}

/// <summary>
/// ダメージの種類。
/// </summary>
public enum DamageType
{
    Base,   // 基本ダメージ
    Fire,   // 火
    Wood,   // 木
    Water,  // 水
    Ice     // 氷
}