using UnityEngine;
using TMPro; // TextMeshProを使用する場合は必須
using System.Collections.Generic;

/// <summary>
/// ダメージ表記UIの生成と管理を行うシングルトンクラス。
/// </summary>
public class DamageTextManager : MonoBehaviour
{
    // --- シングルトンインスタンス ---
    private static DamageTextManager instance;
    public static DamageTextManager Instance => instance;

    // --- 設定値 ---
    [Header("UI設定")]
    [Tooltip("ダメージ表記が親となるキャンバス/ルートTransform")]
    [SerializeField] private Transform canvasParent;
    [Tooltip("ダメージ表記の表示時間")]
    [SerializeField] private float displayDuration = 1.0f;
    [Tooltip("Y軸方向の初期ランダムオフセット範囲")]
    [SerializeField] private float initialRandomYOffset = 0.5f;

    // --- 内部状態 ---
    private Camera mainCamera;
    private readonly Dictionary<DamageType, Color> damageColors = new Dictionary<DamageType, Color>() { };

    // --- Unityライフサイクル ---

    [SerializeField] GameObject basePrefab;
    [SerializeField] GameObject firePrefab;
    [SerializeField] GameObject icePrefab;
    [SerializeField] GameObject woodPrefab;
    [SerializeField] GameObject waterPrefab;

    private void Awake()
    {
        // シングルトンインスタンスの設定
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        mainCamera = Camera.main;
    }

    // --- 公開メソッド ---

    /// <summary>
    /// 指定されたダメージの詳細に基づいて、ダメージ表記を生成して表示します。
    /// </summary>
    /// <param name="damageValue">表示するダメージ量。</param>
    /// <param name="damageType">ダメージの属性。</param>
    /// <param name="worldPosition">ダメージを受けたエンティティのワールド座標。</param>
    public void ShowDamageText(float damageValue, DamageType damageType, Vector3 worldPosition)
    {
        if (canvasParent == null || mainCamera == null) return;
        GameObject textPrefab;
        switch (damageType)
        {
            case DamageType.Base:
                textPrefab = basePrefab;
                break;
            case DamageType.Fire:
                textPrefab = firePrefab;
                break;
            case DamageType.Ice:
                textPrefab = icePrefab;
                break;
            case DamageType.Wood:
                textPrefab = woodPrefab;
                break;
            case DamageType.Water:
                textPrefab = waterPrefab;
                break;
            default:
                textPrefab = basePrefab;
                break;
        }

        // 1. ダメージテキストの生成
        GameObject textObj = Instantiate(textPrefab, canvasParent);

        // 3. UIの初期位置設定（ワールド座標からスクリーン座標へ変換）
        // ワールド座標をUIのCanvas座標系に変換
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();

        // 画面外の場合は表示しない
        if (screenPos.z < 0)
        {
            Destroy(textObj);
            return;
        }

        // オフセットを加えて初期位置を設定
        float randomOffsetY = Random.Range(-initialRandomYOffset, initialRandomYOffset);
        rectTransform.position = screenPos + Vector3.up * randomOffsetY * rectTransform.lossyScale.y; // スケールを考慮してオフセット適用

        var animator = textObj.GetComponent<DamageText>();
        animator.Initialize(damageValue, displayDuration);
    }
}