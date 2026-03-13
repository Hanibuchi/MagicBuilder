using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 主弾丸に付与され、バネ運動しながら追従する弾丸を管理するコンポーネント。
/// </summary>
public class FollowingCenter : MonoBehaviour, ISpellProjectileDestroyListener
{
    private List<SpellBase> _followerSpells;
    private List<SpellBase> _wandSpells;
    List<ISpellCastListener> listeners;
    private List<int> _followerIndices;
    private SpellContext _context;
    private float _magicCircleDelay;
    private float _amplitude;
    private float _frequency;

    private Rigidbody2D _rb;
    private bool _hasFired = false;

    public void Init(List<SpellBase> followerSpells, List<SpellBase> wandSpells, List<ISpellCastListener> listeners, List<int> followerIndices, SpellContext context, float magicCircleDelay, float amplitude, float frequency)
    {
        _followerSpells = followerSpells;
        _wandSpells = wandSpells;
        this.listeners = listeners;
        _followerIndices = followerIndices;
        _context = context;
        _magicCircleDelay = magicCircleDelay;
        _amplitude = amplitude;
        _frequency = frequency;

        _rb = GetComponent<Rigidbody2D>();

        StartCoroutine(FireFollowersWithDelay());
    }

    private IEnumerator FireFollowersWithDelay()
    {
        yield return null;

        if (_followerSpells == null || _followerSpells.Count == 0) yield break;

        float centerRotationZ = transform.rotation.eulerAngles.z;
        Vector2 forward = _rb != null && _rb.linearVelocity.sqrMagnitude > 0.01f
            ? _rb.linearVelocity.normalized
            : (Vector2)transform.right;
        Vector2 up = new Vector2(-forward.y, forward.x);

        // 魔法陣の表示
        for (int i = 0; i < _followerSpells.Count; i++)
        {
            float side = (i % 2 == 0) ? 1f : -1f;
            Vector2 spawnPos = (Vector2)transform.position + _amplitude * side * up;
            GameObject mcPrefab = SpellCommonData.Instance?.magicCirclePrefab;
            if (mcPrefab != null)
            {
                GameObject mc = Instantiate(mcPrefab, spawnPos, Quaternion.Euler(0, 0, centerRotationZ), this.transform);
                if (mc.TryGetComponent<MagicCircle>(out var magicCircle))
                {
                    magicCircle.Show(_magicCircleDelay, color: SpellCommonData.Instance.otherColor);
                }
                StartCoroutine(HideMagicCircle(mc, _magicCircleDelay));
            }
        }

        yield return new WaitForSeconds(_magicCircleDelay);

        FireFollowers();
    }

    public void FireFollowers()
    {
        if (_hasFired) return;
        _hasFired = true;

        if (_followerSpells == null || _followerSpells.Count == 0 || _context == null) return;

        float centerRotationZ = transform.rotation.eulerAngles.z;
        Vector2 forward = _rb != null && _rb.linearVelocity.sqrMagnitude > 0.01f
            ? _rb.linearVelocity.normalized
            : (Vector2)transform.right;
        Vector2 up = new Vector2(-forward.y, forward.x);

        for (int i = 0; i < _followerSpells.Count; i++)
        {
            SpellBase spell = _followerSpells[i];
            if (spell == null) continue;

            float side = (i % 2 == 0) ? 1f : -1f;
            Vector2 offset = _amplitude * side * up;

            SpellContext folContext = _context.Clone();
            // 魔法陣と同じ正しい位置から生成されるように設定
            folContext.CasterPosition = (Vector2)transform.position + offset; 

            folContext.ProjectileModifier += folObj =>
            {
                if (folObj != null && this != null)
                {
                    var following = folObj.AddComponent<FollowingSatellite>();
                    following.Init(transform, side, _amplitude, _frequency);
                }
            };

            // 進行方向と同じ向きで発射
            spell.FireSpell(_wandSpells, listeners, _followerIndices[i], centerRotationZ, 1.0f, folContext);
        }
    }

    void ISpellProjectileDestroyListener.Destroy() => FireFollowers();
    private void OnDestroy() { if (Application.isPlaying) FireFollowers(); }

    private IEnumerator HideMagicCircle(GameObject mc, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (mc != null && mc.TryGetComponent<MagicCircle>(out var magicCircle))
        {
            magicCircle.Hide(delay);
        }
    }
}
