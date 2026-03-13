using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

/// <summary>
/// 右側にある呪文を複数回連続で呼び出す多重詠唱呪文クラス。
/// </summary>
[CreateAssetMenu(fileName = "MultiSplit", menuName = "Wand System/Multi Split")]
public class MultiSplit : SpellBase
{
    [Tooltip("繰り返す回数")]
    public int repeatCount = 2;

    [Tooltip("発射間隔の係数。errorDegreeに掛け合わされ、等間隔の発射間隔(秒)になります。")]
    public float delayMultiplier = 0.2f;

    [Tooltip("各キャストへの相対的な位置オフセット。repeatCount 分指定可能です。")]
    public Vector2[] positionOffsets = { Vector2.zero };

    [Header("魔法陣演出設定")]
    [Tooltip("魔法陣を表示してから発射するまでの待ち時間")]
    public float magicCircleDelay = 0.5f;

    [SerializeField, Tooltip("追加の誤差角度")]
    float additionalErrorDegree = 2f;

    public MultiSplit()
    {
        category = SpellCategory.Other;
        spellName = "多重詠唱";
    }

    // ----------------------------------------------------------------------------------
    // SpellBase のオーバーライド
    // ----------------------------------------------------------------------------------

    public override void DisplayAimingLine(
        List<SpellBase> wandSpells,
        int currentSpellIndex,
        float rotationZ,
        float strength,
        SpellContext context,
        bool clearLine = false)
    {
        int targetIndex = currentSpellIndex + 1;
        if (targetIndex < 0 || targetIndex >= wandSpells.Count) return;

        SpellBase spellToDisplay = wandSpells[targetIndex];
        if (spellToDisplay == null) return;

        Vector2 baseCasterPoint = context.CasterPosition;

        for (int r = 0; r < repeatCount; r++)
        {
            Vector2 offset = (positionOffsets != null && r < positionOffsets.Length)
                ? positionOffsets[r]
                : Vector2.zero;

            // 発射角度 (rotationZ) に合わせてオフセットを回転させる
            Vector2 rotatedOffset = Quaternion.Euler(0, 0, rotationZ) * offset;

            SpellContext newContext = (r == 0) ? context : context.Clone();
            newContext.CasterPosition = baseCasterPoint + rotatedOffset;
            newContext.callId = r; // 繰り返し回数をIDとして設定

            spellToDisplay.DisplayAimingLine(
                wandSpells,
                targetIndex,
                rotationZ,
                strength,
                newContext,
                clearLine
            );
        }
    }

    public override void FireSpell(
        List<SpellBase> wandSpells,
        List<ISpellCastListener> listeners,
        int currentSpellIndex,
        float rotationZ,
        float strength,
        SpellContext context)
    {
        int targetIndex = currentSpellIndex + 1;
        if (targetIndex < 0 || targetIndex >= wandSpells.Count) return;

        SpellBase spellToFire = wandSpells[targetIndex];
        if (spellToFire == null) return;

        context.errorDegree += additionalErrorDegree;
        float interval = delayMultiplier * context.errorDegree;

        Vector2 baseCasterPoint = context.CasterPosition;

        for (int r = 0; r < repeatCount; r++)
        {
            Vector2 offset = (positionOffsets != null && r < positionOffsets.Length)
                ? positionOffsets[r]
                : Vector2.zero;

            // 発射角度 (rotationZ) に合わせてオフセットを回転させる
            Vector2 rotatedOffset = Quaternion.Euler(0, 0, rotationZ) * offset;

            // 最初の発射の時は元のSpellContextを使用し、それ以外はCloneする
            SpellContext newContext = (r == 0) ? context : context.Clone();
            newContext.CasterPosition = baseCasterPoint + rotatedOffset;
            newContext.callId = r;

            float delayTime = r * interval;

            if (SpellScheduler.Instance != null)
            {
                SpellScheduler.Instance.StartCoroutine(
                    FireSingleSpellDelayed(
                        spellToFire,
                        wandSpells,
                        listeners,
                        targetIndex,
                        rotationZ,
                        strength,
                        newContext,
                        delayTime
                    )
                );
            }
            else
            {
                Debug.LogError("SpellSchedulerが見つかりません。");
            }
        }
    }

    private IEnumerator FireSingleSpellDelayed(
        SpellBase spellToFire,
        List<SpellBase> wandSpells,
        List<ISpellCastListener> listeners,
        int targetIndex,
        float rotationZ,
        float strength,
        SpellContext newContext,
        float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        MagicCircle magicCircle = null;
        GameObject prefab = SpellCommonData.Instance?.magicCirclePrefab;

        if (prefab != null)
        {
            GameObject circleGo = Instantiate(prefab, newContext.CasterPosition, Quaternion.Euler(0, 0, rotationZ));
            magicCircle = circleGo.GetComponent<MagicCircle>();

            if (magicCircle != null)
            {
                // SpellCommonData.Instance.otherColor を使用
                Color color = SpellCommonData.Instance.otherColor;
                magicCircle.Show(magicCircleDelay, color: color);
                yield return new WaitForSeconds(magicCircleDelay);
            }
        }

        spellToFire.FireSpell(
            wandSpells,
            listeners, targetIndex,
            rotationZ,
            strength,
            newContext
        );

        if (magicCircle != null)
        {
            magicCircle.Hide(magicCircleDelay);
        }
    }

    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells,
        int currentSpellIndex)
    {
        // 常に一つ右の呪文を指す（複雑なグループ計算は行わない）
        return new int[] { 1 };
    }
}

