using UnityEngine;

/// <summary>
/// スコアを管理するシングルトンクラス。
/// </summary>
public class ScoreManager : MonoBehaviour
{
    // シングルトンインスタンス
    private static ScoreManager _instance;

    /// <summary>
    /// ScoreManagerのインスタンスを取得します。
    /// </summary>
    public static ScoreManager Instance => _instance;

    // 現在の総スコア
    private float totalScore = 0f;

    // スコア変更通知用のインスタンス
    private IScoreObserver scoreObserver;

    /// <summary>
    /// インスタンスの初期化と重複チェックを行います。
    /// </summary>
    private void Awake()
    {
        if (_instance == null)
        {
            // 初めてのインスタンスの場合、自分自身をインスタンスに設定
            _instance = this;
            // シーンをまたいで永続化する場合は DontDestroyOnLoad(gameObject); を追加
        }
        else if (_instance != this)
        {
            // 既にインスタンスが存在する場合、自分自身（新しい方）を破棄
            Debug.LogWarning("ScoreManagerのインスタンスが重複しています。新しい方を破棄します。");
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// 現在の総スコアを取得します。
    /// </summary>
    /// <returns>現在の総スコア</returns>
    public float GetTotalScore()
    {
        return totalScore;
    }

    /// <summary>
    /// スコアを追加し、オブザーバーに通知します。
    /// </summary>
    /// <param name="scoreToAdd">追加するスコア</param>
    public void AddScore(float scoreToAdd)
    {
        if (scoreToAdd > 0)
        {
            totalScore += scoreToAdd;
            Debug.Log($"スコアを追加しました: +{scoreToAdd}。合計スコア: {totalScore}");

            // オブザーバーが設定されていれば通知
            scoreObserver?.OnScoreChanged(totalScore, scoreToAdd);
        }
    }

    /// <summary>
    /// スコア変更を監視するオブザーバーを設定します。
    /// </summary>
    /// <param name="observer">IScoreObserverを実装したインスタンス</param>
    public void SetScoreObserver(IScoreObserver observer)
    {
        scoreObserver = observer;
    }
}

/// <summary>
/// スコアの変更を通知するためのインターフェース。
/// </summary>
public interface IScoreObserver
{
    /// <summary>
    /// スコアが変更されたときに呼び出されます。
    /// </summary>
    /// <param name="newTotalScore">変更後の総スコア</param>
    /// <param name="addedScore">今回追加されたスコア</param>
    void OnScoreChanged(float newTotalScore, float addedScore);
}