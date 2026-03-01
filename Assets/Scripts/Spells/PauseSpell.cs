using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[CreateAssetMenu(fileName = "PauseSpell", menuName = "Wand System/Pause Spell")]
public class PauseSpell : SpellBase
{
    [Header("一時停止設定")]
    [Tooltip("次の呪文が呼び出されるまでの待機時間（魔法陣の表示時間を含む）")]
    [SerializeField] private float pauseDuration = 2f;

    [Header("演出設定")]
    [Tooltip("魔法陣の表示にかける時間")]
    [SerializeField] private float magicCircleDisplayTime = 0.3f;

    public override void DisplayAimingLine(
        List<SpellBase> wandSpells, int currentSpellIndex, float rotationZ,
        float strength, SpellContext context,
        bool clearLine = false)
    {
        // 予測線は待機時間を考慮せず即座に表示
        DisplayAimingLineForNextSpells(
            GetNextSpellOffsets(wandSpells, currentSpellIndex),
            wandSpells, currentSpellIndex, rotationZ, strength, context, clearLine
        );
    }

    public override void FireSpell(
        List<SpellBase> wandSpells, int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        if (SpellScheduler.Instance != null)
        {
            SpellScheduler.Instance.StartCoroutine(
                FireNextDelayedCoroutine(wandSpells, currentSpellIndex, rotationZ, strength, context)
            );
        }
        else
        {
            Debug.LogError("SpellSchedulerが見つかりません。");
        }
    }

    private IEnumerator FireNextDelayedCoroutine(
        List<SpellBase> wandSpells, int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        // 魔法陣の表示タイミングを計算
        // pauseDuration が全体の待機時間なので、そこから魔法陣の表示時間を引いた分だけ最初に待つ
        float initialWait = Mathf.Max(0, pauseDuration - magicCircleDisplayTime);
        yield return new WaitForSeconds(initialWait);

        MagicCircle magicCircle = null;
        if (SpellCommonData.Instance != null && SpellCommonData.Instance.magicCirclePrefab != null)
        {
            GameObject go = Instantiate(
                SpellCommonData.Instance.magicCirclePrefab,
                context.CasterPosition,
                Quaternion.Euler(0, 0, rotationZ)
            );
            magicCircle = go.GetComponent<MagicCircle>();
            if (magicCircle != null)
            {
                // Modifierの色を適用
                Color circleColor = SpellCommonData.Instance.modifierColor;
                magicCircle.Show(magicCircleDisplayTime, color: circleColor);
            }
        }

        // 魔法陣を表示している時間（または残りの時間）待機
        yield return new WaitForSeconds(Mathf.Min(pauseDuration, magicCircleDisplayTime));

        // 次の呪文を発射
        FireSpellForNextSpells(
            GetNextSpellOffsets(wandSpells, currentSpellIndex),
            wandSpells, currentSpellIndex, rotationZ, strength, context
        );

        // 魔法陣を非表示にする
        if (magicCircle != null)
        {
            magicCircle.Hide(magicCircleDisplayTime);
        }
    }

    private readonly int[] _nextSpellOffsets = { 1 };
    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells, int currentSpellIndex)
    {
        return _nextSpellOffsets;
    }

    public override List<SpellDescriptionItem> GetDescriptionDetails()
    {
        base.GetDescriptionDetails();

        detailItems.Add(new SpellDescriptionItem
        {
            icon = SpellCommonData.Instance.coolDownIcon,
            descriptionText = $"待機時間 : {pauseDuration:F1} 秒"
        });

        return detailItems;
    }
}
