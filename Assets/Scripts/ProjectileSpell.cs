using UnityEngine;
using System.Collections.Generic;


public class ProjectileSpell : SpellBase
{
    public override void DisplayAimingLine(
        List<SpellBase> wandSpells, int currentSpellIndex, float rotationZ,
        float strength, Vector2 casterPosition)
    {
        // 1. Z回転と強さから初速ベクトルを計算
        float angleRad = rotationZ * Mathf.Deg2Rad;
        Vector2 initialVelocity = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * strength;
        float gravityMagnitude = Physics2D.gravity.magnitude;
        // 2. 一定時間ごとに軌道上の点を計算し、trajectoryPrefabを生成
        for (float t = 0; t < maxPredictionTime; t += trajectoryPrefabInterval)
        {
            Vector2 position = CalculateTrajectoryPoint(casterPosition, initialVelocity, gravityMagnitude, t);
            // 実際にはここでプレハブをInstantiateする処理が入ります
            Instantiate(trajectoryPrefab, position, Quaternion.identity);
        }
    }
    [Header("投射物設定")]
    [Tooltip("発射する魔法弾のプレハブ。Rigidbody2Dが必要です。")]
    public GameObject projectilePrefab;

    public override void FireSpell(
        List<SpellBase> wandSpells, int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        if (projectilePrefab == null)
        {
            Debug.LogError($"呪文 [{spellName}] の発射に失敗しました: projectilePrefabが設定されていません。");
            return;
        }

        // 1. プレハブを生成
        GameObject projectileGO = Instantiate(
            projectilePrefab,
            context.CasterPosition,
            Quaternion.Euler(0, 0, rotationZ) // 初期のZ回転を設定
        );

        // 2. Rigidbody2Dを取得し、初速を計算して設定
        Rigidbody2D rb = projectileGO.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError($"プレハブ [{projectilePrefab.name}] に Rigidbody2D がありません。");
            Destroy(projectileGO);
            return;
        }

        // 角度 (rotationZ) をラジアンに変換
        float angleRad = rotationZ * Mathf.Deg2Rad;

        // Z軸回転と強さから初速ベクトルを計算 (X=Cos, Y=Sin)
        Vector2 initialVelocity = new Vector2(
            Mathf.Cos(angleRad),
            Mathf.Sin(angleRad)
        ) * strength; // strengthを強さとして乗算

        // 初速を適用
        rb.linearVelocity = initialVelocity;

        // 3. 進行方向を向くコンポーネントをアタッチ (ProjectileControllerを想定)
        // 進行方向に常にオブジェクトを向けるロジックは、このコンポーネントに実装します。
        ProjectileController pc = projectileGO.AddComponent<ProjectileController>();
        pc.Initialize(rb); // Rigidbody2Dを渡してトラッキングを開始

        Debug.Log($"[{spellName}]を発射！角度:{rotationZ}°、強さ:{strength}");
    }
}