using UnityEngine;
using System.Collections;

/// <summary>
/// 呪文ドロップ時のアニメーションとインベントリへの追加を管理するシングルトンクラス。
/// </summary>
public class SpellDropManager : MonoBehaviour
{
    // --- シングルトンパターン ---
    public static SpellDropManager Instance { get; private set; }

    // --- インスペクタ設定 ---
    [Header("設定")]
    [Tooltip("ドロップUIを表示するCanvasのRectTransform")]
    [SerializeField] private RectTransform dropCanvas;

    [Header("アニメーション設定")]
    [Tooltip("放物線アニメーションの所要時間（秒）")]
    [SerializeField] private float animationDuration = 1.0f;
    [Tooltip("放物線の高さ（アニメーションの頂点の高さ）")]
    [SerializeField] private float arcHeight = 300f; // 画面を跨ぐための高さ

    // --- 初期化 ---
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // シーンをまたいで保持する場合は DontDestroyOnLoad(gameObject); を追加
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [SerializeField] AudioClip dropSound;
    [SerializeField] float dropSoundVolume = 1.0f;

    /// <summary>
    /// 呪文ドロップアニメーションを開始し、完了後にインベントリに追加します。
    /// </summary>
    /// <param name="worldPosition">ドロップ開始のワールド座標。</param>
    /// <param name="spellData">ドロップする呪文データ。</param>
    public void DropSpell(Vector3 worldPosition, SpellBase spellData)
    {
        if (dropCanvas == null || SpellInventory.Instance == null)
        {
            Debug.LogError("DropCanvas または SpellInventory が設定されていません。");
            return;
        }

        // 1. SpellBaseからドロップUIオブジェクトを生成
        GameObject dropUIObject = spellData.CreateDropUI();
        if (dropUIObject == null)
        {
            Debug.LogError("ドロップUIオブジェクトの生成に失敗しました。"); return;
        }

        // 2. Canvasの子オブジェクトに設定
        dropUIObject.transform.SetParent(dropCanvas, false);

        // 3. UIの初期位置をワールド座標からCanvasのローカル座標に変換
        Vector2 startPosition;
        var targetCanvas = dropCanvas.GetComponent<Canvas>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dropCanvas,
            Camera.main.WorldToScreenPoint(worldPosition),
            targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : targetCanvas.worldCamera,
            out startPosition
        );

        // 4. 終端位置をSpellInventoryから取得 (RectTransformの anchoredPosition)
        Vector2 endPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dropCanvas,
            SpellInventory.Instance.GetDropTargetPosition(),
            targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : targetCanvas.worldCamera,
            out endPosition
        );

        if (SoundManager.Instance != null && dropSound != null)
        {
            SoundManager.Instance.PlaySE(dropSound, dropSoundVolume);
        }

        // 5. アニメーションコルーチンを開始
        StartCoroutine(AnimateDrop(dropUIObject.GetComponent<RectTransform>(), startPosition, endPosition, spellData));
    }

    /// <summary>
    /// ドロップUIを放物線状にターゲット位置まで移動させるコルーチン。
    /// </summary>
    private IEnumerator AnimateDrop(RectTransform dropUI, Vector2 start, Vector2 end, SpellBase spellData)
    {
        float startTime = Time.time;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed = Time.time - startTime;
            float t = elapsed / animationDuration; // 0から1へ線形に変化

            // XZ平面での線形補間 (ここではXY平面)
            Vector2 newPos = Vector2.Lerp(start, end, t);

            // 放物線の計算 (Y軸方向に追加の高さ補間)
            // t(1-t)はt=0, t=1で0、t=0.5で最大値0.25を取る二次曲線
            float arcY = arcHeight * 4f * t * (1f - t);
            newPos.y += arcY;

            // UIの位置を更新
            dropUI.anchoredPosition = newPos;

            yield return null;
        }

        // アニメーション終了: ターゲット位置に位置を確定
        dropUI.anchoredPosition = end;

        // 画面外に出た（アニメーション終了）と見なし、UIを破棄
        Destroy(dropUI.gameObject);

        // SpellInventoryに呪文を追加
        SpellInventory.Instance.AddSpellToInventory(spellData);

        Debug.Log($"呪文 '{spellData.spellName}' のドロップアニメーションが完了し、インベントリに追加されました。");
    }

    [SerializeField] Transform test_startPosRect;
    [SerializeField] SpellBase test_spellData;
    public void Test()
    {
        DropSpell(test_startPosRect.position, test_spellData);
    }
}