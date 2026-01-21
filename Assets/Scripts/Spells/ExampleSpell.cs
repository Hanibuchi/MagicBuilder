using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "ExampleSpell", menuName = "Wand System/Example Spell")]
public class ExampleSpell : SpellBase
{
    List<GameObject> trajectoryPrefabs = new();
    [Header("補助線設定")]
    [Tooltip("軌道プレハブの生成間隔（秒）。小さいほど密になります。")]
    public float trajectoryPrefabInterval = 0.1f;
    [Tooltip("⚡ 軌道予測を行う最大の時間（秒）。この時間を超える軌道は計算しません。")]
    public float maxPredictionTime = 2.0f;

    public override void DisplayAimingLine(
        List<SpellBase> wandSpells, int currentSpellIndex, float rotationZ,
        float strength, Vector2 casterPosition, Action<GameObject> aimingModifier,
        bool clearLine = false)
    {
        if (clearLine)
        {
            foreach (var obj in trajectoryPrefabs)
            {
                if (obj != null)
                    PoolManager.Instance.ReturnToPool(PoolType.Trajectory, obj);
            }
            trajectoryPrefabs.Clear();
            return;
        }
        // 1. Z回転と強さから初速ベクトルを計算
        float angleRad = rotationZ * Mathf.Deg2Rad;
        Vector2 initialVelocity = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * strength * strengthMultiplier;
        float gravityMagnitude = Physics2D.gravity.magnitude;
        // 2. 一定時間ごとに軌道上の点を計算し、trajectoryPrefabを生成
        foreach (var obj in trajectoryPrefabs)
        {
            if (obj != null)
                PoolManager.Instance.ReturnToPool(PoolType.Trajectory, obj);
        }
        trajectoryPrefabs.Clear();
        for (float t = 0; t < maxPredictionTime; t += trajectoryPrefabInterval)
        {
            if (t == 0)
                continue;
            Vector2 position = CalculateTrajectoryPoint(casterPosition, initialVelocity, gravityMagnitude, t);
            // ここでプールから軌道表示用オブジェクトを取得
            GameObject trajectoryObj = PoolManager.Instance.GetFromPool(PoolType.Trajectory);
            trajectoryPrefabs.Add(trajectoryObj);
            trajectoryObj.transform.position = position;

            // 修飾子の実行（例：ExpansionSpellによる拡大など）
            aimingModifier?.Invoke(trajectoryObj);
        }
    }
    [Header("投射物設定")]
    [Tooltip("発射する魔法弾のプレハブ。Rigidbody2Dが必要です。")]
    public GameObject projectilePrefab;
    [SerializeField] float strengthMultiplier = 20f;

    [Header("誤差設定")]
    [Tooltip("発射角に追加する誤差の標準偏差（度）。平均0の正規分布に従います。")]
    [SerializeField] float errorDegree = 1f; // 例として1度を設定
    [SerializeField] Damage damage; // 例として1度を設定
    [Header("説明設定")]
    [SerializeField] DamageSourceType damageType;

    public override void FireSpell(
        List<SpellBase> wandSpells, int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        if (projectilePrefab == null)
        {
            Debug.LogError($"呪文 [{spellName}] の発射に失敗しました: projectilePrefabが設定されていません。");
            return;
        }

        // 誤差を元の角度に追加
        float finalRotationZ = rotationZ + GetGaussianRandom(errorDegree + context.errorDegree);

        // 1. プレハブを生成
        GameObject projectileGO = Instantiate(
            projectilePrefab,
            context.CasterPosition,
            Quaternion.Euler(0, 0, finalRotationZ) // 初期のZ回転を設定
        );

        context.damage += damage;
        projectileGO.GetComponent<SpellProjectileDamageSource>().Initialize(strength, context);

        // 2. Rigidbody2Dを取得し、初速を計算して設定
        Rigidbody2D rb = projectileGO.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError($"プレハブ [{projectilePrefab.name}] に Rigidbody2D がありません。");
            Destroy(projectileGO);
            return;
        }
        // 角度 (finalRotationZ) をラジアンに変換
        float angleRad = finalRotationZ * Mathf.Deg2Rad;

        // Z軸回転と強さから初速ベクトルを計算 (X=Cos, Y=Sin)
        Vector2 initialVelocity = new Vector2(
            Mathf.Cos(angleRad),
            Mathf.Sin(angleRad)
        ) * strength * strengthMultiplier; // strengthを強さとして乗算

        // 初速を適用
        rb.linearVelocity = initialVelocity;

        if (isClickTrigger)
        {
            context.ProjectileModifier += ClickTriggerSpell.CreateClickTriggerAction(wandSpells, currentSpellIndex, context);
        }

        ModifyProjectile(context, projectileGO);

        Debug.Log($"[{spellName}]を発射！角度:{finalRotationZ}°、強さ:{strength}");
    }

    [SerializeField] bool isClickTrigger = false;

    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells,
        int currentSpellIndex)
    {
        if (isClickTrigger)
            return new int[] { 1 };
        else
            return new int[0];
    }

    public override List<SpellDescriptionItem> GetDescriptionDetails()
    {
        base.GetDescriptionDetails();
        if (damage.baseDamage != 0)
            detailItems.Add(new SpellDescriptionItem
            {
                icon = SpellCommonData.Instance.damageIcon,
                descriptionText = "ダメージ : " + damage.baseDamage.ToString(),
            });
        if (damage.FireDamage > 0)
            detailItems.Add(new SpellDescriptionItem
            {
                icon = SpellCommonData.Instance.fireDamageIcon,
                descriptionText = "炎ダメージ : " + damage.FireDamage.ToString(),
            });
        if (damage.IceDamage > 0)
            detailItems.Add(new SpellDescriptionItem
            {
                icon = SpellCommonData.Instance.iceDamageIcon,
                descriptionText = "氷ダメージ : " + damage.IceDamage.ToString(),
            });
        if (damage.waterDamage != 0)
            detailItems.Add(new SpellDescriptionItem
            {
                icon = SpellCommonData.Instance.waterDamageIcon,
                descriptionText = "水ダメージ : " + damage.waterDamage.ToString(),
            });
        if (damage.woodDamage != 0)
            detailItems.Add(new SpellDescriptionItem
            {
                icon = SpellCommonData.Instance.woodDamageIcon,
                descriptionText = "木ダメージ : " + damage.woodDamage.ToString(),
            });
        if (damage.knockback > 0)
            detailItems.Add(new SpellDescriptionItem
            {
                icon = SpellCommonData.Instance.knockbackIcon,
                descriptionText = "ノックバック : " + damage.knockback.ToString(),
            });
        if (errorDegree != 0)
            detailItems.Add(new SpellDescriptionItem
            {
                icon = SpellCommonData.Instance.errorDegreeIcon,
                descriptionText = "誤差 : " + errorDegree.ToString() + "度",
            });
        switch (damageType)
        {
            case DamageSourceType.SingleHit:
                detailItems.Add(new SpellDescriptionItem
                {
                    icon = null,
                    descriptionText = "ダメージタイプ : 単発ヒット",
                });
                break;
            case DamageSourceType.AreaOfEffect:
                detailItems.Add(new SpellDescriptionItem
                {
                    icon = null,
                    descriptionText = "ダメージタイプ : 範囲攻撃",
                });
                break;
            case DamageSourceType.MultiHit:
                detailItems.Add(new SpellDescriptionItem
                {
                    icon = null,
                    descriptionText = "ダメージタイプ : 多段ヒット",
                });
                break;
            default:
                Debug.LogWarning("Unknown DamageSourceType");
                break;

        }

        return detailItems;
    }
}