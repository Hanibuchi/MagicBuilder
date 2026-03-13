using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BigBowSpell", menuName = "Wand System/Big Bow Spell")]
public class BigBowSpell : ExampleSpell
{
    [Header("弓の演出設定")]
    [Tooltip("出現する弓のプレハブ。MagicCircleクラスが必要です。")]
    [SerializeField] private MagicCircle bowPrefab;
    [Tooltip("弓が表示されてから発射されるまでの時間（秒）")]
    [SerializeField] private float bowDelay = 0.8f;
    [Tooltip("出現位置から弓をどれだけ前に出すか")]
    [SerializeField] private float bowDistance = 1.0f;
    [Tooltip("発射後に弓が残る時間（秒）")]
    [SerializeField] private float bowLingerTime = 0.3f;
    [Tooltip("弓のサイズをアニメーションさせず、最初から固定にするか")]
    [SerializeField] private bool useConstantScale = true;

    public override void FireSpell(
        List<SpellBase> wandSpells,
        List<ISpellCastListener> listeners,
        int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        if (SpellScheduler.Instance != null && bowPrefab != null)
        {
            SpellScheduler.Instance.StartCoroutine(FireWithBow(
                wandSpells, listeners, currentSpellIndex, rotationZ, strength, context));
        }
        else
        {
            // プレハブがない、あるいはSchedulerがない場合は即座に矢を発射
            base.FireSpell(wandSpells, listeners, currentSpellIndex, rotationZ, strength, context);
        }
    }

    private IEnumerator FireWithBow(
        List<SpellBase> wandSpells, List<ISpellCastListener> listeners, int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        // 発射方向を計算
        float angleRad = rotationZ * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

        // 弓の座標を計算（出現場所より少し前）
        Vector2 bowPos = context.CasterPosition + direction * bowDistance;

        // 弓を生成
        GameObject bowGo = Instantiate(bowPrefab.gameObject, bowPos, Quaternion.Euler(0, 0, rotationZ));
        MagicCircle magicCircle = bowGo.GetComponent<MagicCircle>();

        if (magicCircle != null)
        {
            // 修飾子の適用（弓にも拡大などの効果を乗せる）
            context.AimingModifier?.Invoke(bowGo);

            // 表示演出の開始。useConstantScaleがtrueならスケールアニメーションを無効にする
            magicCircle.Show(bowDelay, animateScale: !useConstantScale);
            // 表示完了まで待機
            yield return new WaitForSeconds(bowDelay);
        }

        // 親クラス（ExampleSpell）のFireSpellを呼び出して、実際に巨大な矢（Projectile）を発射
        base.FireSpell(wandSpells, listeners, currentSpellIndex, rotationZ, strength, context);

        if (magicCircle != null)
        {
            // 発射後に少し放置してから消す
            yield return new WaitForSeconds(bowLingerTime);
            // 消滅演出の開始
            magicCircle.Hide(bowDelay, animateScale: !useConstantScale);
        }
    }
}
