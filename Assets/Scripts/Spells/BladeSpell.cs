using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "BladeSpell", menuName = "Wand System/Blade Spell")]
public class BladeSpell : ExampleSpell
{
    [Header("ブレイド予測表示設定")]
    [Tooltip("回転の最小角度（相対）")]
    [SerializeField] private float minRotationAngle = -45f;
    [Tooltip("回転の最大角度（相対）")]
    [SerializeField] private float maxRotationAngle = 45f;
    [Tooltip("片道の回転にかかる時間（秒）")]
    [SerializeField] private float sweepDuration = 0.25f;

    private Dictionary<(int index, int callId), GameObject> bladeDisplayObjects = new();

    public override void DisplayAimingLine(
        List<SpellBase> wandSpells, int currentSpellIndex, float rotationZ,
        float strength, SpellContext context,
        bool clearLine = false)
    {
        var key = (currentSpellIndex, context.callId);

        // 1. お掃除処理
        if (clearLine)
        {
            if (bladeDisplayObjects.TryGetValue(key, out var obj))
            {
                if (obj != null)
                    PoolManager.Instance.ReturnToPool(PoolType.BladeTrajectory, obj);
                bladeDisplayObjects.Remove(key);
            }
            return;
        }

        // 2. オブジェクトの準備（1つだけ生成・再利用）
        if (!bladeDisplayObjects.TryGetValue(key, out var bladeObj) || bladeObj == null)
        {
            bladeObj = PoolManager.Instance.GetFromPool(PoolType.BladeTrajectory);
            if (bladeObj == null) return;
            bladeDisplayObjects[key] = bladeObj;
        }

        // 3. 位置の更新
        bladeObj.transform.position = context.CasterPosition;

        // 4. 角度の計算（往復運動）
        float t = 0f;
        if (sweepDuration > 0)
        {
            // PingPongを使用して 0.0 -> sweepDuration -> 0.0 の値を生成し、0.0〜1.0に正規化
            t = Mathf.PingPong(Time.time, sweepDuration) / sweepDuration;
        }
        
        // 角度を -45 〜 45 の範囲で補間
        float angleOffset = Mathf.Lerp(minRotationAngle, maxRotationAngle, t);
        bladeObj.transform.rotation = Quaternion.Euler(0, 0, rotationZ + angleOffset);

        // 5. 修飾子の適用（拡大など）
        ApplyTrajectoryModifier(bladeObj, context);
    }
}
