using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

/// <summary>
/// タイルマップの操作（アニメーションを伴う消去と再出現）を管理するシングルトンクラス
/// </summary>
public class TilemapManager : MonoBehaviour
{
    public static TilemapManager Instance { get; private set; }

    [Header("設定")]
    [SerializeField] private Tilemap targetTilemap;
    [SerializeField] private GameObject animationPrefab;
    [SerializeField] private float animationWaitTime = 0.5f; // アニメーションが完了するまでの待機時間

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 指定された座標のタイルをアニメーション後に消去し、一定時間後にアニメーションを伴って再出現させる
    /// </summary>
    /// <param name="worldPosition">ワールド座標</param>
    /// <param name="restoreDelay">再出現までの待ち時間</param>
    public void HandleTileEraseAndRestore(Vector3 worldPosition, float restoreDelay)
    {
        Vector3Int cellPosition = targetTilemap.WorldToCell(worldPosition);
        if (targetTilemap.HasTile(cellPosition))
        {
            StartCoroutine(EraseAndRestoreRoutine(cellPosition, restoreDelay));
        }
    }

    /// <summary>
    /// 指定された境界（Bounds）内に含まれる全てのタイルをアニメーション後に消去し、復元する
    /// </summary>
    public void HandleTilesInBounds(Bounds bounds, float restoreDelay)
    {
        Vector3Int min = targetTilemap.WorldToCell(bounds.min);
        Vector3Int max = targetTilemap.WorldToCell(bounds.max);

        // Z軸は無視して2D範囲をループ
        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                if (targetTilemap.HasTile(cellPos))
                {
                    // 重複してコルーチンが走らないよう、タイルが存在する場合のみ開始
                    StartCoroutine(EraseAndRestoreRoutine(cellPos, restoreDelay));
                }
            }
        }
    }

    /// <summary>
    /// 指定された円形範囲内（中心と半径）に含まれる全てのタイルをアニメーション後に消去し、復元する
    /// </summary>
    public void HandleTilesInCircle(Vector3 center, float radius, float restoreDelay)
    {
        // 半径を元に境界を計算
        Vector3 minPos = center - new Vector3(radius, radius, 0);
        Vector3 maxPos = center + new Vector3(radius, radius, 0);
        
        Vector3Int min = targetTilemap.WorldToCell(minPos);
        Vector3Int max = targetTilemap.WorldToCell(maxPos);

        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                if (targetTilemap.HasTile(cellPos))
                {
                    Vector3 tileWorldPos = targetTilemap.GetCellCenterWorld(cellPos);
                    // 中心からの距離が半径以内かチェック（2D平面）
                    if (Vector2.Distance((Vector2)center, (Vector2)tileWorldPos) <= radius)
                    {
                        StartCoroutine(EraseAndRestoreRoutine(cellPos, restoreDelay));
                    }
                }
            }
        }
    }

    private IEnumerator EraseAndRestoreRoutine(Vector3Int cellPosition, float delay)
    {
        // 処理開始時にタイルが既になければ（他のコルーチンで消去中など）中断
        if (!targetTilemap.HasTile(cellPosition)) yield break;

        // タイル情報の取得
        TileBase tileBase = targetTilemap.GetTile(cellPosition);
        Sprite tileSprite = targetTilemap.GetSprite(cellPosition);
        Vector3 worldCenterPos = targetTilemap.GetCellCenterWorld(cellPosition);

        // 1. 消去アニメーション
        GameObject eraseFX = Instantiate(animationPrefab, worldCenterPos, Quaternion.identity);
        TileAnimationEffect eraseEffect = eraseFX.GetComponent<TileAnimationEffect>();
        if (eraseEffect != null)
        {
            eraseEffect.PlayEraseAnimation(tileSprite, worldCenterPos);
        }

        // タイルを消去
        targetTilemap.SetTile(cellPosition, null);

        // 2. 待機
        yield return new WaitForSeconds(delay);

        // 3. 再出現アニメーション
        GameObject restoreFX = Instantiate(animationPrefab, worldCenterPos, Quaternion.identity);
        TileAnimationEffect restoreEffect = restoreFX.GetComponent<TileAnimationEffect>();
        if (restoreEffect != null)
        {
            restoreEffect.PlayRestoreAnimation(tileSprite, worldCenterPos);
        }

        // アニメーション完了まで待機
        yield return new WaitForSeconds(animationWaitTime);

        // タイルを元に戻す
        targetTilemap.SetTile(cellPosition, tileBase);
    }
}
