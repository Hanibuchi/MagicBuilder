using UnityEngine;

/// <summary>
/// キャラクター全体で共通のデータや設定を管理するScriptableObject。
/// </summary>
[CreateAssetMenu(fileName = "CharacterCommonData", menuName = "Stats/Character Common Data")]
public class CharacterCommonData : ScriptableObject
{
    private static CharacterCommonData _instance;

    /// <summary>
    /// CharacterCommonDataのシングルトンインスタンスを取得します。
    /// 初めてアクセスされた時に、"Resources"フォルダからアセットをロードします。
    /// </summary>
    public static CharacterCommonData Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<CharacterCommonData>("CharacterCommonData");
                if (_instance == null)
                {
                    Debug.LogWarning("CharacterCommonData asset not found in 'Resources' folder. Using default settings.");
                }
            }
            return _instance;
        }
    }

    [Header("ダメージ蓄積設定")]
    [Tooltip("ダメージを適用するまで蓄積するフレーム数のデフォルト値")]
    public int defaultAccumulationFrames = 10;
}
