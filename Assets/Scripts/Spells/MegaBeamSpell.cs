using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MegaBeamSpell", menuName = "Wand System/Mega Beam Spell")]
public class MegaBeamSpell : ExampleSpell
{
    private Dictionary<(int index, int callId), GameObject> beamDisplayObjects = new();

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
}
