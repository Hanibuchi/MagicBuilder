using UnityEngine;
using System.Collections.Generic;

public class AttractionMover : MonoBehaviour, ISpellProjectileDestroyListener
{
    private float _totalRange;
    private float _totalForce;
    private LayerMask _targetLayer;
    private GameObject _attractionPrefab;
    private GameObject _repulsionPrefab;
    private AttractionEffect _effectInstance;
    private bool _hasFinished = false;

    public void AddAttractionData(float range, float force, GameObject attractionPrefab, GameObject repulsionPrefab, LayerMask targetLayer)
    {
        _totalRange += range;
        _totalForce += force;
        _targetLayer |= targetLayer; // レイヤーを追加（複数の呪文で異なるレイヤーを指定する場合を考慮）

        // プレハブ情報を更新（最新の呪文の設定を優先）
        if (attractionPrefab != null) _attractionPrefab = attractionPrefab;
        if (repulsionPrefab != null) _repulsionPrefab = repulsionPrefab;

        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (_hasFinished) return;

        // すでに演出がある場合は即座に破棄して新しく生成（設定更新のため）
        if (_effectInstance != null)
        {
            Destroy(_effectInstance.gameObject);
        }

        // 最終的な「範囲の値（_totalRange）」の正負で引力か斥力か（演出）を判定
        float absRange = Mathf.Abs(_totalRange);
        bool isAttraction = _totalRange >= 0;
        GameObject targetPrefab = isAttraction ? _attractionPrefab : _repulsionPrefab;

        if (targetPrefab != null)
        {
            GameObject go = Instantiate(targetPrefab, transform.position, Quaternion.identity);
            _effectInstance = go.GetComponent<AttractionEffect>();
            if (_effectInstance != null)
            {
                _effectInstance.Init(transform, absRange);
            }
        }
    }

    private void FixedUpdate()
    {
        if (_hasFinished) return;

        float absRange = Mathf.Abs(_totalRange);
        if (absRange <= 0.01f) return;

        // 範囲内の指定レイヤーのコライダーを取得
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, absRange, _targetLayer);

        foreach (var col in colliders)
        {
            // 自分自身は除外
            if (col.gameObject == gameObject) continue;

            // Rigidbody2D を持っているか確認
            Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // 中心に向かうベクトルを計算
                Vector2 direction = (Vector2)transform.position - rb.position;
                float distance = direction.magnitude;

                if (distance > 0.1f) // 中心に近づきすぎないように制限
                {
                    // _totalForceが正なら引力、負なら斥力として働く
                    Vector2 attractionForce = direction.normalized * _totalForce;
                    rb.AddForce(attractionForce);
                }
            }
        }
    }

    // ISpellProjectileDestroyListener の実装
    void ISpellProjectileDestroyListener.Destroy()
    {
        Finish();
    }

    private void OnDestroy()
    {
        Finish();
    }

    private void Finish()
    {
        if (_hasFinished) return;
        _hasFinished = true;

        if (_effectInstance != null)
        {
            _effectInstance.Finish();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _totalForce >= 0 ? Color.cyan : Color.red;
        Gizmos.DrawWireSphere(transform.position, Mathf.Abs(_totalRange));
    }
}
