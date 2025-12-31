using System.Collections;
using UnityEngine;

/// <summary>
/// コインを入手したときの演出を管理するクラス。
/// Canvasにアタッチして使用します。
/// </summary>
public class CoinCollectionEffect : MonoBehaviour
{
    public static CoinCollectionEffect Instance { get; private set; }

    [Header("Settings")]
    [SerializeField, Tooltip("生成されるコインのプレハブ")]
    private GameObject coinPrefab;

    [SerializeField, Tooltip("コインが移動する目標地点")]
    private Transform targetTransform;

    [SerializeField, Tooltip("生成範囲の半径")]
    private float spawnRadius = 150f;

    [SerializeField, Tooltip("すべてのコインが生成されるまでの合計時間")]
    private float spawnDuration = 1.0f;

    [SerializeField, Tooltip("移動開始までの待機時間")]
    private float waitDuration = 0.5f;

    [SerializeField, Tooltip("目標地点までの移動時間")]
    private float moveDuration = 0.8f;

    [SerializeField, Tooltip("1枚のコインUIが表す金額（この値で割った数のコインが生成されます）")]
    private int amountPerCoin = 1;

    [SerializeField, Tooltip("一度に生成される最大コイン数")]
    private int maxCoinCount = 20;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// コイン取得演出を再生します。
    /// </summary>
    /// <param name="amount">取得したコインの総数</param>
    public void PlayEffect(int amount)
    {
        if (coinPrefab == null || targetTransform == null)
        {
            Debug.LogWarning("CoinCollectionEffect: Prefab or TargetTransform is not set.");
            return;
        }

        int coinCount = Mathf.Clamp(amount / amountPerCoin, 1, maxCoinCount);

        for (int i = 0; i < coinCount; i++)
        {
            float delay = Random.Range(0f, spawnDuration);
            StartCoroutine(SpawnAndAnimateCoin(delay));
        }
    }

    private IEnumerator SpawnAndAnimateCoin(float delay)
    {
        // 指定されたランダムな時間待機
        yield return new WaitForSeconds(delay);

        // 画面中央（このオブジェクトの位置）からランダムな位置に生成
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        GameObject coin = Instantiate(coinPrefab, transform);
        coin.transform.localPosition = randomOffset;

        // 一定時間その場で待機
        yield return new WaitForSeconds(waitDuration);

        Transform coinTransform = coin.transform;
        Vector3 startPos = coinTransform.position;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;

            // イージング（加速しながら移動）
            t = t * t;

            if (coinTransform == null) yield break;

            // 目標地点へ移動
            coinTransform.position = Vector3.Lerp(startPos, targetTransform.position, t);
            yield return null;
        }

        // 到着したら破棄
        if (coin != null)
        {
            Destroy(coin);
        }
    }

    public int test_amount = 5;
    public void Test()
    {
        PlayEffect(test_amount);
    }
}