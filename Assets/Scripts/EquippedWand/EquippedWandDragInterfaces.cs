/// <summary>
/// 杖ドラッグUIから取得する共通インターフェース。
/// </summary>
public interface IEquippedWandDraggable
{
    Wand GetWandData();
    int GetSlotIndex();
    bool IsFromEquippedSlot();
    void NotifyDropSucceeded();
}

/// <summary>
/// 杖ドラッグUIから通知を受けるオブザーバー。
/// </summary>
public interface IEquippedWandDragObserver
{
    void NotifyDragStarted(Wand draggedWand, int fromSlotIndex, bool fromEquippedSlot, int originalSiblingIndex);
    void NotifyDroppedOnEquippedSlot(Wand droppedWand, int targetSlotIndex);
    void NotifyDroppedOutside(Wand draggedWand, int fromSlotIndex, bool fromEquippedSlot);
    void NotifyDroppedOnTrash(Wand draggedWand, int fromSlotIndex, bool fromEquippedSlot);
    void NotifyDropCompleted(Wand draggedWand, int fromSlotIndex, bool fromEquippedSlot);
}
