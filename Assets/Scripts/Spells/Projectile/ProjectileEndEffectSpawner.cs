using UnityEngine;

/// <summary>
/// Projectileの開始時など、特定GameObjectの生成時にエフェクトをスポーンさせるマネージャー。
/// </summary>
public class ProjectileEndEffectSpawner : MonoBehaviour, ISpellProjectileInitListener, ISpellProjectileDestroyListener
{
    [Header("起動設定")]
    [Tooltip("このコンポーネントがSpellProjectileDamageSourceから直接呼び出されたときにエフェクトをスポーンさせるかどうか。")]
    [SerializeField] private bool spawnOnDestroy = true;

    [Header("エフェクト設定")]
    [Tooltip("スポーンさせるエフェクトのPrefab。")]
    [SerializeField] private GameObject startEffectPrefab;

    [Tooltip("エフェクトをスポーンさせる、このオブジェクトからの相対位置。")]
    [SerializeField] private Vector3 localSpawnOffset = Vector3.zero;

    [Tooltip("エフェクトをスポーンさせる際、このオブジェクトの回転を適用するかどうか。")]
    [SerializeField] private bool matchRotation = true;

    [Tooltip("エフェクトをスポーンさせる際、このオブジェクトのProjectileModifierを適用するかどうか。")]

    [SerializeField] bool matchProjectileModifier = false;

    public void SpawnEndEffect()
    {
        float scale = transform.lossyScale.x;
        // 1. スポーン位置を計算
        // このGameObjectの座標に、ローカルオフセットを足すことでワールド座標を取得
        Vector3 spawnPosition = transform.TransformPoint(localSpawnOffset);

        // 2. スポーン回転を決定
        Quaternion spawnRotation = matchRotation ? transform.rotation : Quaternion.identity;

        // 3. エフェクトPrefabが設定されているか確認
        if (startEffectPrefab != null)
        {
            // 4. エフェクトをスポーン（生成）
            GameObject spawnedEffect = Instantiate(
                startEffectPrefab,
                spawnPosition,
                spawnRotation
            );

            // 補足: 親子関係を設定したい場合は以下を使用 (今回は要求されていないためコメントアウト)
            if (matchProjectileModifier && spellContext != null)
                spellContext.ProjectileModifier?.Invoke(spawnedEffect);
        }
        else
        {
            Debug.LogWarning($"CastEffectManager: {gameObject.name} に Start Effect Prefab が設定されていません。", this);
        }
    }

    SpellContext spellContext;
    public void Initialize(float strength, SpellContext spellContext)
    {
        this.spellContext = spellContext;
    }

    public void Destroy()
    {
        if (spawnOnDestroy)
            SpawnEndEffect();
    }
}