using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 装備解除（ゴミ箱）エリア。
/// ここにドロップされたら、アイコン側に「成功」を伝えるだけで、
/// 実際のデータ削除はアイコン側の既存ロジックに任せる。
/// </summary>
public class EquippedTrashAreaUI : MonoBehaviour, IDropHandler
{
    [SerializeField] private AudioClip dropSound;
    public void OnDrop(PointerEventData eventData)
    {
        // ドラッグされているアイコンを取得
        EquippedSpellIconUI draggedIcon = eventData.pointerDrag?.GetComponent<EquippedSpellIconUI>();

        // 持ち込みスロットからのドラッグである場合のみ受理
        if (draggedIcon != null && draggedIcon.IsEquippedSlot)
        {
            Debug.Log("[Trash] 装備解除を受理しました。");
            SoundManager.Instance?.PlaySE(dropSound);
            // アイコン側の「ドロップ成功時」の処理をキックする。
            // これにより、アイコン内部で NotifyEquippedSpellRemoved(index) が呼ばれ、
            // Controller 経由で Model/Manager のデータが削除される。
            draggedIcon.NotifyDropSuccess();
        }
    }
}