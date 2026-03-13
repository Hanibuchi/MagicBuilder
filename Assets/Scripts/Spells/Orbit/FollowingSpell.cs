using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "FollowingSpell", menuName = "Wand System/Following Spell")]
public class FollowingSpell : SpellBase
{
    [Header("追従設定")]
    [Tooltip("魔法陣を表示してから発射するまでの待ち時間")]
    public float magicCircleDelay = 0.5f;

    [Tooltip("バネ運動の振幅（中心からの最大距離）")]
    public float amplitude = 2f;

    [Tooltip("バネ運動の周波数（1秒間に何往復するか）")]
    public float frequency = 1f;

    [System.NonSerialized] private List<SpellBase> _lastWandSpells;
    [System.NonSerialized] private Dictionary<int, int[]> _cachedIndicesMap = new Dictionary<int, int[]>();

    private int[] GetTargetIndices(List<SpellBase> wandSpells, int currentSpellIndex)
    {
        if (!AreSpellsEqual(_lastWandSpells, wandSpells))
        {
            _lastWandSpells = wandSpells != null ? new List<SpellBase>(wandSpells) : null;
            _cachedIndicesMap.Clear();
        }

        if (_cachedIndicesMap.TryGetValue(currentSpellIndex, out int[] cached)) return cached;

        // 次の呪文(1)をメイン、その次(2, 3)を追従弾とする
        int[] result = GetAbsoluteIndicesFromSpellGroupArray(wandSpells, currentSpellIndex, new int[] { 1, 2, 3 });
        _cachedIndicesMap[currentSpellIndex] = result;
        return result;
    }

    private bool AreSpellsEqual(List<SpellBase> a, List<SpellBase> b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a == null || b == null || a.Count != b.Count) return false;
        return a.SequenceEqual(b);
    }

    public override void DisplayAimingLine(List<SpellBase> wandSpells, int currentSpellIndex, float rotationZ, float strength, SpellContext context, bool clearLine = false)
    {
        int[] targetIndices = GetTargetIndices(wandSpells, currentSpellIndex);
        if (targetIndices.Length < 1) return;

        int mainIndex = targetIndices[0];
        if (mainIndex >= 0 && mainIndex < wandSpells.Count)
        {
            wandSpells[mainIndex]?.DisplayAimingLine(wandSpells, mainIndex, rotationZ, strength, context, clearLine);
        }
    }

    public override void FireSpell(List<SpellBase> wandSpells,
        List<ISpellCastListener> listeners,
        int currentSpellIndex, float rotationZ, float strength, SpellContext context)
    {
        int[] targetIndices = GetTargetIndices(wandSpells, currentSpellIndex);
        if (targetIndices.Length < 1) return;

        int mainIndex = targetIndices[0];
        if (mainIndex >= 0 && mainIndex < wandSpells.Count)
        {
            SpellBase mainSpell = wandSpells[mainIndex];
            if (mainSpell == null) return;

            if (targetIndices.Length >= 2)
            {
                var followerIndices = targetIndices.Skip(1).ToArray();
                context.ProjectileModifier += (GameObject obj) =>
                {
                    if (obj != null)
                    {
                        var center = obj.AddComponent<FollowingCenter>();
                        center.Init(
                            followerIndices.Select(idx => wandSpells[idx]).ToList(),
                            wandSpells,
                            listeners,
                            followerIndices.ToList(),
                            new(context.layer),
                            magicCircleDelay,
                            amplitude,
                            frequency
                        );
                    }
                };
            }

            mainSpell.FireSpell(wandSpells, listeners, mainIndex, rotationZ, strength, context);
        }
    }

    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells, int currentSpellIndex)
    {
        int[] targetIndices = GetTargetIndices(wandSpells, currentSpellIndex);
        return targetIndices.Select(index => index - currentSpellIndex).ToArray();
    }
}
