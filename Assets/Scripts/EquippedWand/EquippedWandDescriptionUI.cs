using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// 装備中の杖の情報を表示するUI。
/// 杖の画像、名称、説明、および固定呪文を表示します。
/// </summary>
public class EquippedWandDescriptionUI : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler, IEquippedWandDraggable, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField, Tooltip("杖の見た目を表示するImage")]
    private Image wandImage;
    [SerializeField, Tooltip("杖の名前を表示するテキスト")]
    private TextMeshProUGUI nameText;
    [SerializeField, Tooltip("杖の説明を表示するテキスト")]
    private TextMeshProUGUI descriptionText;
    [SerializeField, Tooltip("固定呪文アイコンを配置する親要素")]
    private Transform fixedSpellsContainer;
    [SerializeField, Tooltip("固定呪文の表示に使用するアイコンプレハブ")]
    private SimpleSpellUI simpleSpellUIPrefab;
    [SerializeField]
    Image dragSourceImage;
    [SerializeField]
    private Image frameImage;
    [SerializeField]
    private Material defaultMaterial;
    [SerializeField]
    private Material hoverMaterial;
    [SerializeField, Tooltip("ドラッグ開始時の効果音")]
    private AudioClip dragStartClip;

    [Header("バッジUI")]
    [SerializeField] private GameObject newBadge;

    private Wand _wand;
    private int _slotIndex = -1;
    private bool _dropSucceeded;
    private bool _canDrag = true;
    private Transform _originalParent;
    private int _originalSiblingIndex;
    private IEquippedWandDragObserver _observer;

    /// <summary>
    /// 杖の表示内容を更新します。
    /// </summary>
    public void SetData(Wand wand)
    {
        _wand = wand;

        if (wand == null)
        {
            ClearFixedSpellIcons();
            if (wandImage != null)
            {
                wandImage.sprite = null;
            }
            if (nameText != null)
            {
                nameText.text = string.Empty;
            }
            if (descriptionText != null)
            {
                descriptionText.text = string.Empty;
            }
            return;
        }

        if (wandImage != null)
        {
            wandImage.sprite = wand.wandSprite;
        }

        if (nameText != null)
        {
            nameText.text = wand.wandName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = wand.description;
        }

        UpdateFixedSpellIcons(wand.fixedSpells);
    }

    public void SetSlotIndex(int slotIndex)
    {
        _slotIndex = slotIndex;
    }

    public void SetNewBadgeActive(bool active)
    {
        if (newBadge != null)
        {
            newBadge.SetActive(active);
        }
    }

    public void SetObserver(IEquippedWandDragObserver observer)
    {
        _observer = observer;
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
        return true;
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

        if (SoundManager.Instance != null && dragStartClip != null)
        {
            SoundManager.Instance.PlaySE(dragStartClip);
        }

        if (dragSourceImage != null)
        {
            dragSourceImage.raycastTarget = false;
        }

        SetNewBadgeActive(false);

        RectTransform root = DraggingSpellRootProvider.Instance != null
            ? DraggingSpellRootProvider.Instance.GetRootTransform()
            : null;
        if (root != null)
        {
            transform.SetParent(root, true);
        }

        _observer?.NotifyDragStarted(_wand, _slotIndex, true, _originalSiblingIndex);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        StartCoroutine(HandleDropResult());
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
        {
            return;
        }

        ResetMaterial();

        if (!eventData.pointerDrag.TryGetComponent(out IEquippedWandDraggable dragged))
        {
            return;
        }

        Wand droppedWand = dragged.GetWandData();
        if (droppedWand == null)
        {
            return;
        }

        dragged.NotifyDropSucceeded();
        _observer?.NotifyDroppedOnEquippedSlot(droppedWand, _slotIndex);
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

    private IEnumerator HandleDropResult()
    {
        yield return null;

        if (dragSourceImage != null)
        {
            dragSourceImage.raycastTarget = true;
        }

        if (!_dropSucceeded)
        {
            _observer?.NotifyDroppedOutside(_wand, _slotIndex, true);
        }
        else
        {
            _observer?.NotifyDropCompleted(_wand, _slotIndex, true);
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

    private void UpdateFixedSpellIcons(System.Collections.Generic.List<SpellBase> fixedSpells)
    {
        ClearFixedSpellIcons();

        if (fixedSpellsContainer == null || simpleSpellUIPrefab == null || fixedSpells == null)
        {
            return;
        }

        foreach (var fixedSpell in fixedSpells)
        {
            if (fixedSpell == null)
            {
                continue;
            }

            SimpleSpellUI spellUI = Instantiate(simpleSpellUIPrefab, fixedSpellsContainer);
            spellUI.transform.localScale = Vector3.one * 0.5f;
            spellUI.SetData(fixedSpell);
        }
    }

    private void ClearFixedSpellIcons()
    {
        if (fixedSpellsContainer == null)
        {
            return;
        }

        foreach (Transform child in fixedSpellsContainer)
        {
            Destroy(child.gameObject);
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