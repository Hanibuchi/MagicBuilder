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
    private class BarrierInstanceInfo
    {
        public GameObject instance;
        public int lastPrefabIndex = -1;
    }
    private Dictionary<int, BarrierInstanceInfo> barrierInstancesByIndex = new();

    /// <summary>
    /// Strengthに応じてバリアのプレハブを選択し、軌道（Aiming Line）として表示します。
    /// </summary>
    public override void DisplayAimingLine(
        List<SpellBase> wandSpells, int currentSpellIndex, float rotationZ,
        float strength, Vector2 casterPosition, Action<GameObject> aimingModifier,
        bool clearLine = false)
    {
        if (!barrierInstancesByIndex.TryGetValue(currentSpellIndex, out var info))
        {
            info = new BarrierInstanceInfo();
            barrierInstancesByIndex[currentSpellIndex] = info;
        }

        // 1. 補助線のクリア処理
        if (clearLine)
        {
            if (info.instance != null)
            {
                // PoolManagerを使用していないため、非アクティブ化で対応
                info.instance.SetActive(false);
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
            if (info.instance != null)
            {
                info.instance.SetActive(false);
            }
            return;
        }

        // 3. インスタンスの再利用または新規取得
        if (targetIndex != info.lastPrefabIndex || info.instance == null)
        {
            // 前回のインスタンスを破棄 (PoolManagerを使っていないため)
            if (info.instance != null)
            {
                Destroy(info.instance);
            }

            // 新しいバリアを生成
            info.instance = Instantiate(targetPrefab);
            info.instance.SetActive(true); // 新規作成時はアクティブ化
            info.lastPrefabIndex = targetIndex;
        }
        else if (!info.instance.activeSelf)
        {
            // インスタンスを使い回す場合でも非アクティブならアクティブに戻す
            info.instance.SetActive(true);
        }

        info.instance.transform.rotation = Quaternion.Euler(0, 0, rotationZ); // rotationZをそのまま設定
        info.instance.transform.position = casterPosition;
        // スケールを一旦プレハブのデフォルトに戻す（修飾子の累積適用を防ぐため）
        info.instance.transform.localScale = targetPrefab.transform.localScale;

        // 修飾子の実行（例：ExpansionSpellによる拡大など）
        aimingModifier?.Invoke(info.instance);
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

        float finalRotationZ = rotationZ + GetGaussianRandom(context.errorDegree);
        // **　バリアは casterPosition の方向 (rotationZ) を向く**
        Quaternion rotation = Quaternion.Euler(0, 0, finalRotationZ);

        // 3. プレハブを生成
        GameObject barrierGO = Instantiate(
            targetPrefab,
            context.CasterPosition,
            rotation
        );

        float totalDuration = context.duration + duration;
        context.duration = totalDuration;
        if (barrierGO.TryGetComponent(out CharacterHealth health))
        {
            health.maxHealth = barrierHP;
            if (!context.IsPermanent())
            {
                // バリアの場合は持続時間終了時に Kill(true) を呼び出す
                health.Invoke(nameof(health.KillSilently), context.duration);
            }
        }

        // 5. 投射物修正ロジックの実行
        ModifyProjectile(context, barrierGO);

        // CharacterHealth がない場合のみ従来の Destroy を使用（寿命管理）
        if (health == null && !context.IsPermanent())
        {
            Destroy(barrierGO, context.duration);
        }

        Debug.Log($"[{spellName}]を発射！バリアの種類インデックス:{targetIndex}、角度:{rotationZ}°、強さ:{strength}");
    }


    public void ModifyProjectile(SpellContext context, GameObject projectile)
    {
        context.ProjectileModifier?.Invoke(projectile);
    }
    [SerializeField] float barrierHP = 50;
    [Tooltip("呪文の持続時間（秒）。-1の場合は無限。")]
    [SerializeField] float duration = 10f;

    public override List<SpellDescriptionItem> GetDescriptionDetails()
    {
        base.GetDescriptionDetails();
        detailItems.Add(new SpellDescriptionItem
        {
            icon = SpellCommonData.Instance.HPIcon,
            descriptionText = "耐久値 : " + barrierHP.ToString(),
        });
        if (duration > 0)
            detailItems.Add(new SpellDescriptionItem
            {
                icon = null,
                descriptionText = "持続時間 : " + duration.ToString("F1") + " 秒",
            });
        return detailItems;
    }
}