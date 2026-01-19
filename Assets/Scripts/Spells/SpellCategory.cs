using UnityEngine;

/// <summary>
/// 呪文の種別を表す列挙型。主に見た目（UIの色など）の識別に利用されます。
/// </summary>
public enum SpellCategory
{
    Attack,     // 攻撃呪文
    Modifier,   // 修飾呪文
    Branch,     // 分岐呪文
    Other       // その他
}
