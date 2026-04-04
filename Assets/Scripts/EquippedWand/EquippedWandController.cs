using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 持ち込み杖選択UIの橋渡しを行うコントローラー。
/// </summary>
public class EquippedWandController : MonoBehaviour,
    IEquippedWandsObserver,
    IEquippedWandUIProvider
{
    private static EquippedWandController _instance;
    public static EquippedWandController Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject singletonObject = new GameObject(nameof(EquippedWandController));
                _instance = singletonObject.AddComponent<EquippedWandController>();
                DontDestroyOnLoad(singletonObject);
            }
            return _instance;
        }
    }

    private EquippedWandUI _wandUI;

    [Header("UI 生成設定")]
    [SerializeField] private EquippedWandUI equippedWandUIPrefab;

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
        EquippedWandManager.Instance.RegisterObserver(this);

        if (_wandUI != null)
        {
            _wandUI.Init(this);
        }
    }

    public void SetUI(EquippedWandUI ui)
    {
        _wandUI = ui;
        if (_wandUI != null)
        {
            _wandUI.Init(this);
        }
    }

    public void OpenWandSelectionUI(Action onCloseCallback = null)
    {
        if (_wandUI == null)
        {
            if (equippedWandUIPrefab == null)
            {
                Debug.LogError("[EquippedWandController] equippedWandUIPrefab が未設定です。");
                return;
            }

            _wandUI = Instantiate(equippedWandUIPrefab);
            _wandUI.Init(this);
        }

        _wandUI.Open(onCloseCallback);
    }

    public void CloseWandSelectionUI()
    {
        _wandUI?.Close();
    }

    public void OnEquippedWandsChanged(IReadOnlyList<Wand> equippedWands)
    {
        _wandUI?.SetEquippedWands(equippedWands);
    }

    public IReadOnlyList<Wand> GetAllWands()
    {
        return EquippedWandManager.Instance.GetAllWands();
    }

    public IReadOnlyList<Wand> GetEquippedWands()
    {
        return EquippedWandManager.Instance.GetEquippedWands();
    }

    public bool IsWandUnlocked(Wand wand)
    {
        return EquippedWandManager.Instance.IsWandUnlocked(wand);
    }

    public bool IsWandEquipped(Wand wand)
    {
        return EquippedWandManager.Instance.IsWandEquipped(wand);
    }

    public void SetWand(int slotIndex, Wand wand)
    {
        EquippedWandManager.Instance.SetWand(slotIndex, wand);
    }

    public void RemoveWand(int slotIndex)
    {
        EquippedWandManager.Instance.RemoveWand(slotIndex);
    }
}

public interface IEquippedWandUIProvider
{
    IReadOnlyList<Wand> GetAllWands();
    IReadOnlyList<Wand> GetEquippedWands();
    bool IsWandUnlocked(Wand wand);
    bool IsWandEquipped(Wand wand);
    void SetWand(int slotIndex, Wand wand);
    void RemoveWand(int slotIndex);
}
