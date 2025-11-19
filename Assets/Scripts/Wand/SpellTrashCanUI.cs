// ファイル名: SpellTrashCanUI.cs

using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// SpellUIをドロップすることで、その呪文を杖から削除（破棄）する機能を担当するUI。
/// </summary>
public class SpellTrashCanUI : MonoBehaviour, IDropHandler
{
    // --- ドロップ処理 ---

    public void OnDrop(PointerEventData eventData)
    {
        // 1. ドロップされたオブジェクトが SpellUI であるかを確認します。
        SpellUI droppedSpellUI = eventData.pointerDrag.GetComponent<SpellUI>();

        if (droppedSpellUI != null)
        {
            // 2. SpellUIが持つインターフェース ISpellContainer を通じて、
            //    元の杖UIに「削除が成功した」ことを通知します。
            //    これにより、
            //    a) SpellUI自身のクリーンアップ処理（OnEndDrag内）が実行され、
            //    b) ISpellContainer.NotifySpellRemoved(index) が呼ばれ、
            //    c) 最終的に元の杖の呪文リストから削除されます。
            droppedSpellUI.NotifyDropSuccess();

            // ※ ここで RebuildUI() を呼ぶ必要はありません。
            //    NotifyDropSuccess() の内部で、元の ISpellContainer (WandUIなど) が
            //    NotifySpellRemovedを呼び出し、その後に RebuildUI が適切に処理されます。
            
            // 例: ドロップ成功時の視覚的なフィードバック（任意）
            Debug.Log($"呪文 '{droppedSpellUI.GetSpellData().spellName}' がゴミ箱にドロップされ、杖から削除されました。");
        }
    }
}