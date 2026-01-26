// ファイル名: EnemyCounter.cs
using UnityEngine;

public class EnemyCounter : MonoBehaviour
{
    // ⚔️ シングルトンインスタンス
    private static EnemyCounter _instance;
    public static EnemyCounter Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("EnemyCounter");
                _instance = go.AddComponent<EnemyCounter>();
            }
            return _instance;
        }
    }

    // 🛡️ 敵の数（内部カウンタ）
    [SerializeField] int _currentEnemyCount = 0;

    // 📜 ゼロ通知用のインターフェース（外部からセット可能）
    // ご要望通り、1つだけセットできるようにします
    private IZeroEnemyNotifier _zeroNotifier;

    // シーンをまたいでもインスタンスが破棄されないように設定
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        if (_instance != this)
        {
            Destroy(gameObject); // 既に別のインスタンスが存在する場合は自身を破棄
        }
    }

    // 🔢 現在の敵の数を外部から取得
    public int CurrentEnemyCount
    {
        get { return _currentEnemyCount; }
    }

    // --- 外部から呼び出すメソッド ---

    // ➕ カウンタを1増やす (新しい敵が生成された時など)
    public void AddEnemy()
    {
        _currentEnemyCount++;
        Debug.Log($"敵が1体増えました。現在の敵の数: {_currentEnemyCount}");
    }

    // ➖ カウンタを1減らす (敵が倒された時など)
    public void RemoveEnemy()
    {
        // 0より大きい場合にのみ減算する（安全のため）
        if (_currentEnemyCount > 0)
        {
            _currentEnemyCount--;
            Debug.Log($"敵が1体減りました。現在の敵の数: {_currentEnemyCount}");

            // ゼロになったかチェックし、通知メソッドを呼び出す
            if (_currentEnemyCount == 0)
            {
                _zeroNotifier?.OnEnemyCountZero();
                Debug.Log("🎉 敵の数が0になりました！通知を実行。");
            }
        }
        else
        {
            // 既に0以下の場合はログなどで警告しても良い
            Debug.LogWarning("敵の数が既に0以下です。");
        }
    }

    // 📢 ゼロ通知インターフェースを外部からセット
    public void SetZeroNotifier(IZeroEnemyNotifier notifier)
    {
        _zeroNotifier = notifier;
    }
}

// ファイル名: IZeroEnemyNotifier.cs
public interface IZeroEnemyNotifier
{
    // 敵の数が0になったときに呼び出されるメソッド
    void OnEnemyCountZero();
}