using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ドラッグ中の装備杖を破棄（装備解除）するドロップエリア。
/// </summary>
public class EquippedWandTrashUI : MonoBehaviour, IDropHandler
{
    [SerializeField, Tooltip("ゴミ箱にドロップしたときの効果音")]
    private AudioClip dropSound;

    private IEquippedWandDragObserver _observer;

    public void SetObserver(IEquippedWandDragObserver observer)
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

        if (!dragged.IsFromEquippedSlot())
        {
            return;
        }

        if (SoundManager.Instance != null && dropSound != null)
        {
            SoundManager.Instance.PlaySE(dropSound);
        }

        dragged.NotifyDropSucceeded();
        _observer?.NotifyDroppedOnTrash(dragged.GetWandData(), dragged.GetSlotIndex(), true);
    }
}
