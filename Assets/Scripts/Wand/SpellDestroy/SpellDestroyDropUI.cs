using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// SpellUIをドロップすることで、その呪文をゲームから完全に削除（破棄）する機能を担当するUI。
/// SpellDestroyControllerを呼び出して確認ダイアログを表示します。
/// </summary>
public class SpellDestroyDropUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Visuals")]
    [SerializeField] private Image targetImage; // 色やマテリアルを変える対象の画像
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material hoverMaterial;

    private void Awake()
    {
        if (targetImage != null)
        {
            targetImage.material = defaultMaterial;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // SpellUIがドラッグされている時のみホバー効果を発動
        if (eventData.pointerDrag != null && eventData.pointerDrag.TryGetComponent<SpellUI>(out _))
        {
            if (targetImage != null)
            {
                targetImage.material = hoverMaterial;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // ホバーが外れたら元のマテリアルに戻す
        if (targetImage != null)
        {
            targetImage.material = defaultMaterial;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // ドロップされたらホバー状態を解除
        if (targetImage != null)
        {
            targetImage.material = defaultMaterial;
        }
        if (eventData.pointerDrag == null) return;

        // ドロップされたオブジェクトが SpellUI であるかを確認します。
        SpellUI droppedSpellUI = eventData.pointerDrag.GetComponent<SpellUI>();

        if (droppedSpellUI != null)
        {
            // まずWandUIなどから外すためにNotifyDropSuccessを呼ぶ
            droppedSpellUI.NotifyDropSuccess();

            SpellBase spellData = droppedSpellUI.GetSpellData();

            // コントローラーを通じて削除確認画面を呼び出す
            if (SpellDestroyController.Instance != null && spellData != null)
            {
                SpellDestroyController.Instance.RequestDestroySpell(spellData);
            }
        }
    }
}
