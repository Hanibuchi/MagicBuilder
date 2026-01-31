using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ClickTriggerProjectileModifier : MonoBehaviour, ISpellProjectileDestroyListener
{
    public SpellBase nextSpell;
    public List<SpellBase> wandSpells;
    public int nextSpellIndex;
    public SpellContext context;
    public float magicCircleDelay;

    private bool _hasFired = false;

    public void Init(SpellBase nextSpell, List<SpellBase> wandSpells, int nextSpellIndex, SpellContext context, float magicCircleDelay)
    {
        this.nextSpell = nextSpell;
        this.wandSpells = wandSpells;
        this.nextSpellIndex = nextSpellIndex;
        this.context = context;
        this.magicCircleDelay = magicCircleDelay;
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
        if (_hasFired) return;
        _hasFired = true;

        if (nextSpell != null)
        {
            // SpellSchedulerを使用して、このオブジェクトが破棄されてもコルーチンが継続するようにする
            SpellScheduler.Instance.StartSpellCoroutine(FireWithDelay());
        }

        // 発射時に通知を送る
        var listeners = GetComponents<IClickTriggerFireListener>();
        foreach (var listener in listeners)
        {
            listener.OnFire();
        }
    }

    void ISpellProjectileDestroyListener.Destroy()
    {
        Fire();
    }

    private IEnumerator FireWithDelay()
    {
        // yieldの前に必要な情報（トランスフォームなど）を取得しておく
        // yield以降に gameObject や transform にアクセスすると、インスタンス破棄時に「MissingReferenceException」が発生するため
        Vector3 spawnPosition = transform.position;
        float rotationZ = transform.rotation.eulerAngles.z;

        // 💡 魔法陣の表示演出を追加
        MagicCircle magicCircle = null;
        GameObject prefab = SpellCommonData.Instance?.magicCirclePrefab;
        context.CasterPosition = spawnPosition;

        if (prefab != null)
        {
            // Instantiateは UnityEngine.Object の静的メソッドなので、このオブジェクトが破棄されていても動作する
            GameObject circleGo = Instantiate(prefab, spawnPosition, Quaternion.Euler(0, 0, rotationZ));
            magicCircle = circleGo.GetComponent<MagicCircle>();

            if (magicCircle != null)
            {
                // 要件: 完全に表示されるまでの時間(magicCircleDelay), サイズ1, 指定した色
                Color? circleColor = SpellCommonData.Instance?.triggerMagicCircleColor;
                magicCircle.Show(magicCircleDelay, color: circleColor);
                // 魔法陣が出てから少し待ってから発射
                yield return new WaitForSeconds(magicCircleDelay);
            }
        }

        nextSpell.FireSpell(wandSpells, nextSpellIndex, rotationZ, 1, context);

        // 消滅演出の開始 (表示と同じ時間をかけて消える)
        if (magicCircle != null)
        {
            magicCircle.Hide(magicCircleDelay);
        }
    }
}

/// <summary>
/// ClickTriggerが発動（Fire）された際の通知を受け取るためのインターフェース。
/// </summary>
public interface IClickTriggerFireListener
{
    void OnFire();
}
