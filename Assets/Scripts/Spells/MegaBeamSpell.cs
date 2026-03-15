using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MegaBeamSpell", menuName = "Wand System/Mega Beam Spell")]
public class MegaBeamSpell : ExampleSpell
{
    private Dictionary<(int index, int callId), GameObject> beamDisplayObjects = new();

    [Header("ビーム発射前演出")]
    [SerializeField] private GameObject chargeEffectPrefab;
    [SerializeField] private float chargeTime = 1.0f;

    public override void DisplayAimingLine(
        List<SpellBase> wandSpells, int currentSpellIndex, float rotationZ,
        float strength, SpellContext context,
        bool clearLine = false)
    {
        var key = (currentSpellIndex, context.callId);

        // 1. お掃除処理
        if (beamDisplayObjects.TryGetValue(key, out var oldObj))
        {
            if (oldObj != null)
                PoolManager.Instance.ReturnToPool(PoolType.MegaBeamTrajectory, oldObj);
            beamDisplayObjects.Remove(key);
        }

        if (clearLine)
        {
            return;
        }

        // 2. オブジェクトの準備
        GameObject beamObj = PoolManager.Instance.GetFromPool(PoolType.MegaBeamTrajectory);
        if (beamObj == null) return;
        beamDisplayObjects[key] = beamObj;

        // --- 修正: モディファイアを先に適用して正確なスケールを確定させる ---
        // 4. 修飾子の適用（拡大など）
        ApplyTrajectoryModifier(beamObj, context);

        // 3. メガビームの予測表示を更新
        if (beamObj.TryGetComponent<MegaBeamTrajectory>(out var trajectory))
        {
            float angleRad = rotationZ * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            
            // 正確なスケールに基づいてビームを計算
            trajectory.UpdateBeam(context.CasterPosition, direction);
        }
    }

    public override void FireSpell(List<SpellBase> wandSpells,
        List<ISpellCastListener> listeners,
        int currentSpellIndex, float rotationZ, float strength, SpellContext context)
    {
        // 演出がない、または時間が0以下の場合は即座に発射
        if (chargeEffectPrefab == null || chargeTime <= 0f)
        {
            base.FireSpell(wandSpells, listeners, currentSpellIndex, rotationZ, strength, context);
            return;
        }

        // 演出用のコルーチンを開始 (SpellScheduler経由)
        SpellScheduler.Instance.StartSpellCoroutine(ChargeAndFire(wandSpells, listeners, currentSpellIndex, rotationZ, strength, context));
    }

    private System.Collections.IEnumerator ChargeAndFire(List<SpellBase> wandSpells, List<ISpellCastListener> listeners, int currentSpellIndex, float rotationZ, float strength, SpellContext context)
    {
        // 1. 演出用プレハブを生成
        GameObject chargeEffect = Instantiate(chargeEffectPrefab, context.CasterPosition, Quaternion.Euler(0, 0, rotationZ));

        // 2. AimingModifierを適用
        context.AimingModifier?.Invoke(chargeEffect);

        // 3. 一定時間待機
        yield return new WaitForSeconds(chargeTime);

        // 5. 実際のFireSpellを実行
        base.FireSpell(wandSpells, listeners, currentSpellIndex, rotationZ, strength, context);
    }
}
