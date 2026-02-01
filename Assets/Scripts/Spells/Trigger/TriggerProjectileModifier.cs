using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 弾丸が何かにヒットした際（ダメージ計算時や破壊時）に次の呪文を誘発させるクラス。
/// </summary>
public class TriggerProjectileModifier : MonoBehaviour, ISpellProjectileTriggerListener, ISpellProjectileDestroyListener
{
    private SpellBase _nextSpell;
    private List<SpellBase> _wandSpells;
    private int _nextSpellIndex;
    private SpellContext _context;
    private float _magicCircleDelay;

    public void Init(SpellBase nextSpell, List<SpellBase> wandSpells, int nextSpellIndex, SpellContext context, float magicCircleDelay)
    {
        _nextSpell = nextSpell;
        _wandSpells = wandSpells;
        _nextSpellIndex = nextSpellIndex;
        _context = context;
        _magicCircleDelay = magicCircleDelay;
    }

    public void OnTrigger()
    {
        Fire();
    }

    public void Destroy()
    {
        Fire();
    }

    private void Fire()
    {
        if (_nextSpell == null) return;

        // SpellSchedulerを使用して、このオブジェクトが破棄されてもコルーチンが継続するようにする
        if (SpellScheduler.Instance != null)
        {
            SpellScheduler.Instance.StartSpellCoroutine(FireWithDelay());
        }
    }

    private IEnumerator FireWithDelay()
    {
        Vector3 spawnPosition = transform.position;
        float rotationZ = transform.rotation.eulerAngles.z;

        MagicCircle magicCircle = null;
        GameObject prefab = SpellCommonData.Instance?.magicCirclePrefab;

        _context.CasterPosition = spawnPosition;

        if (prefab != null)
        {
            GameObject circleGo = Instantiate(prefab, spawnPosition, Quaternion.Euler(0, 0, rotationZ));
            magicCircle = circleGo.GetComponent<MagicCircle>();

            if (magicCircle != null)
            {
                // トリガー用の色があれば使用する（SpellCommonDataにある場合）
                Color? circleColor = SpellCommonData.Instance?.otherColor;
                magicCircle.Show(_magicCircleDelay, color: circleColor);
                yield return new WaitForSeconds(_magicCircleDelay);
            }
        }

        _nextSpell.FireSpell(_wandSpells, _nextSpellIndex, rotationZ, 1, _context);

        if (magicCircle != null)
        {
            magicCircle.Hide(_magicCircleDelay);
        }
    }
}
