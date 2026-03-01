using UnityEngine;
using System.Collections;

public class FireflyTriggerProjectile : SpellProjectileDamageSource, IClickTriggerFireListener
{
    [Header("蛍トリガー設定")]
    [Tooltip("ランダムに加える力の最大値")]
    [SerializeField] private float maxRandomForce = .1f;
    [Tooltip("力を加える間隔（秒）の最小値")]
    [SerializeField] private float minInterval = 0.5f;
    [Tooltip("力を加える間隔（秒）の最大値")]
    [SerializeField] private float maxInterval = 1.5f;
    [Tooltip("反対方向へ向かうバイアスの強さ。高いほどその場にとどまりやすくなります。")]
    [SerializeField] private float counterBias = 0.8f;

    private Rigidbody2D _rb;
    private Vector2 _lastImpulse;
    private bool _isFlying = true;

    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody2D>();
        
        if (_rb != null)
        {
            StartCoroutine(FlyCoroutine());
        }
    }

    /// <summary>
    /// クリックトリガーが発動した際に呼ばれる。
    /// </summary>
    public void OnFire()
    {
        Destroy();
    }

    private void LateUpdate()
    {
        // 常に rotation.z を 0 に固定する
        transform.rotation = Quaternion.identity;
    }

    private IEnumerator FlyCoroutine()
    {
        while (_isFlying)
        {
            // 次の方向転換まで待機
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            if (_rb == null) yield break;

            // ランダムな新しい方向を生成
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            
            // 前回の方向の逆ベクトルへのバイアスをかける
            // これにより、右に行った次は左に行きやすくなる
            Vector2 biasDir = -_lastImpulse * counterBias;
            
            // 最終的な力を計算
            Vector2 combinedDir = (randomDir + biasDir).normalized;
            float randomStrength = Random.Range(0f, maxRandomForce);
            
            Vector2 impulse = combinedDir * randomStrength;
            
            _rb.AddForce(impulse, ForceMode2D.Impulse);
            _lastImpulse = combinedDir; // 今回の方向を記録
        }
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        // 蛍トリガーは当たり判定を持たない（Collider2Dを持たない、またはTrigger設定）という要求だが、
        // 念のため衝突処理をオーバーライドして何もしないようにしておく
        // base.OnCollisionEnter2D(collision);
    }
}
