// ファイル名: SpellInventory.cs (更新版)

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

// ISpellContainer を実装
public class SpellInventory : MonoBehaviour, ISpellContainer
{
    public List<SpellBase> availableSpells = new List<SpellBase>();
    [SerializeField] private RectTransform inventoryFrame;

    private List<SpellUI> spellUIs = new List<SpellUI>();
    SpellUI draggingSpellUI;


    void Start()
    {
        availableSpells = SpellDatabase.Instance.allSpells.Select(e => e.spellAsset).ToList();

        // 初期位置を最小Y座標として保存
        inventoryUIPosY_Min = inventoryUI.anchoredPosition.y;
        targetPosY = inventoryUIPosY_Min;

        // ボタンにリスナーを設定
        moveUpButton.onClick.AddListener(MoveInventoryUp);
        moveDownButton.onClick.AddListener(MoveInventoryDown);

        // 初期状態では両方のボタンを非アクティブに設定
        moveUpButton.gameObject.SetActive(false);
        moveDownButton.gameObject.SetActive(false);


        // ゲーム開始時にインベントリUIを構築する
        RebuildUI();
    }

    /// <summary>
    /// インベントリにある呪文UIをすべてクリアし、現在のリストに基づいて再生成する。
    /// </summary>
    public void RebuildUI()
    {
        RebuildUIWithoutDragging();
        if (draggingSpellUI != null)
        {
            Destroy(draggingSpellUI.gameObject);
            draggingSpellUI = null;
        }

        UpdateScroll();
    }

    /// <summary>
    /// インベントリの呪文UIをドラッグ中のUIを除いて再構築します。
    /// </summary>
    void RebuildUIWithoutDragging()
    {
        // 既存のUI要素を全て破棄
        foreach (var spellUI in spellUIs)
        {
            if (spellUI != null)
            {
                Destroy(spellUI.gameObject);
            }
        }
        spellUIs.Clear();

        // 呪文リストに基づいて新しいUI要素を生成
        for (int i = 0; i < availableSpells.Count; i++)
        {
            CreateSpellUI(i, availableSpells[i]);
        }
    }

    /// <summary>
    /// SpellBaseのCreateUI()メソッドを呼び出して呪文UIを生成し、リストに追加する。
    /// </summary>
    private void CreateSpellUI(int index, SpellBase spell)
    {
        if (spell == null || spell.uiPrefab == null)
        {
            Debug.LogError($"Index {index} の呪文 '{spell.spellName}' に uiPrefab が設定されていません。");
            return;
        }

        // SpellBase.CreateUI() を利用してUIを生成 (内部で Instantiate(uiPrefab) が行われる)
        SpellUI spellUI = spell.CreateUI();

        if (spellUI != null)
        {
            spellUI.transform.SetParent(inventoryFrame, false); // 親を設定
            spellUI.SetIndex(index);
            // SpellInventory自身をコンテナとして渡す
            spellUI.Initialize(this);
            spellUIs.Add(spellUI);

            spellUI.gameObject.name = $"Inv_SpellUI_{index}_{spell.spellName}";
        }
    }


    // --- ISpellContainer の実装 ---
    public void NotifyDragBegin(int index)
    {
        draggingSpellUI = spellUIs[index];
        spellUIs[index] = null;
        RebuildUIWithoutDragging();
    }

    /// <summary>
    /// SpellUIがドラッグ＆ドロップによってコンテナから削除されたことを通知。
    /// インベントリの場合、呪文はデータリストからは削除せず、UIのみを再構築する。
    /// </summary>
    public void NotifySpellRemoved(int removedIndex)
    {
        RebuildUI();
        Debug.Log($"インベントリの呪文 ({availableSpells[removedIndex].spellName}) の削除通知を受け取りましたが、データは削除しません。");
    }


    // --- 追加機能: 杖への追加/移動のためのヘルパー ---

    /// <summary>
    /// 指定したインデックスの呪文データ（SpellBase）を取得する。
    /// これは、InventoryUIを継承したSpellUIが自身が持つデータを参照するために使用される。
    /// </summary>
    public SpellBase GetSpell(int index)
    {
        if (index >= 0 && index < availableSpells.Count)
        {
            return availableSpells[index];
        }
        return null;
    }


