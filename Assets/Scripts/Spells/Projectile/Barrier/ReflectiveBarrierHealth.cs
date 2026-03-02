using UnityEngine;

/// <summary>
/// 反射バリア用のHealthコンポーネント。
/// 通常のCharacterHealthとは異なり、衝突時のダメージ処理（HandleCollisionEnter/Stay）を無効化します。
/// 反射処理自体は別のコンポーネント（例: ReflectProjectile）で行われることを想定しています。
/// </summary>
public class ReflectiveBarrierHealth : CharacterHealth
{
    /// <summary>
    /// 衝突時の処理。反射バリアではダメージ判定をここで行わないため、何もしません。
    /// </summary>
    protected override void HandleCollisionEnter(GameObject obj)
    {
        // 何もしない
    }

    /// <summary>
    /// 接触継続時の処理。反射バリアではダメージ判定をここで行わないため、何もしません。
    /// </summary>
    protected override void HandleCollisionStay(GameObject obj)
    {
        // 何もしない
    }
}
