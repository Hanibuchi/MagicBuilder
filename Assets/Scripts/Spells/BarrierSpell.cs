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

    [Tooltip("バリアが生成されるキャスターからの最小距離")]
    [SerializeField] float minDistance = 1.0f;

    [Tooltip("バリアが生成されるキャスターからの最大距離")]
    [SerializeField] float maxDistance = 5.0f;

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

        // 4. 位置と回転の計算

        // 距離の線形補間
        // targetIndex/(Length-1) は、インデックスを 0.0〜1.0 の範囲に正規化する
        float normalizedIndex = (float)targetIndex / (arrayLength - 1);
        float distance = Mathf.Lerp(minDistance, maxDistance, normalizedIndex);

        // 角度をラジアンに変換
        float angleRad = rotationZ * Mathf.Deg2Rad;

        // X軸の正の方向 (rotationZ=0) からのオフセット位置を計算
        Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        Vector2 position = casterPosition + direction * distance;

        // 5. インスタンスの配置と回転設定

        currentBarrierInstance.transform.position = position;

        // **【修正点】Prefabは casterPosition の方向 (rotationZ) を向く**
        currentBarrierInstance.transform.rotation = Quaternion.Euler(0, 0, rotationZ); // rotationZをそのまま設定
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

        // 2. 位置と回転の計算
        // DisplayAimingLineと同じロジック
        float normalizedIndex = (float)targetIndex / (arrayLength - 1);
        float distance = Mathf.Lerp(minDistance, maxDistance, normalizedIndex);
        float angleRad = rotationZ * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        Vector2 position = context.CasterPosition + direction * distance;

        // **【修正点】バリアは casterPosition の方向 (rotationZ) を向く**
        Quaternion rotation = Quaternion.Euler(0, 0, rotationZ);

        // 3. プレハブを生成
        GameObject barrierGO = Instantiate(
            targetPrefab,
            position,
            rotation
        );

        // 4. 速度は追加しない（静止バリアのため）

        // 5. 投射物修正ロジックの実行
        ModifyProjectile(context, barrierGO);

        Debug.Log($"[{spellName}]を発射！バリアの種類インデックス:{targetIndex}、角度:{rotationZ}°、強さ:{strength}");
    }
}