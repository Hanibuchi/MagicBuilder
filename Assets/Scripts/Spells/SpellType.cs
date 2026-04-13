using UnityEngine;

// SpellType.cs
// 役割ごと（攻撃、修飾、トリガー、システム）にグループ化して整理
public enum SpellType
{
    None,

    // --- 【攻撃：基本・弾丸系】 ---
    // 単発ヒット
    ExampleSpell,
    IceShot,
    FireShot,
    WoodShot,
    WaterShot,
    HealShot,
    ExampleSpellClickTrigger,
    IceShotClickTrigger,
    FireShotClickTrigger,
    WoodShotClickTrigger,
    WaterShotClickTrigger,
    HealShotClickTrigger,
    Teleport,           // テレポート
    Enchant,        // 付与
    InstantDeath,   // 即死

    // トリガー
    Gumball,        // ガムボール（粘着弾）
    FireflyTrigger, // 蛍トリガー

    // 範囲ダメージ
    IceBreak,       // アイスブレイク
    Inferno,         // 隕石

    // 多段ヒット
    Blade,          // ブレイド
    GiantArrow,     // 巨大矢
    VoidGate,      // ヴォイドゲート
    AreaHeal,
    MegaBeam,       // 極太ビーム

    // --- 【防御・設置系】 ---
    Barrier,
    ReflectBarrier, // 反射バリア

    // --- 【攻撃：属性ダメージ・追加効果】 ---
    AdditionalDamage,
    IceDamage,
    FireDamage,
    WoodDamage,
    WaterDamage,
    HealingDamage,  // 回復ダメージ
    AdditionalKnockback, // ノックバック付与


    // --- 【修飾：パラメータ変化】 ---
    Upward,         // 上方
    Downward,       // 下方
    TurnLeft,
    TurnRight,
    AccelUp,         // 上昇（速度）
    AccelDown,       // 下降（速度）
    AccelLeft,       // 左進
    AccelRight,      // 右進
    Invert,          // 反転
    InvertHorizontal, // 左右反転
    InvertVertical,   // 上下反転
    Acceleration,   // 加速
    Deceleration,   // 減速

    Expansion,      // 膨張
    Contraction,    // 収縮
    ErrorDegreeReduce,   // 誤差減少
    ErrorDegreeIncrease, // 誤差増加
    Attraction,     // 引力
    Repulsion,      // 斥力
    Extension,      // 持続時間延長
    Shorten,        // 持続時間短縮
    LowGravity,    // 低重力
    AddGravity,      // 重力追加

    Pause,          // 一時停止
    Remote,         // 遠隔
    SelfHarm,       // 諸刃（自傷）
    Penetration,    // 透過性
    Elasticity,     // 弾性

    // --- 【修飾：軌道・方向変化】 ---
    // ホーミング
    Directivity,    // 指向
    Homing,         // 追尾
    TeleportHoming,       // 転位

    // --- 【マルチキャスト・フロー制御】 ---
    TwoChainCast,
    ThreeChainCast,
    FiveChainCast,

    TwoRandomChainCast,
    ThreeRandomChainCast,
    FiveRandomChainCast,
    Skip,           // 1マス飛ばし

    // 軌道変化
    Orbit,          // 公転
    Following,       // 追従

    AddTrigger,     // トリガー追加
    AddClickTrigger,// クリックトリガー追加

    CoolTimeShortenFlat,    // クールタイム短縮（固定）
    CoolTimeShorten,        // クールタイム短縮（倍率）

    TwoMultiplier,  // 2倍複製
    ThreeMultiplier,
    FiveMultiplier,
    TwoSplit, // 多重詠唱
    ThreeSplit,
    FiveSplit,
}