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

    [Header("多段ヒット設定")]
    [Tooltip("多段ヒット（MultiHit）でダメージを受ける間隔（呼び出し回数）")]
    public int multiHitIntervalCount = 5;

    [Header("死亡時ヒットストップ設定")]
    [Tooltip("死亡した際のヒットストップ時間（秒）")]
    public float hitStopDurationOnDie = 0.15f;
    [Tooltip("死亡時のヒットストップ中のタイムスケール（0で完全停止）")]
    public float hitStopTimeScaleOnDie = 0f;

    [Header("窒息ダメージ設定")]
    [Tooltip("地形に埋まってからダメージを受け始めるまでの猶予時間（秒）")]
    public float suffocationDelay = 0.5f;
    [Tooltip("地形に埋まった際の1フレームあたりのダメージ量")]
    public float suffocationDamagePerFrame = 0.5f;
}
