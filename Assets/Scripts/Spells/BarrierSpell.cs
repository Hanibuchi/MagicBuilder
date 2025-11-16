using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "BarrierSpell", menuName = "Wand System/Barrier Spell")]
public class BarrierSpell : SpellBase
{
    [Header("バリア設定")]
    [Tooltip("Strengthに応じて選択されるバリアプレハブ(補助用）の配列")]
    public GameObject[] barrierTrajectoryPrefabs;
    [Tooltip("Strengthに応じて選択されるバリアプレハブの配列")]
    public GameObject[] barrierPrefabs;

    // 軌道表示用のバッファ
    private GameObject currentBarrierInstance = null;
    private int lastPrefabIndex = -1; // 前回使用したPrefabのインデックス

    /// <summary>
    /// Strengthに応じてバリアのプレハブを選択し、軌道（Aiming Line）として表示します。
    /// </summary>
    public override void DisplayAimingLine(
        List<SpellBase> wandSpells, int currentSpellIndex, float rotationZ,
        float strength, Vector2 casterPosition, bool clearLine = false)
    {
        // 1. 補助線のクリア処理
        if (clearLine)
        {
            if (currentBarrierInstance != null)
            {
                // PoolManagerを使用していないため、非アクティブ化で対応
                currentBarrierInstance.SetActive(false);
            }
            return;
        }

        // 2. Strengthに基づくPrefabの選択
        // 配列の要素数に基づいてインデックスを計算 (0〜Length-1)
        int arrayLength = barrierTrajectoryPrefabs.Length;
        int targetIndex = Mathf.FloorToInt(strength * (arrayLength - 1));
        if (targetIndex < 0) targetIndex = 0;
        if (targetIndex >= arrayLength) targetIndex = arrayLength - 1;

        GameObject targetPrefab = barrierTrajectoryPrefabs[targetIndex];

        if (targetPrefab == null)
        {
            Debug.LogError($"呪文 [{spellName}] のインデックス [{targetIndex}] にPrefabが設定されていません。");
            // Prefabがない場合は表示をクリア
            if (currentBarrierInstance != null)
            {
                currentBarrierInstance.SetActive(false);
            }
            return;
        }

        // 3. インスタンスの再利用または新規取得
        if (targetIndex != lastPrefabIndex || currentBarrierInstance == null)
        {
            // 前回のインスタンスを破棄 (PoolManagerを使っていないため)
            if (currentBarrierInstance != null)
            {
                Destroy(currentBarrierInstance);
            }

            // 新しいバリアを生成
            currentBarrierInstance = Instantiate(targetPrefab);
            currentBarrierInstance.SetActive(true); // 新規作成時はアクティブ化
            lastPrefabIndex = targetIndex;
        }
        else if (!currentBarrierInstance.activeSelf)
        {
            // インスタンスを使い回す場合でも非アクティブならアクティブに戻す
            currentBarrierInstance.SetActive(true);
        }

        currentBarrierInstance.transform.rotation = Quaternion.Euler(0, 0, rotationZ); // rotationZをそのまま設定
        currentBarrierInstance.transform.position = casterPosition;
    }

    /// <summary>
    /// Strengthに応じて選択されたバリアを、軌道表示と同じ位置に生成します。
    /// </summary>
    public override void FireSpell(
        List<SpellBase> wandSpells, int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        // 1. Strengthに基づくPrefabの選択
        int arrayLength = barrierPrefabs.Length;
        int targetIndex = Mathf.FloorToInt(strength * (arrayLength - 1));
        if (targetIndex < 0) targetIndex = 0;
        if (targetIndex >= arrayLength) targetIndex = arrayLength - 1;

        GameObject targetPrefab = barrierPrefabs[targetIndex];

        if (targetPrefab == null)
        {
            Debug.LogError($"呪文 [{spellName}] の発射に失敗しました: インデックス [{targetIndex}] のPrefabが設定されていません。");
            return;
        }

        // **　バリアは casterPosition の方向 (rotationZ) を向く**
        Quaternion rotation = Quaternion.Euler(0, 0, rotationZ);

        // 3. プレハブを生成
        GameObject barrierGO = Instantiate(
            targetPrefab,
            context.CasterPosition,
            rotation
        );

        // 4. 速度は追加しない（静止バリアのため）

        // 5. 投射物修正ロジックの実行
        ModifyProjectile(context, barrierGO);

        Debug.Log($"[{spellName}]を発射！バリアの種類インデックス:{targetIndex}、角度:{rotationZ}°、強さ:{strength}");
    }
}