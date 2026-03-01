using UnityEngine;

[CreateAssetMenu(fileName = "CreditData", menuName = "GameData/Credit Data")]
public class CreditData : ScriptableObject
{
    [TextArea(10, 20)]
    public string creditTextContent = "ゲームタイトル\n\nディレクター: [名前]\nプログラマー: [名前]\nアーティスト: [名前]\n\n特別な感謝: [協力者]\n\n© 2025 [会社名]";
}