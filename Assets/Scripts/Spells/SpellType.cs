using UnityEngine;

// SpellType.cs
// 役割ごと（攻撃、修飾、トリガー、システム）にグループ化して整理
public enum SpellType
{
    None,

    // --- 【攻撃：基本・弾丸系】 ---
    ExampleSpell,
    FireShot,
    IceShot,
    WaterShot,
    WoodShot,
    HealShot,
    IceBreak,       // アイスブレイク
    Inferno,         // 隕石
    MegaBeam,       // 極太ビーム
    Blade,          // ブレイド
    GiantArrow,     // 巨大矢
    BlackHole,      // ブラックホール
    InstantDeath,   // 即死
    Gumball,        // ガムボール（粘着弾）
    ExampleSpellClickTrigger,
    FireflyTrigger, // 蛍トリガー
    AreaHeal,

    // --- 【攻撃：属性ダメージ・追加効果】 ---
    AdditionalDamage,
    FireDamage,
    IceDamage,
    WaterDamage,
    WoodDamage,
    HealingDamage,  // 回復ダメージ

    // --- 【防御・設置系】 ---
    Barrier,
    ReflectBarrier, // 反射バリア

    // --- 【修飾：パラメータ変化】 ---
    Expansion,      // 膨張
    Contraction,    // 収縮
    Acceleration,   // 加速
    Deceleration,   // 減速
    ErrorDegreeReduce,   // 誤差減少
    ErrorDegreeIncrease, // 誤差増加
    KnockbackIncrease,   // ノックバック増加
    KnockbackReduce,     // ノックバック減少
    AdditionalKnockback, // ノックバック付与
    CoolTimeShorten,        // クールタイム短縮（倍率）
    CoolTimeShortenFlat,    // クールタイム短縮（固定）
    Shorten,        // 持続時間短縮
    Extension,      // 持続時間延長
    ZeroGravity,    // 無重力
    Penetration,    // 透過性
    Elasticity,     // 弾性

    // --- 【修飾：軌道・方向変化】 ---
    Homing,         // 追尾
    Directivity,    // 指向
    Orbit,          // 公転
    TurnRight,
    TurnLeft,
    InvertHorizontal, // 左右反転
    InvertVertical,   // 上下反転
    Upward,         // 上方
    Downward,       // 下方
    MoveUp,         // 上昇（速度）
    MoveDown,       // 下降（速度）
    MoveLeft,       // 左進
    MoveRight,      // 右進

    // --- 【特殊・挙動変化】 ---
    Enchant,        // 付与
    SelfHarm,       // 諸刃（自傷）
    Remote,         // 遠隔
    Teleport,       // 転位
    Attraction,     // 引力
    Repulsion,      // 斥力
    Move,           // 移動

    // --- 【トリガー・遅延系】 ---
    Delay,          // 遅延
    AddTrigger,     // トリガー追加
    AddClickTrigger,// クリックトリガー追加

    // --- 【マルチキャスト・フロー制御】 ---
    TwoChainCast,
    ThreeChainCast,
    FiveChainCast,
    TwoMultiplier,  // 2倍複製
    ThreeMultiplier,
    FiveMultiplier,
    Skip,           // 1マス飛ばし
    TwoSplit, // 多重詠唱
    ThreeSplit,
    FiveSplit,
}