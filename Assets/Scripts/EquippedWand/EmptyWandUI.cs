using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 装備スロットが空の時に表示されるUI。
/// </summary>
public class EmptyWandUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private UnityEngine.UI.Image frameImage;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material hoverMaterial;

    private int _slotIndex = -1;
    private IEmptyWandUIObserver _observer;

    public void Initialize(int slotIndex)
    {
        _slotIndex = slotIndex;
        gameObject.name = $"EmptyWandUI_{slotIndex}";
    }

    public void SetObserver(IEmptyWandUIObserver observer)
    {
        _observer = observer;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
        {
            return;
        }

        if (!eventData.pointerDrag.TryGetComponent(out IEquippedWandDraggable dragged))
        {
            return;
        }

        Wand wand = dragged.GetWandData();
        if (wand == null)
        {
            return;
        }

        ResetMaterial();

        dragged.NotifyDropSucceeded();
        _observer?.NotifyWandDroppedOnEmptySlot(wand, _slotIndex);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null && eventData.pointerDrag.TryGetComponent(out IEquippedWandDraggable _))
        {
            if (frameImage != null)
            {
                frameImage.material = hoverMaterial;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetMaterial();
    }

    private void ResetMaterial()
    {
        if (frameImage != null)
        {
            frameImage.material = defaultMaterial;
        }
    }
}

public interface IEmptyWandUIObserver
{
    void NotifyWandDroppedOnEmptySlot(Wand droppedWand, int targetSlotIndex);
}
