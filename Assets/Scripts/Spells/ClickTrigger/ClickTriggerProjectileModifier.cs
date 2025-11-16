using UnityEngine;
using System.Collections.Generic;

public class ClickTriggerProjectileModifier : MonoBehaviour
{
    public SpellBase nextSpell;
    public List<SpellBase> wandSpells;
    public int nextSpellIndex;
    public SpellContext context;

    public void Init(SpellBase nextSpell, List<SpellBase> wandSpells, int nextSpellIndex, SpellContext context)
    {
        this.nextSpell = nextSpell;
        this.wandSpells = wandSpells;
        this.nextSpellIndex = nextSpellIndex;
        this.context = context;
    }
    private void Start()
    {
        // クリックを感知するクラスへ登録
        ClickTriggerInputReader.Instance.RegisterCallback(Fire);
    }

    private void OnDestroy()
    {
        // クリックを感知するクラスの登録を解除
        ClickTriggerInputReader.Instance.UnregisterCallback(Fire);
    }

    public void Fire()
    {
        if (nextSpell != null)
        {
            float rotationZ = gameObject.transform.rotation.eulerAngles.z;
            context.CasterPosition = gameObject.transform.position;
            nextSpell.FireSpell(wandSpells, nextSpellIndex, rotationZ, 1, context);
        }
    }
}
