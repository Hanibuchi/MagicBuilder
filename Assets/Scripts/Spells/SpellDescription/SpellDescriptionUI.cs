using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

/// <summary>
/// 呪文の詳細説明パネルを管理するシングルトンクラス。
/// SpellBaseの情報を受け取り、UIに表示します。
/// </summary>
public class SpellDescriptionUI : MonoBehaviour, IPointerClickHandler
{
    // シングルトンインスタンス
    public static SpellDescriptionUI Instance { get; private set; }

    [Header("UI要素への参照")]
    [Tooltip("詳細説明パネル全体のルートGameObject")]
    [SerializeField] private GameObject detailPanelRoot;

    [Tooltip("呪文の名前を表示するTextMeshProUGUI")]
    [SerializeField] private TextMeshProUGUI spellNameText;

    [Tooltip("呪文のアイコンを生成して配置する親Transform")]
    [SerializeField] private Transform iconParent;

    [Tooltip("呪文の概要説明を表示するTextMeshProUGUI")]
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Tooltip("詳細項目のプレハブを生成する親Transform")]
    [SerializeField] private Transform detailItemsParent;

    [Tooltip("各詳細項目（クールタイムなど）を表示するためのプレハブ")]
    [SerializeField] private GameObject detailItemPrefab;


    private SpellBase currentlyDisplayedSpell = null;
    private List<GameObject> activeDetailItems = new List<GameObject>();
    // 現在表示されているドロップUIのGameObjectを保持するためのフィールドを追加
    private GameObject currentDropUI = null;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // 初期状態では非表示
        detailPanelRoot.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        HideDescription();
    }

    /// <summary>
    /// 指定されたSpellBaseの詳細説明パネルを表示します。
    /// </summary>
    /// <param name="spell">表示するSpellBaseインスタンス。</param>
    public void ShowDescription(SpellBase spell)
    {
        if (currentlyDisplayedSpell == spell) return;
        if (spell == null)
        {
            HideDescription();
            return;
        }

        // 以前の詳細項目をクリア
        ClearDetailItems();
        ClearDropUI(); // アイコンクリアを追加

        currentlyDisplayedSpell = spell;

        if (spellNameText != null)
        {
            spellNameText.text = spell.spellName;
        }

        if (iconParent != null)
        {
            // CreateDropUI() を使ってドロップUI（アイコン）を生成
            GameObject dropUI = spell.CreateDropUI();
            if (dropUI != null)
            {
                // 親子関係を設定
                dropUI.transform.SetParent(iconParent, false);
                currentDropUI = dropUI;
            }
        }

        // 1. 呪文の概要説明を設定
        descriptionText.text = spell.GetDescription();

        // 2. 詳細項目リストを取得し、UIを生成・設定
        List<SpellDescriptionItem> details = spell.GetDescriptionDetails();

        foreach (var item in details)
        {
            // プレハブからUIオブジェクトを生成
            GameObject detailItemGO = Instantiate(detailItemPrefab, detailItemsParent);
            activeDetailItems.Add(detailItemGO);

            // SpellDescriptionDetailUIコンポーネントを取得し、データを設定
            if (detailItemGO.TryGetComponent<SpellDescriptionDetailUI>(out var detailUI))
            {
                detailUI.SetData(item);
            }
            else
            {
                Debug.LogError("詳細項目プレハブに SpellDescriptionDetailUI が見つかりません。");
            }
        }

        // 3. パネルを表示
        detailPanelRoot.SetActive(true);
    }

    /// <summary>
    /// 詳細説明パネルを非表示にし、データをリセットします。
    /// </summary>
    private void HideDescription()
    {
        detailPanelRoot.SetActive(false);
        currentlyDisplayedSpell = null;
        ClearDetailItems();
        ClearDropUI(); // アイコンクリアを追加
    }

    /// <summary>
    /// 生成された詳細項目のUIオブジェクトを全て破棄します。
    /// </summary>
    private void ClearDetailItems()
    {
        foreach (var go in activeDetailItems)
        {
            Destroy(go);
        }
        activeDetailItems.Clear();
    }

    /// <summary>
    /// 生成されたドロップUI（アイコン）を破棄します。
    /// </summary>
    private void ClearDropUI()
    {
        if (currentDropUI != null)
        {
            Destroy(currentDropUI);
            currentDropUI = null;
        }
    }

    public SpellBase Test_spell;
    public void Test()
    {
        ShowDescription(Test_spell);
    }
}