    // --- 追加フィールド ---
    [Header("UI Scroll Settings")]
    [SerializeField] RectTransform inventoryUI;
    [SerializeField] private GridLayoutGroup gridLayout; // 呪文UIが配置されるGridLayoutGroup
    [SerializeField] private Button moveUpButton;       // インベントリを上に上げるボタン
    [SerializeField] private Button moveDownButton;     // インベントリを下に下げるボタン
    [SerializeField] private float scrollSpeed = 10f;    // スクロール速度

    [SerializeField] float inventoryUIPosY_Min = -50f; // インベントリの下限Y座標 (初期位置)
    private float inventoryUIPosY_Max; // インベントリの上限Y座標 (全表示位置)
    private bool isInventoryUp = false;  // 現在インベントリが上がっているかどうかのフラグ
    private float targetPosY;            // 現在のターゲットY座標
    // ----------------------


    void UpdateScroll()
    {
        // --- スクロール機能の制御処理 ---
        if (gridLayout != null)
        {
            // 呪文の総数と1行あたりの列数から行数を計算
            int cellCount = availableSpells.Count;
            int constraintCount = gridLayout.constraintCount; // 1行あたりの列数

            // 行数を計算 (小数点以下を切り上げ)
            int rowCount = Mathf.CeilToInt((float)cellCount / constraintCount);

            // 2行以上であれば、上に上げるボタンを有効化
            if (rowCount >= 2)
            {
                // 初期状態は下にいるので、上に上げるボタンのみ有効
                moveUpButton.gameObject.SetActive(!isInventoryUp);
                moveDownButton.gameObject.SetActive(isInventoryUp);

                // 上限Y座標 (全要素が見える位置) を計算
                // (行数 - 1) * セルの高さ (cellSize.y) + 行間 (spacing.y) の合計分だけ上に移動させる
                float heightToMove = (rowCount - 1) * (gridLayout.cellSize.y + gridLayout.spacing.y);
                inventoryUIPosY_Max = inventoryUIPosY_Min + heightToMove;
            }
            else
            {
                // 1行以下ならスクロール不要なので、ボタンを非アクティブにし、インベントリを最小位置に戻す
                moveUpButton.gameObject.SetActive(false);
                moveDownButton.gameObject.SetActive(false);
                isInventoryUp = false;
                targetPosY = inventoryUIPosY_Min;
            }
        }
    }

    /// <summary>
    /// インベントリを上に移動させる。
    /// </summary>
    public void MoveInventoryUp()
    {
        if (isInventoryUp) return; // すでに上がっていたら何もしない

        targetPosY = inventoryUIPosY_Max;
        isInventoryUp = true;
        moveUpButton.gameObject.SetActive(false);
        moveDownButton.gameObject.SetActive(true);
        Debug.Log("インベントリを上に移動します。");
    }

    /// <summary>
    /// インベントリを下に移動させる（初期位置に戻す）。
    /// </summary>
    public void MoveInventoryDown()
    {
        if (!isInventoryUp) return; // すでに下がっていたら何もしない

        targetPosY = inventoryUIPosY_Min;
        isInventoryUp = false;
        moveUpButton.gameObject.SetActive(true);
        moveDownButton.gameObject.SetActive(false);
        Debug.Log("インベントリを下に移動します。");
    }

    void Update()
    {
        // ターゲット位置へ徐々に移動させる処理
        LerpInventory();
    }

    /// <summary>
    /// RectTransformをターゲット位置に徐々に移動させる。
    /// </summary>
    private void LerpInventory()
    {
        Vector2 currentPos = inventoryUI.anchoredPosition;

        // ターゲットY座標に向けて滑らかに移動
        float newPosY = Mathf.Lerp(currentPos.y, targetPosY, Time.deltaTime * scrollSpeed);

        // ターゲットに十分に近づいたら位置を確定し、移動を終了
        if (Mathf.Abs(newPosY - targetPosY) < 0.01f)
        {
            newPosY = targetPosY;
        }

        inventoryUI.anchoredPosition = new Vector2(currentPos.x, newPosY);
    }
}