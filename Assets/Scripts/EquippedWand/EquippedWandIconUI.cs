using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// 装備中の杖の見た目のみを表示するUI。
/// </summary>
public class EquippedWandIconUI : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IEquippedWandDraggable
{
    [Header("UI References")]
    [SerializeField, Tooltip("杖の見た目を表示するImage")]
    private Image wandImage;
    [SerializeField] Image frameImage;
    [SerializeField] Material grayMaterial;

    [SerializeField] Image dragSourceImage;

    private Wand _wand;
    private bool _canDrag = true;
    private bool _dropSucceeded;
    private int _slotIndex = -1;
    private IEquippedWandDragObserver _observer;
    private Transform _originalParent;
    private int _originalSiblingIndex;

    /// <summary>
    /// 杖の表示内容を更新します。
    /// </summary>
    public void SetData(Wand wand)
    {
        _wand = wand;

        if (wandImage == null)
        {
            return;
        }

        if (wand == null)
        {
            wandImage.sprite = null;
            return;
        }

        wandImage.sprite = wand.wandSprite;
    }

    public void SetObserver(IEquippedWandDragObserver observer)
    {
        _observer = observer;
    }

    public void SetSlotIndex(int slotIndex)
    {
        _slotIndex = slotIndex;
    }

    public void SetDragAndVisualState(bool canDrag, bool grayOut)
    {
        _canDrag = canDrag;

        if (wandImage != null)
        {
            wandImage.material = grayOut ? grayMaterial : null;
        }

        if (frameImage != null)
        {
            frameImage.material = grayOut ? grayMaterial : null;
        }
    }

    public Wand GetWandData()
    {
        return _wand;
    }

    public int GetSlotIndex()
    {
        return _slotIndex;
    }

    public bool IsFromEquippedSlot()
    {
        return false;
    }

    public void NotifyDropSucceeded()
    {
        _dropSucceeded = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_wand == null)
        {
            return;
        }

        if (!_canDrag)
        {
            eventData.pointerDrag = null;
            return;
        }

        _dropSucceeded = false;

        _originalParent = transform.parent;
        _originalSiblingIndex = transform.GetSiblingIndex();

        if (dragSourceImage != null)
        {
            dragSourceImage.raycastTarget = false;
        }

        RectTransform root = DraggingSpellRootProvider.Instance != null
            ? DraggingSpellRootProvider.Instance.GetRootTransform()
            : null;
        if (root != null)
        {
            transform.SetParent(root, true);
        }

        _observer?.NotifyDragStarted(_wand, _slotIndex, IsFromEquippedSlot(), _originalSiblingIndex);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        StartCoroutine(HandleDropResult());
    }

    private IEnumerator HandleDropResult()
    {
        yield return null;

        if (dragSourceImage != null)
        {
            dragSourceImage.raycastTarget = true;
        }

        if (!_dropSucceeded)
        {
            _observer?.NotifyDroppedOutside(_wand, _slotIndex, IsFromEquippedSlot());

            if (_originalParent != null)
            {
                transform.SetParent(_originalParent, true);
                transform.SetSiblingIndex(_originalSiblingIndex);
            }
        }
        else
        {
            _observer?.NotifyDropCompleted(_wand, _slotIndex, IsFromEquippedSlot());
        }

        if (!_dropSucceeded && _originalParent != null)
        {
            transform.SetParent(_originalParent, true);
            transform.SetSiblingIndex(_originalSiblingIndex);
        }
        else if (_dropSucceeded)
        {
            Destroy(gameObject);
        }
    }

    [SerializeField]
    private Wand testWand;

    [ContextMenu("Test Set Data")]
    private void TestSetData()
    {
        SetData(testWand);
    }
}