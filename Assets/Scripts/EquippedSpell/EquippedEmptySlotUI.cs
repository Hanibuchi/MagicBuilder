// EquippedEmptySlotUI.cs

using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 持ち込み呪文スロットが空の場合に表示されるUIコンポーネント。
/// 呪文UIがドロップされたときに、装備リストへの登録処理を開始する役割を持つ。
/// </summary>
public class EquippedEmptySlotUI : MonoBehaviour, IDropHandler
{
    // --- 内部データ ---

    private int _slotIndex = -1; // この空スロットが表す持ち込みスロットの番号
    
    // 外部通知用（登録できるのは1つのみ）
    private IEquippedEmptySlotObserver _observer;


    // --- 初期化 ---

    /// <summary>
    /// 初期化処理として、このスロットのインデックスを設定します。
    /// </summary>
    /// <param name="index">持ち込みスロットの番号</param>
    public void Initialize(int index)
    {
        _slotIndex = index;
        this.gameObject.name = $"EquippedEmptySlotUI_{_slotIndex}";
        
        // 通常、空スロットはアイコンを表示しないため、ここではスロット番号のみを保持
    }

    /// <summary>
    /// 変更通知を受け取るオブザーバーをセットします。（1つのみ登録可能）
    /// </summary>
    public void SetObserver(IEquippedEmptySlotObserver observer)
    {
        _observer = observer;
    }


    // --- ドロップ処理 (IDropHandler) ---

    public void OnDrop(PointerEventData eventData)
    {
        // 1. ドラッグされているオブジェクトから EquippedSpellIconUI を取得する
        if (eventData.pointerDrag == null)
        {
            Debug.LogWarning("Dropped object has no valid pointerDrag reference.");
            return;
        }

        if (eventData.pointerDrag.TryGetComponent(out EquippedSpellIconUI droppedSpellUI))
        {
            // 2. ドロップされた呪文データを取得
            SpellBase droppedSpell = droppedSpellUI.GetSpellData();

            if (droppedSpell != null)
            {
                // 3. ドロップ元のUIに成功を通知し、ドロップ元のUIをクリーンアップするロジックを起動
                droppedSpellUI.NotifyDropSuccess();

                // 4. オブザーバー経由で、このスロットに呪文を登録するよう通知する
                _observer?.NotifySpellDroppedOnEmptySlot(droppedSpell, _slotIndex);
                
                Debug.Log($"[EquippedEmptySlotUI {_slotIndex}] Spell '{droppedSpell.spellName}' dropped. Notifying registration.");
            }
            else
            {
                Debug.LogWarning($"[EquippedEmptySlotUI {_slotIndex}] Dropped UI has null SpellData.");
            }
        }
        else
        {
            // ドロップされたものが EquippedSpellIconUI でない場合は無視する
            Debug.Log($"[EquippedEmptySlotUI {_slotIndex}] Dropped object is not a valid SpellIconUI.");
        }
    }
}


// --- インターフェース定義 ---

/// <summary>
/// EquippedEmptySlotUIの操作結果をコントローラー/マネージャーに通知するためのインターフェース。
/// </summary>
public interface IEquippedEmptySlotObserver
{
    /// <summary>
    /// 呪文UIが空のスロットにドロップされたことを通知し、その呪文をスロットに登録するよう要求します。
    /// </summary>
    /// <param name="droppedSpell">ドロップされた呪文のデータ</param>
    /// <param name="targetSlotIndex">呪文を登録する空スロットのインデックス</param>
    void NotifySpellDroppedOnEmptySlot(SpellBase droppedSpell, int targetSlotIndex);
}