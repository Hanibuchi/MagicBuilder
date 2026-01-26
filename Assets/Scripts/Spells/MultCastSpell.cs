using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System;
using Unity.VisualScripting;

[CreateAssetMenu(fileName = "MultCastSpell", menuName = "Wand System/MultCast Spell")]
public class MultCastSpell : SpellBase
{
    [Header("マルチキャスト設定")]
    [Tooltip("この呪文からの相対的なインデックス（オフセット）の配列。1は次の呪文のまとまり。")]
    public int[] relativeSpellGroupOffsets = { 1 };

    [Tooltip("発射間隔のランダム性の係数。0のとき間隔0")]
    public float delayMultiplier = 0.05f;

    [Tooltip("各ターゲットグループへの相対的な位置オフセット。")]
    public Vector2[] positionOffsets = { Vector2.zero };

    [Header("魔法陣演出設定")]
    [Tooltip("魔法陣を表示してから発射するまでの待ち時間")]
    public float magicCircleDelay = 0.5f;

    // キャッシュ用
    [System.NonSerialized] private List<SpellBase> _lastWandSpells;
    [System.NonSerialized] private Dictionary<int, int[]> _cachedIndicesMap = new Dictionary<int, int[]>();

    private int[] GetTargetIndices(List<SpellBase> wandSpells, int currentSpellIndex)
    {
        // 呪文リストが変更された場合はキャッシュをクリア
        if (!AreSpellsEqual(_lastWandSpells, wandSpells))
        {
            _lastWandSpells = wandSpells != null ? new List<SpellBase>(wandSpells) : null;
            _cachedIndicesMap.Clear();
        }

        // 呪文の位置（インデックス）をキーにしてキャッシュを確認
        if (_cachedIndicesMap.TryGetValue(currentSpellIndex, out int[] cached))
        {
            return cached;
        }

        // キャッシュがない場合は計算して保存
        int[] result = GetAbsoluteIndicesFromSpellGroupArray(
            wandSpells,
            currentSpellIndex,
            relativeSpellGroupOffsets
        );
        _cachedIndicesMap[currentSpellIndex] = result;

        return result;
    }

    /// <summary>
    /// 2つの呪文リストが論理的に同一（中身の並びが同じ）か判定します。
    /// </summary>
    private bool AreSpellsEqual(List<SpellBase> a, List<SpellBase> b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a == null || b == null) return false;
        if (a.Count != b.Count) return false;

