using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "InfernoSpell", menuName = "Wand System/Inferno Spell")]
public class InfernoSpell : ExampleSpell
{
    [Header("隕石演出設定")]
    [Tooltip("出現する魔法陣のプレハブ")]
    [SerializeField] private MagicCircle magicCirclePrefab;
    [Tooltip("魔法陣が表示されてから発射されるまでの時間（秒）")]
    [SerializeField] private float magicCircleDelay = 1f;
    [Tooltip("出現位置から魔法陣をどれだけ前に出すか")]
    [SerializeField] private float magicCircleDistance = 2.0f;
    [Tooltip("発射後に魔法陣が残る時間（秒）")]
    [SerializeField] private float magicCircleLingerTime = 0.5f;

    public override void FireSpell(
        List<SpellBase> wandSpells, int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        if (SpellScheduler.Instance != null && magicCirclePrefab != null)
        {
            SpellScheduler.Instance.StartCoroutine(FireWithMagicCircle(
                wandSpells, currentSpellIndex, rotationZ, strength, context));
        }
        else
        {
            // 魔法陣がない、あるいはSchedulerがない場合は即座に発射
            base.FireSpell(wandSpells, currentSpellIndex, rotationZ, strength, context);
        }
    }

    private IEnumerator FireWithMagicCircle(
        List<SpellBase> wandSpells, int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        // 発射方向を計算
        float angleRad = rotationZ * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

        // 魔法陣の座標を計算（出現場所より少し前）
        Vector2 circlePos = context.CasterPosition + direction * magicCircleDistance;

        // 魔法陣を生成
        GameObject circleGo = Instantiate(magicCirclePrefab.gameObject, circlePos, Quaternion.Euler(0, 0, rotationZ));
        MagicCircle magicCircle = circleGo.GetComponent<MagicCircle>();

        if (magicCircle != null)
        {
            // 修飾子の適用（魔法陣にも拡大などの効果を乗せる）
            context.AimingModifier?.Invoke(circleGo);

            // 表示演出の開始
            magicCircle.Show(magicCircleDelay);
            // 表示完了まで待機
            yield return new WaitForSeconds(magicCircleDelay);
        }

        // 親クラス（ExampleSpell）のFireSpellを呼び出して、実際に弾（隕石）を発射
        base.FireSpell(wandSpells, currentSpellIndex, rotationZ, strength, context);

        if (magicCircle != null)
        {
            // 発射後に少し放置してから消す
            yield return new WaitForSeconds(magicCircleLingerTime);
            // 消滅演出の開始
            magicCircle.Hide(magicCircleDelay);
        }
    }
}
