// ファイル名: SpellInventory.cs (更新版)

using UnityEngine;
using System.Collections.Generic;

// ISpellContainer を実装
public class SpellInventory : MonoBehaviour, ISpellContainer
{
    public List<SpellBase> availableSpells = new List<SpellBase>();
    [SerializeField] private RectTransform inventoryFrame;

    private List<SpellUI> spellUIs = new List<SpellUI>();


    void Start()
    {
        // ゲーム開始時にインベントリUIを構築する
        RebuildUI();
    }

    /// <summary>
    /// インベントリにある呪文UIをすべてクリアし、現在のリストに基づいて再生成する。
    /// </summary>
    public void RebuildUI()
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
}