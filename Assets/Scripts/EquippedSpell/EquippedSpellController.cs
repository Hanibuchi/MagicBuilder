using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 持ち込み呪文選択画面のコントローラー。
/// Modelからの変更通知をUIに伝え、UIからの操作をModel/Managerに反映させる橋渡しを行います。
/// </summary>
public class EquippedSpellController : MonoBehaviour, 
    IEquippedSpellModelObserver, 
    IEquippedSpellUIProvider
{
    // --- シングルトン実装 ---
    private static EquippedSpellController _instance;
    public static EquippedSpellController Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject singletonObject = new GameObject(nameof(EquippedSpellController));
                _instance = singletonObject.AddComponent<EquippedSpellController>();
                DontDestroyOnLoad(singletonObject);
            }
            return _instance;
        }
    }

    // --- 参照 ---
    [SerializeField] private EquippedSpellSelectionUI _selectionUI;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Modelのオブザーバーとして自身を登録
        EquippedSpellModel.Instance.SetObserver(this);

        // UIの初期化（Providerとして自身を渡す）
        if (_selectionUI != null)
        {
            _selectionUI.Init(this);
        }
    }

    /// <summary>
    /// UIへの参照をセットします（シーン遷移時などに利用）。
    /// </summary>
    public void SetUI(EquippedSpellSelectionUI ui)
    {
        _selectionUI = ui;
        if (_selectionUI != null)
        {
            _selectionUI.Init(this);
        }
    }

    // --- IEquippedSpellModelObserver の実装 (Model -> UI) ---

    public void OnAllSpellStatusesChanged(IReadOnlyList<SpellHoldStatus> allSpellStatuses)
    {
        if (_selectionUI != null)
        {
            _selectionUI.SetHoldSpells(allSpellStatuses);
        }
    }

    public void OnEquippedSpellsChanged(IReadOnlyList<SpellBase> equippedSpells)
    {
        if (_selectionUI != null)
        {
            _selectionUI.SetEquippedSpells(equippedSpells);
        }
    }

    public void OnMaxCapacityChanged(int newCapacity)
    {
        // 現状のUIに最大容量専用の更新メソッドがないため、
        // 必要に応じて UI 側の拡張（スロット枠の増減など）をここで呼び出します。
        Debug.Log($"[Controller] Max Capacity Changed to: {newCapacity}");
    }


    // --- IEquippedSpellUIProvider の実装 (UI -> Model/Manager) ---

    public IReadOnlyList<SpellHoldStatus> GetAllSpellStatuses()
    {
        return EquippedSpellModel.Instance.GetAllSpellStatuses();
    }

    public IReadOnlyList<SpellBase> GetCurrentEquippedSpells()
    {
        return EquippedSpellModel.Instance.GetEquippedSpells();
    }

    public void SetSpell(int index, SpellBase spell)
    {
        // Managerを介してデータを更新。
        // 更新されると EquippedSpellManager -> EquippedSpellModel -> Controller(Observer) 
        // の順で通知が戻り、最終的に UI が再構築されます。
        EquippedSpellManager.Instance.SetSpell(index, spell);
    }

    public void RemoveSpell(int index)
    {
        EquippedSpellManager.Instance.RemoveSpell(index);
    }
}