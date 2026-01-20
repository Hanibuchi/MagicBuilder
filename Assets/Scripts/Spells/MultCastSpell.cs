using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Unity.VisualScripting;

[CreateAssetMenu(fileName = "MultCastSpell", menuName = "Wand System/MultCast Spell")]
public class MultCastSpell : SpellBase
{
    [Header("マルチキャスト設定")]
    [Tooltip("この呪文からの相対的なインデックス（オフセット）の配列。1は次の呪文のまとまり。")]
    public int[] relativeSpellGroupOffsets = { 1 };

    [Tooltip("発射間隔のランダム性の係数。0のとき間隔0")]
    public float delayMultiplier = 0.05f;

    // キャッシュ用
    [System.NonSerialized] private List<SpellBase> _lastWandSpells;
    [System.NonSerialized] private int _lastCurrentSpellIndex = -1;
    [System.NonSerialized] private int[] _cachedTargetIndices;

    private int[] GetTargetIndices(List<SpellBase> wandSpells, int currentSpellIndex)
    {
        // すでに同じ状況での計算結果があればそれを返す
        if (_lastCurrentSpellIndex == currentSpellIndex && _cachedTargetIndices != null && AreSpellsEqual(_lastWandSpells, wandSpells))
        {
            return _cachedTargetIndices;
        }

        // キャッシュを更新
        _lastWandSpells = wandSpells != null ? new List<SpellBase>(wandSpells) : null;
        _lastCurrentSpellIndex = currentSpellIndex;
        _cachedTargetIndices = GetAbsoluteIndicesFromSpellGroupArray(
            wandSpells,
            currentSpellIndex,
            relativeSpellGroupOffsets
        );

        return _cachedTargetIndices;
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
        Vector2 casterPosition,
        bool clearLine = false)
    {
        // relativeSpellGroupOffsetsに基づいて呼び出す次の呪文の絶対インデックスを取得
        int[] targetIndices = GetTargetIndices(wandSpells, currentSpellIndex);

        // 絶対インデックスを現在の呪文からの相対オフセットに変換し、連鎖処理を呼び出す
        int[] relativeOffsets = targetIndices
            .Select(index => index - currentSpellIndex)
            .ToArray();

        // DisplayAimingLineForNextSpellsを利用して、連鎖先のDisplayAimingLineを呼び出す
        // relativeOffsetsの配列は、連鎖の始点となる呪文の配列と解釈されます。
        DisplayAimingLineForNextSpells(
            relativeOffsets,
            wandSpells,
            currentSpellIndex,
            rotationZ,
            strength,
            casterPosition,
            clearLine
        );
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
        foreach (int targetIndex in targetIndices)
        {
            if (targetIndex >= 0 && targetIndex < wandSpells.Count)
            {
                SpellBase spellToFire = wandSpells[targetIndex];

                context.errorDegree += additionalErrorDegree;
                // Contextの複製（Executorも引き継ぐ）
                SpellContext newContext = context.Clone();

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

        // 待機後、呪文を発射
        spellToFire?.FireSpell(
            wandSpells,
            targetIndex,
            rotationZ,
            strength,
            newContext
        );

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