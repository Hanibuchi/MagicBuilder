using UnityEngine;
using System;

/// <summary>
/// 呪文の詳細説明パネルに表示する個別の項目データ。
/// アイコンと説明文のペアを持つ。
/// </summary>
[Serializable]
public class SpellDescriptionItem
{
    [Tooltip("項目に関連するアイコン")]
    public Sprite icon;

    [Tooltip("項目の説明文（例: クールタイム: 0.5秒）")]
    [TextArea(1, 3)]
    public string descriptionText;
}