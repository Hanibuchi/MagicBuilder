using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 主呪文の弾丸に付与され、周囲を回転する衛星呪文を管理するコンポーネント。
/// </summary>
public class OrbitCenter : MonoBehaviour
{
    private List<SpellBase> _satelliteSpells;
    private List<SpellBase> _wandSpells;
    private List<int> _satelliteIndices;
    private SpellContext _context;
    private float _magicCircleDelay;
    private float _radius;

    private Rigidbody2D _rb;

    public void Init(List<SpellBase> satelliteSpells, List<SpellBase> wandSpells, List<int> satelliteIndices, SpellContext context, float magicCircleDelay, float radius)
    {
        _satelliteSpells = satelliteSpells;
        _wandSpells = wandSpells;
        _satelliteIndices = satelliteIndices;
        _context = context;
        _magicCircleDelay = magicCircleDelay;
        _radius = radius;

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
            : (Vector2)(Quaternion.Euler(0, 0, centerRotationZ) * Vector2.right);

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
                // OrbitCenterの子オブジェクトとして生成し、相対座標を維持して追従させる
                GameObject mc = Instantiate(mcPrefab, spawnPos, Quaternion.Euler(0, 0, centerRotationZ), this.transform);
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
        foreach (var p in satelliteParams)
        {
            Vector2 currentOffset = p.offset;
            // 衛生用コンテキストの作成
            SpellContext satContext = _context.Clone();
            satContext.CasterPosition = transform.position + (Vector3)currentOffset;

            // 衛星にOrbitalSatelliteコンポーネントを付与するModifierを追加
            bool isUpper = p.index % 2 == 0;
            satContext.ProjectileModifier += satObj =>
            {
                if (satObj != null && this != null)
                {
                    var orbital = satObj.AddComponent<OrbitalSatellite>();
                    orbital.Init(transform, isUpper, _radius);
                }
            };

            // 発射
            float rotationZ = p.index % 2 == 0 ? centerRotationZ : centerRotationZ + 180f;
            p.spell.FireSpell(_wandSpells, _satelliteIndices[p.index], rotationZ, 1.0f, satContext);
        }
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
