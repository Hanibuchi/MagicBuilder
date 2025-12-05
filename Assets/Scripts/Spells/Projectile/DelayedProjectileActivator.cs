using UnityEngine;
using System.Collections;

/// <summary>
/// 起動エフェクトをスポーンさせた後、Projectile本体を一定時間遅延させて起動するコンポーネント。
/// ProjectileのRigidbody2Dの動きを遅延させるために使用されます。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))] // Rigidbody2Dが必要です
public class DelayedProjectileActivator : MonoBehaviour
{
    [Header("遅延設定")]
    [Tooltip("Projectileの動きをアクティブにするまでの遅延時間（秒）。")]
    [SerializeField] private float activationDelay = 1.0f; // 例として0.5秒を設定

    [Header("エフェクト設定")]
    [Tooltip("このGameObjectのAwake時に呼び出すエフェクトスポナー。")]
    [SerializeField] private ProjectileStartEffectSpawner startEffectSpawner;
    private Vector2 initialVelocity;

    void Start()
    {
        // 2. ProjectileStartEffectSpawnerでエフェクトをスポーンさせる
        if (startEffectSpawner != null)
        {
            // startEffectSpawnerのspawnOnStartがtrueならStart()で自動的に呼ばれるが、
            // より確実にAwake/Startのタイミングで呼び出すために明示的に呼び出す。
            startEffectSpawner.SpawnStartEffect();
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: ProjectileStartEffectSpawnerが設定されていません。遅延のみ実行します。");
        }

        if (TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            // 3. ExampleSpell.csで設定された初期速度を保存する (Start()実行時には速度が設定済みのはず)
            initialVelocity = rb.linearVelocity;

        // 4. 遅延起動コルーチンを開始
        if (SpellScheduler.Instance != null)
        {
            SpellScheduler.Instance.StartSpellCoroutine(ActivateAfterDelay());
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("SpellScheduler.Instanceがシーンに見つかりません。Projectileを即時起動します。");
            ActivateProjectile();
        }
    }

    /// <summary>
    /// 遅延後にProjectileをアクティブにするコルーチン。
    /// </summary>
    private IEnumerator ActivateAfterDelay()
    {
        // 指定された時間待機
        yield return new WaitForSeconds(activationDelay);

        // Projectileをアクティブにする
        ActivateProjectile();
    }

    /// <summary>
    /// Projectileの物理挙動を再開し、初速を適用する。
    /// </summary>
    private void ActivateProjectile()
    {
        gameObject.SetActive(true);
        Debug.Log("velocity: " + initialVelocity);
        if (TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            // 3. ExampleSpell.csで設定された初期速度を保存する (Start()実行時には速度が設定済みのはず)
            rb.linearVelocity = initialVelocity;
        if (TryGetComponent(out SpellProjectileDamageSource component))
            component.PlayLaunchSound();
    }
}