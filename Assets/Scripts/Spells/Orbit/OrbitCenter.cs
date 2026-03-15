using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 主呪文の弾丸に付与され、周囲を回転する衛星呪文を管理するコンポーネント。
/// </summary>
public class OrbitCenter : MonoBehaviour, ISpellProjectileDestroyListener
{
    private List<SpellBase> _satelliteSpells;
    private List<SpellBase> _wandSpells;
    List<ISpellCastListener> listeners;
    private List<int> _satelliteIndices;
    private SpellContext _context;
    private float _magicCircleDelay;
    private float _radius;
    private float _minInitSpeed;

    private Rigidbody2D _rb;
    private bool _hasFired = false;

    public void Init(List<SpellBase> satelliteSpells, List<ISpellCastListener> listeners, List<SpellBase> wandSpells, List<int> satelliteIndices, SpellContext context, float magicCircleDelay, float radius, float minInitSpeed)
    {
        _satelliteSpells = satelliteSpells;
        _wandSpells = wandSpells;
        _satelliteIndices = satelliteIndices;
        _context = context;
        _magicCircleDelay = magicCircleDelay;
        _radius = radius;
        _minInitSpeed = minInitSpeed;
        this.listeners = listeners;

        _rb = GetComponent<Rigidbody2D>();

        StartCoroutine(FireSatellitesWithDelay());
    }

    private IEnumerator FireSatellitesWithDelay()
    {
        // 少し待ってから発射（主弾丸が発射された直後に位置を確定させるため）
        yield return null;

        if (_satelliteSpells == null || _satelliteSpells.Count == 0) yield break;

        float centerRotationZ = transform.rotation.eulerAngles.z;

        // 速度が0の場合は前方を向いていると仮定
        Vector2 forward = _rb != null && _rb.linearVelocity.sqrMagnitude > 0.01f
            ? _rb.linearVelocity.normalized
            : (Vector2)transform.right;

        Vector2 up = new Vector2(-forward.y, forward.x); // 前方に垂直なベクトル

        // 各衛星の発射パラメータを計算
        var satelliteParams = new List<(SpellBase spell, Vector2 offset, float radius, float speed, int index)>(_satelliteSpells.Count);
        for (int i = 0; i < _satelliteSpells.Count; i++)
        {
            SpellBase spell = _satelliteSpells[i];
            if (spell == null) continue;

            float spellSpeed = 15f;
            if (spell is ExampleSpell example)
            {
                spellSpeed = example.StrengthMultiplier;
            }

            float side = (i % 2 == 0) ? 1f : -1f;
            Vector2 offset = _radius * side * up;

            satelliteParams.Add((spell, offset, _radius, spellSpeed, i));
        }

        // 1. 全ての魔法陣を同時に表示
        foreach (var p in satelliteParams)
        {
            Vector2 spawnPos = (Vector2)transform.position + p.offset;
            GameObject mcPrefab = SpellCommonData.Instance?.magicCirclePrefab;
            if (mcPrefab != null)
            {
                // 反時計回りに合わせて魔法陣の向きも調整（上側は後ろ、下側は前）
                float mcRotationZ = p.index % 2 == 0 ? centerRotationZ + 180f : centerRotationZ;
                // OrbitCenterの子オブジェクトとして生成し、相対座標を維持して追従させる
                GameObject mc = Instantiate(mcPrefab, spawnPos, Quaternion.Euler(0, 0, mcRotationZ), this.transform);
                if (mc.TryGetComponent<MagicCircle>(out var magicCircle))
                {
                    magicCircle.Show(_magicCircleDelay, color: SpellCommonData.Instance.otherColor);
                }

                if (SpellScheduler.Instance != null)
                    SpellScheduler.Instance.StartCoroutine(HideMagicCircle(mc, _magicCircleDelay));
                else
                    StartCoroutine(HideMagicCircle(mc, _magicCircleDelay));
            }
        }

        // 魔法陣の表示待機
        yield return new WaitForSeconds(_magicCircleDelay);

        // 2. 全ての衛星を同時に発射
        FireSatellites();
    }

    public void FireSatellites()
    {
        if (_hasFired) return;
        _hasFired = true;

        if (_satelliteSpells == null || _satelliteSpells.Count == 0 || _context == null) return;

        float centerRotationZ = transform.rotation.eulerAngles.z;

        // 速度が0の場合は前方を向いていると仮定
        Vector2 forward = _rb != null && _rb.linearVelocity.sqrMagnitude > 0.01f
            ? _rb.linearVelocity.normalized
            : (Vector2)transform.right;

        Vector2 up = new Vector2(-forward.y, forward.x); // 前方に垂直なベクトル

        for (int i = 0; i < _satelliteSpells.Count; i++)
        {
            SpellBase spell = _satelliteSpells[i];
            if (spell == null) continue;

            float side = (i % 2 == 0) ? 1f : -1f;
            Vector2 offset = _radius * side * up;

            // 衛生用コンテキストの作成
            SpellContext satContext = _context.Clone();
            satContext.CasterPosition = (Vector2)transform.position + offset;

            // 衛星にOrbitalSatelliteコンポーネントを付与するModifierを追加
            bool isUpper = i % 2 == 0;
            satContext.ProjectileModifier += satObj =>
            {
                if (satObj != null && this != null)
                {
                    var orbital = satObj.AddComponent<OrbitalSatellite>();
                    orbital.Init(transform, isUpper, _radius, _minInitSpeed);
                }
            };

            // 発射（反時計回りに合わせて：上側は後ろ向き、下側は前向きに発射）
            float rotationZ = i % 2 == 0 ? centerRotationZ + 180f : centerRotationZ;
            spell.FireSpell(_wandSpells, listeners, _satelliteIndices[i], rotationZ, 1.0f, satContext);
        }
    }

    void ISpellProjectileDestroyListener.Destroy()
    {
        FireSatellites();
    }

    private void OnDestroy()
    {
        if (!Application.isPlaying) return;
        FireSatellites();
    }

    private IEnumerator HideMagicCircle(GameObject mc, float delay)
    {
        yield return new WaitForSeconds(delay); // 表示後の持続時間（適宜調整）
        if (mc != null && mc.TryGetComponent<MagicCircle>(out var magicCircle))
        {
            magicCircle.Hide(delay);
        }
    }
}
