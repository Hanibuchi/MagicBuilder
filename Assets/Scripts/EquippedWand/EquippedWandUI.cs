using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

/// <summary>
/// ステージ持ち込み杖選択UI全体を管理するクラス。
/// </summary>
public class EquippedWandUI : MonoBehaviour,
    IEmptyWandUIObserver,
    IEquippedWandDragObserver
{
    private const int EquippedSlotCount = 3;

    [Header("UI コンポーネント")]
    [SerializeField] private Transform equippedSlotsParent;
    [SerializeField] private Transform holdWandsParent;
    [SerializeField] private Button closeButton;

    [Header("プレハブ")]
    [SerializeField] private EquippedWandDescriptionUI equippedWandDescriptionPrefab;
    [SerializeField] private EmptyWandUI emptyWandPrefab;
    [SerializeField] private EquippedWandIconUI equippedWandIconPrefab;
    [SerializeField] private LockedWandIconUI lockedWandIconPrefab;

    [Header("ゴミ箱")]
    [SerializeField] private EquippedWandTrashUI trashArea;

    [Header("表示制御")]
    [SerializeField] private Animator animator;
    [SerializeField] private string openTrigger = "Open";
    [SerializeField] private string closeTrigger = "Close";

    private IEquippedWandUIProvider _provider;
    private IReadOnlyList<Wand> _allWands = new List<Wand>();
    private IReadOnlyList<Wand> _equippedWands = new List<Wand>();

    private readonly List<Component> _equippedSlotUIs = new List<Component>();
    private readonly List<Component> _holdWandUIs = new List<Component>();

    public bool IsVisible { get; private set; }

    private Action _closeCallback;

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }

        if (trashArea != null)
        {
            trashArea.SetObserver(this);
            trashArea.gameObject.SetActive(false);
        }
    }

    public void Init(IEquippedWandUIProvider provider)
    {
        _provider = provider;
        _allWands = _provider.GetAllWands();
        _equippedWands = _provider.GetEquippedWands();
        DontDestroyOnLoad(gameObject);

        RebuildEquippedSlots();
        RebuildHoldWands();
    }

    public void Open(Action onCloseCallback = null)
    {
        if (IsVisible)
        {
            return;
        }

        _closeCallback = onCloseCallback;
        IsVisible = true;

        gameObject.SetActive(true);

        if (_provider != null)
        {
            _allWands = _provider.GetAllWands();
            _equippedWands = _provider.GetEquippedWands();
        }

        RebuildEquippedSlots();
        RebuildHoldWands();

        if (animator != null)
        {
            animator.SetTrigger(openTrigger);
            animator.ResetTrigger(closeTrigger);
        }
    }

    public void Close()
    {
        if (!IsVisible)
        {
            return;
        }

        IsVisible = false;
        SetTrashArea(false);

        if (animator != null)
        {
            animator.SetTrigger(closeTrigger);
            animator.ResetTrigger(openTrigger);
        }
        else
        {
            OnCloseAnimationComplete();
        }
    }

    public void OnCloseAnimationComplete()
    {
        gameObject.SetActive(false);
        _closeCallback?.Invoke();
    }

    public void SetEquippedWands(IReadOnlyList<Wand> equippedWands)
    {
        _equippedWands = equippedWands ?? new List<Wand>();
        RebuildEquippedSlots();
        RebuildHoldWands();
    }

    private void RebuildEquippedSlots()
    {
        DestroyAndClear(_equippedSlotUIs);

        if (equippedSlotsParent == null)
        {
            return;
        }

        for (int i = 0; i < EquippedSlotCount; i++)
        {
            Wand wand = i < _equippedWands.Count ? _equippedWands[i] : null;

            Component created;
            if (wand != null)
            {
                if (equippedWandDescriptionPrefab == null)
                {
                    continue;
                }

                EquippedWandDescriptionUI ui = Instantiate(equippedWandDescriptionPrefab, equippedSlotsParent);
                ui.SetSlotIndex(i);
                ui.SetObserver(this);
                ui.SetData(wand);
                created = ui;
            }
            else
            {
                if (emptyWandPrefab == null)
                {
                    continue;
                }

                EmptyWandUI ui = Instantiate(emptyWandPrefab, equippedSlotsParent);
                ui.Initialize(i);
                ui.SetObserver(this);
                created = ui;
            }

            created.transform.SetSiblingIndex(i);
            _equippedSlotUIs.Add(created);
        }
    }

    private void RebuildHoldWands()
    {
        DestroyAndClear(_holdWandUIs);

        if (holdWandsParent == null)
        {
            return;
        }

        foreach (var wand in _allWands)
        {
            if (wand == null)
            {
                continue;
            }

            bool unlocked = _provider != null && _provider.IsWandUnlocked(wand);
            bool equipped = _provider != null && _provider.IsWandEquipped(wand);

            if (!unlocked)
            {
                if (lockedWandIconPrefab == null)
                {
                    continue;
                }

                LockedWandIconUI lockedUi = Instantiate(lockedWandIconPrefab, holdWandsParent);
                lockedUi.SetData(wand);
                _holdWandUIs.Add(lockedUi);
                continue;
            }

            if (equippedWandIconPrefab == null)
            {
                continue;
            }

            EquippedWandIconUI wandIcon = Instantiate(equippedWandIconPrefab, holdWandsParent);
            wandIcon.SetData(wand);
            wandIcon.SetObserver(this);
            wandIcon.SetSlotIndex(-1);
            wandIcon.SetDragAndVisualState(!equipped, equipped);
            _holdWandUIs.Add(wandIcon);
        }
    }

    private static void DestroyAndClear(List<Component> targets)
    {
        foreach (var target in targets)
        {
            if (target != null)
            {
                Destroy(target.gameObject);
            }
        }

        targets.Clear();
    }

    private void SetTrashArea(bool active)
    {
        if (trashArea != null)
        {
            trashArea.gameObject.SetActive(active);
        }
    }

    public void NotifyWandDroppedOnEmptySlot(Wand droppedWand, int targetSlotIndex)
    {
        if (droppedWand == null)
        {
            return;
        }

        _provider?.SetWand(targetSlotIndex, droppedWand);
        SetTrashArea(false);
    }

    public void NotifyDragStarted(Wand draggedWand, int fromSlotIndex, bool fromEquippedSlot)
    {
        if (fromEquippedSlot)
        {
            SetTrashArea(true);
        }
    }

    public void NotifyDroppedOnEquippedSlot(Wand droppedWand, int targetSlotIndex)
    {
        if (droppedWand == null)
        {
            return;
        }

        _provider?.SetWand(targetSlotIndex, droppedWand);
        SetTrashArea(false);
    }

    public void NotifyDroppedOutside(Wand draggedWand, int fromSlotIndex, bool fromEquippedSlot)
    {
        if (!fromEquippedSlot)
        {
            return;
        }

        _provider?.RemoveWand(fromSlotIndex);
        SetTrashArea(false);
    }

    public void NotifyDroppedOnTrash(Wand draggedWand, int fromSlotIndex, bool fromEquippedSlot)
    {
        if (!fromEquippedSlot)
        {
            return;
        }

        _provider?.RemoveWand(fromSlotIndex);
        SetTrashArea(false);
    }

    public void NotifyDropCompleted(Wand draggedWand, int fromSlotIndex, bool fromEquippedSlot)
    {
        if (fromEquippedSlot)
        {
            SetTrashArea(false);
        }
    }
}
