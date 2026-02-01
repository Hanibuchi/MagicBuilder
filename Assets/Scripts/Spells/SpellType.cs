using UnityEngine;

// SpellType.cs
public enum SpellType
{
    None,
    ExampleSpell,
    Barrier,
    Downward,
    Upward,
    ErrorDegreeReduce,
    ErrorDegreeIncrease,
    TwoChainCast,
    ThreeChainCast,
    FiveChainCast,
    TwoMultiplier,
    ThreeMultiplier,
    FiveMultiplier,
    Expansion,
    Contraction,
    Inferno,
    ExampleSpellClickTrigger,
    IceShot,
    FireShot,
    IceBreak,
    IceDamage,
    FireDamage,
    WoodDamage,
    WaterDamage,
    AdditionalDamage,
    AdditionalKnockback,
    HealingDamage,
    Skip,
    Homing,
    HealShot,
    WaterShot,
    WoodShot,
    TurnRight,
    TurnLeft,
    Enchant,            // 付与
    Gumball,            // ガムボール
    KnockbackIncrease,  // ノックバック増加
    KnockbackReduce,    // ノックバック減少
    SelfHarm,        // 諸刃
    Remote,             // 遠隔
    Shorten,            // 短縮
    AddTrigger,         // トリガー追加
    AddClickTrigger,    // クリックトリガー追加
    ReflectBarrier,     // 反射バリア
    MegaBeam,           // 極太ビーム
    Blade,              // ブレイド
    ZeroGravity,        // 無重力
    Extension,          // 延長
    GiantArrow,         // 巨大矢
    Move,               // 移動
    Teleport,           // 転位
    MoveUp,             // 上昇
    MoveDown,           // 下降
    MoveLeft,           // 左進
    MoveRight,          // 右進
    InvertVertical,     // 上下反転（垂直反転）
    InvertHorizontal,   // 左右反転（水平反転）
    Acceleration,       // 加速
    Deceleration,       // 減速
    Elasticity,         // 弾性
    Directivity,        // 指向
    InstantDeath,       // 即死
    Attraction,         // 引力
    Repulsion,          // 斥力
    Delay,              // 遅延
    FireflyTrigger,     // 蛍トリガー
    BlackHole,          // ブラックホール
    ShortenFlat,        // 短縮(固定)
    Penetration,        // 透過性
    Orbit,         // 公転
}