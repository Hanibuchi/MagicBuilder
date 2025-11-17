using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ClickTriggerProjectileModifier : MonoBehaviour
{
    public SpellBase nextSpell;
    public List<SpellBase> wandSpells;
    public int nextSpellIndex;
    public SpellContext context;
    public float delayTime;

    public void Init(SpellBase nextSpell, List<SpellBase> wandSpells, int nextSpellIndex, SpellContext context, float delayTime)
    {
        this.nextSpell = nextSpell;
        this.wandSpells = wandSpells;
        this.nextSpellIndex = nextSpellIndex;
        this.context = context;
        this.delayTime = delayTime;
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
            StartCoroutine(FireWithDelay());
        }
    }

    private IEnumerator FireWithDelay()
    {
        delayTime = Mathf.Abs(nextSpell.GetGaussianRandom(delayTime));
        Debug.Log("delaytime: " + delayTime);
        yield return new WaitForSeconds(delayTime);

        float rotationZ = gameObject.transform.rotation.eulerAngles.z;
        context.CasterPosition = gameObject.transform.position;
        nextSpell.FireSpell(wandSpells, nextSpellIndex, rotationZ, 1, context);
    }
}