        for (int i = 0; i < a.Count; i++)
        {
            if (a[i] != b[i]) return false;
        }
        return true;
    }

    // ----------------------------------------------------------------------------------
    // 抽象メソッドの実装
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// 補助線（軌道予測）を表示します。次の呪文のまとまりへ処理を連鎖させます。
    /// </summary>
    public override void DisplayAimingLine(
        List<SpellBase> wandSpells,
        int currentSpellIndex,
        float rotationZ,
        float strength,
        SpellContext context,
        bool clearLine = false)
    {
        Debug.Log($"spells: {string.Join(", ", wandSpells.Select(s => s?.name ?? "null"))}");
        // relativeSpellGroupOffsetsに基づいて呼び出す次の呪文の絶対インデックスを取得
        int[] targetIndices = GetTargetIndices(wandSpells, currentSpellIndex);
        Debug.Log($"targetIndices: {string.Join(", ", targetIndices)}");

        Vector2 baseCasterPoint = context.CasterPosition;
        for (int i = 0; i < targetIndices.Length; i++)
        {
            int targetIndex = targetIndices[i];
            if (targetIndex >= 0 && targetIndex < wandSpells.Count)
            {
                Vector2 offset = (positionOffsets != null && i < positionOffsets.Length)
                    ? positionOffsets[i]
                    : Vector2.zero;

                SpellBase spellToDisplay = wandSpells[targetIndex];

                SpellContext newContext = context;
                if (i > 0)
                    newContext = context.Clone();
                newContext.CasterPosition = baseCasterPoint + offset;

                // 対象の呪文のDisplayAimingLineを呼び出し
                spellToDisplay?.DisplayAimingLine(
                    wandSpells,
                    targetIndex,
                    rotationZ,
                    strength,
                    newContext,
                    clearLine
                );
            }
        }
    }

    [SerializeField] float additionalErrorDegree = 2;

    /// <summary>
    /// 呪文を発射・実行します。このメソッド自体は非同期ではありませんが、
    /// コルーチンをキックして時間差発動を行います。
    /// </summary>
    public override void FireSpell(
        List<SpellBase> wandSpells,
        int currentSpellIndex,
        float rotationZ,
        float strength,
        SpellContext context)
    {
        int[] targetIndices = GetTargetIndices(wandSpells, currentSpellIndex);

        // 💡 変更点: 各ターゲット呪文の発射を独立したコルーチンとして実行者に依頼
        for (int i = 0; i < targetIndices.Length; i++)
        {
            int targetIndex = targetIndices[i];
            if (targetIndex >= 0 && targetIndex < wandSpells.Count)
            {
                Vector2 offset = (positionOffsets != null && i < positionOffsets.Length)
                    ? positionOffsets[i]
                    : Vector2.zero;

                SpellBase spellToFire = wandSpells[targetIndex];

                context.errorDegree += additionalErrorDegree;
                // Contextの複製（Executorも引き継ぐ）
                SpellContext newContext = context.Clone();
                newContext.CasterPosition += offset;

                // 独立した遅延発射コルーチンを開始
                if (SpellScheduler.Instance != null)
                {
                    SpellScheduler.Instance.StartCoroutine(
                            FireSingleSpellDelayed(
                                spellToFire,
                                wandSpells,
                                targetIndex,
                                rotationZ,
                                strength,
                                newContext
                            )
                        );
                }
                else
                {
                    Debug.LogError("SpellSchedulerがシーンに見つかりません。時間差発動ができません。");
                }
            }
        }
    }

    /// <summary>
    /// 💡 新しいコルーチン: 単一の呪文をランダムな遅延後に発射するロジック
    /// </summary>
    private IEnumerator FireSingleSpellDelayed(
        SpellBase spellToFire,
        List<SpellBase> wandSpells,
        int targetIndex,
        float rotationZ,
        float strength,
        SpellContext newContext)
    {
        // 💡 時間差の計算
        // GetGaussianRandomAngle() に変更したと仮定して元のコードを修正
        float randomDelay = GetGaussianRandom(delayMultiplier * newContext.errorDegree);
        float delayTime = Mathf.Max(0f, Mathf.Abs(randomDelay));

        // 待機（この時間が発射開始時点からの遅延時間となる）
        yield return new WaitForSeconds(delayTime);

        MagicCircle magicCircle = null;
        GameObject prefab = SpellCommonData.Instance?.magicCirclePrefab;

        // 💡 魔法陣の表示演出を追加
        if (prefab != null)
        {
            GameObject circleGo = Instantiate(prefab, newContext.CasterPosition, Quaternion.Euler(0, 0, rotationZ));
            magicCircle = circleGo.GetComponent<MagicCircle>();

            if (magicCircle != null)
            {
                // 要件: 完全に表示されるまでの時間(magicCircleDelay), サイズ1
                magicCircle.Show(magicCircleDelay);
                // 魔法陣が出てから少し待ってから発射
                yield return new WaitForSeconds(magicCircleDelay);
            }
        }

        // 待機後、呪文を発射
        spellToFire?.FireSpell(
            wandSpells,
            targetIndex,
            rotationZ,
            strength,
            newContext
        );

        // 消滅演出の開始 (表示と同じ時間をかけて消える)
        if (magicCircle != null)
        {
            magicCircle.Hide(magicCircleDelay);
        }

        // Debug.Log($"Spell at index {targetIndex} fired after {delayTime:F3} seconds.");
    }


    /// <summary>
    /// この呪文から直接的に呼び出される次の呪文の相対オフセットの配列を返します。
    /// これは、DisplayAimingLineForNextSpellsやFireSpellForNextSpellsが、
    /// 「どの呪文のまとまりの**最初**の呪文」を呼び出すかを決定するために使用されます。
    /// </summary>
    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells,
        int currentSpellIndex)
    {
        // GetAbsoluteIndicesFromSpellGroupArrayで絶対インデックスを取得
        int[] targetIndices = GetTargetIndices(wandSpells, currentSpellIndex);

        // 絶対インデックスを現在の呪文からの相対オフセットに変換して返す
        return targetIndices
            .Select(index => index - currentSpellIndex)
            .ToArray();
    }
}