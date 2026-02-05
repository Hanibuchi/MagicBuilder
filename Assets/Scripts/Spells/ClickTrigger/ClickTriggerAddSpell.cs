using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

[CreateAssetMenu(fileName = "ClickTriggerAddSpell", menuName = "Wand System/Click Trigger Add Spell")]
public class ClickTriggerAddSpell : SpellBase
{
    private static ClickTriggerAddSpell _instance;
    public static ClickTriggerAddSpell Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ClickTriggerAddSpell>("Spells/ClickTriggerAddSpell");
            }
            return _instance;
        }
    }

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

        // 命令: GetAbsoluteIndicesFromSpellGroupArrayにint[] {1, 2}を渡した値を使用する
        int[] result = GetAbsoluteIndicesFromSpellGroupArray(
            wandSpells,
            currentSpellIndex,
            new int[] { 1, 2 }
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

        // 最初のターゲット（投射呪文）のDisplayAimingLineを呼び出す
        int projectileSpellIndex = targetIndices[0];
        if (projectileSpellIndex >= 0 && projectileSpellIndex < wandSpells.Count)
        {
            SpellBase projectileSpell = wandSpells[projectileSpellIndex];
            if (projectileSpell != null)
            {
                projectileSpell.DisplayAimingLine(
                    wandSpells,
                    projectileSpellIndex,
                    rotationZ,
                    strength,
                    context,
                    clearLine
                );
            }
        }
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

        int projectileSpellIndex = targetIndices[0];
        if (projectileSpellIndex >= 0 && projectileSpellIndex < wandSpells.Count)
        {
            SpellBase projectileSpell = wandSpells[projectileSpellIndex];
            if (projectileSpell != null)
            {
                if (targetIndices.Length >= 2)
                {
                    int triggerSpellIndex = targetIndices[1];
                    SpellBase triggerSpell = wandSpells[triggerSpellIndex];
                    if (triggerSpell != null)
                    {
                        context.ProjectileModifier += CreateClickTriggerAction(wandSpells, currentSpellIndex, context);
                    }
                }

                projectileSpell.FireSpell(
                    wandSpells,
                    projectileSpellIndex,
                    rotationZ,
                    strength,
                    context
                );
            }
        }
    }

    public static Action<GameObject> CreateClickTriggerAction(
        List<SpellBase> wandSpells,
        int currentSpellIndex,
        SpellContext context)
    {
        return (GameObject obj) =>
        {
            if (obj == null) return;

            if (currentSpellIndex >= 0 && currentSpellIndex < wandSpells.Count)
            {
                var spell = wandSpells[currentSpellIndex];
                float delay = Instance != null ? Instance.magicCircleDelay : 0.5f;
                int triggerSpellIndex = -1;

                if (spell is ClickTriggerAddSpell clickAddSpell)
                {
                    delay = clickAddSpell.magicCircleDelay;
                    int[] targetIndices = clickAddSpell.GetTargetIndices(wandSpells, currentSpellIndex);
                    if (targetIndices.Length >= 2)
                    {
                        triggerSpellIndex = targetIndices[1];
                    }
                }
                else if (spell is ExampleSpell)
                {
                    // ExampleSpell の場合は、右隣の呪文をターゲットにする
                    triggerSpellIndex = currentSpellIndex + 1;
                }

                SpellBase triggerSpell = null;
                if (triggerSpellIndex >= 0 && triggerSpellIndex < wandSpells.Count)
                    triggerSpell = wandSpells[triggerSpellIndex];
                var modifier = obj.AddComponent<ClickTriggerProjectileModifier>();
                modifier.Init(triggerSpell, wandSpells, triggerSpellIndex, new SpellContext(context.layer), delay);
            }
        };
    }

    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells,
        int currentSpellIndex)
    {
        int[] targetIndices = GetTargetIndices(wandSpells, currentSpellIndex);
        return targetIndices.Select(index => index - currentSpellIndex).ToArray();
    }
}
