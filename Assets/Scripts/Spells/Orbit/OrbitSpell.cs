using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "OrbitSpell", menuName = "Wand System/Orbit Spell")]
public class OrbitSpell : SpellBase
{
    [Header("演出設定")]
    [Tooltip("魔法陣を表示してから発射するまでの待ち時間")]
    public float magicCircleDelay = 0.5f;

    // キャッシュ用
    [System.NonSerialized] private List<SpellBase> _lastWandSpells;
    [System.NonSerialized] private Dictionary<int, int[]> _cachedIndicesMap = new Dictionary<int, int[]>();

    private int[] GetTargetIndices(List<SpellBase> wandSpells, int currentSpellIndex)
    {
        if (!AreSpellsEqual(_lastWandSpells, wandSpells))
        {
            _lastWandSpells = wandSpells != null ? new List<SpellBase>(wandSpells) : null;
            _cachedIndicesMap.Clear();
        }

        if (_cachedIndicesMap.TryGetValue(currentSpellIndex, out int[] cached))
        {
            return cached;
        }

        int[] result = GetAbsoluteIndicesFromSpellGroupArray(
            wandSpells,
            currentSpellIndex,
            new int[] { 1, 2, 3 }
        );
        _cachedIndicesMap[currentSpellIndex] = result;

        return result;
    }

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

    public override void DisplayAimingLine(
        List<SpellBase> wandSpells,
        int currentSpellIndex,
        float rotationZ,
        float strength,
        SpellContext context,
        bool clearLine = false)
    {
        int[] targetIndices = GetTargetIndices(wandSpells, currentSpellIndex);
        if (targetIndices.Length < 1) return;

        // 最初のターゲット（主呪文）のDisplayAimingLineを呼び出す
        int mainSpellIndex = targetIndices[0];
        if (mainSpellIndex >= 0 && mainSpellIndex < wandSpells.Count)
        {
            SpellBase mainSpell = wandSpells[mainSpellIndex];
            mainSpell?.DisplayAimingLine(
                wandSpells,
                mainSpellIndex,
                rotationZ,
                strength,
                context,
                clearLine
            );
        }

        // 衛星呪文の軌道予測は一旦省略（複雑になるため、主呪文のみ表示）
    }

    public override void FireSpell(
        List<SpellBase> wandSpells,
        int currentSpellIndex,
        float rotationZ,
        float strength,
        SpellContext context)
    {
        int[] targetIndices = GetTargetIndices(wandSpells, currentSpellIndex);
        if (targetIndices.Length < 1) return;

        int mainSpellIndex = targetIndices[0];
        if (mainSpellIndex >= 0 && mainSpellIndex < wandSpells.Count)
        {
            SpellBase mainSpell = wandSpells[mainSpellIndex];
            if (mainSpell == null) return;

            // 衛星がある場合、OrbitCenterを追加する
            if (targetIndices.Length >= 2)
            {
                var satelliteIndices = targetIndices.Skip(1).ToArray();

                context.ProjectileModifier += (GameObject obj) =>
                {
                    if (obj != null)
                    {
                        var orbitCenter = obj.AddComponent<OrbitCenter>();
                        orbitCenter.Init(
                            satelliteIndices.Select(idx => wandSpells[idx]).ToList(),
                            wandSpells,
                            satelliteIndices.ToList(),
                            new(context.layer),
                            magicCircleDelay
                        );
                    }
                };

                mainSpell.FireSpell(
                    wandSpells,
                    mainSpellIndex,
                    rotationZ,
                    strength,
                    context
                );
            }
            else
            {
                mainSpell.FireSpell(
                    wandSpells,
                    mainSpellIndex,
                    rotationZ,
                    strength,
                    context
                );
            }
        }
    }

    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells,
        int currentSpellIndex)
    {
        int[] targetIndices = GetTargetIndices(wandSpells, currentSpellIndex);
        return targetIndices.Select(index => index - currentSpellIndex).ToArray();
    }
}
