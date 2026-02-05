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

    private IEnumerator EraseAndRestoreRoutine(Vector3Int cellPosition, float delay)
    {
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
