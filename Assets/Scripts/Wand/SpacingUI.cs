// ファイル名: SpacingUI.cs

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 呪文と呪文の間にあるスペース。呪文のドロップ（追加）を受け付ける。
/// </summary>
public class SpacingUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    private int index;
    private WandUI wandUI;
    private Animator animator;

    // Animatorのトリガー名
    private const string HighlightTrigger = "Highlight";
    private const string NormalTrigger = "Normal";

    [SerializeField] ExtendedSpacingTriggerUI extendedTriggerUI;
    [SerializeField] Image image;

    void Awake()
    {
        animator = GetComponent<Animator>();
        extendedTriggerUI.Initialize(this);
        SetExtendedTriggerActive(false);
    }

    public void Initialize(WandUI parentWandUI)
    {
        this.wandUI = parentWandUI;
        // 初期状態では非アクティブ
    }

    public void SetIndex(int newIndex)
    {
        this.index = newIndex;
        this.gameObject.name = $"SpacingUI_{index}";
    }

    // --- ドロップ処理 ---

    [SerializeField]
    private AudioClip spellDropSound; // ドラッグ開始時に再生するAudioClip
    [SerializeField] float spellDropSoundVolume = 1.0f;
    public void OnDrop(PointerEventData eventData)
    {
        // ドロップされたオブジェクトがSpellUIであるかを確認
        SpellUI droppedSpellUI = eventData.pointerDrag.GetComponent<SpellUI>();

        if (droppedSpellUI != null)
        {
            // 1. **移動元がこの杖であるかどうかの判定**
            bool isMovingFromSelf = (droppedSpellUI.spellContainerUI is WandUI spellWandUI && wandUI == spellWandUI);

            // 2. **WandUIに挿入可能かどうかの判定を要求**
            if (!wandUI.CanDropSpell(isMovingFromSelf)) // ★ 挿入判定を追加
            {
                // 挿入不可の場合、処理を中断
                StopHighlight();
                PlayInventoryFullSound();
                return;
            }

            // --- 挿入可能な場合の処理 ---

            if (SoundManager.Instance != null && spellDropSound != null)
                SoundManager.Instance.PlaySE(spellDropSound, spellDropSoundVolume);

            SpellBase spellToAdd = droppedSpellUI.GetSpellData();
            droppedSpellUI.NotifyDropSuccess();

            // ドラッグ完了と同時に要素の削除と追加を行うとずれる問題の回避（既存ロジック）
            if (isMovingFromSelf && droppedSpellUI.index < index) index--;

            // 3. **アニメーションをリセット**
            if (animator != null)
            {
                StopHighlight();
            }

            // 4. **WandUIに呪文の追加を通知**
            wandUI.NotifySpellAdded(index, spellToAdd);
        }
    }

    // --- ドロップオーバー時のアニメーション処理 ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        // ドラッグ中のオブジェクトがある場合のみ処理
        if (eventData.pointerDrag != null)
        {
            SpellUI droppedSpellUI = eventData.pointerDrag.GetComponent<SpellUI>();

            if (droppedSpellUI != null)
            {
                wandUI.NotifySpellEntered(this);

                bool isMovingFromSelf = (droppedSpellUI.spellContainerUI is WandUI spellWandUI && wandUI == spellWandUI);
                // WandUIに通知し、追加可能かどうか（canAdd）を受け取る
                bool canAdd = wandUI.CanDropSpell(isMovingFromSelf);

                if (canAdd) // 呪文の追加が可能な場合のみハイライト
                {
                    if (animator != null)
                    {
                        animator.ResetTrigger(NormalTrigger);
                        animator.SetTrigger(HighlightTrigger);
                    }
                }
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (animator != null)
        {
            StopHighlight();
        }
    }

    /// <summary>
    /// 他のSpacingUIからハイライト解除要求が来た時に呼ばれる。
    /// </summary>
    public void StopHighlight()
    {
        if (animator != null)
        {
            animator.SetTrigger(NormalTrigger);
            animator.ResetTrigger(HighlightTrigger);
        }
    }

    /// <summary>
    /// 拡張トリガーUIを有効/無効にする（WandUIから呼び出される）
    /// </summary>
    public void SetExtendedTriggerActive(bool isActive)
    {
        Debug.Log("isActive" + isActive);
        if (extendedTriggerUI != null)
        {
            extendedTriggerUI.gameObject.SetActive(isActive);
        }
        SetImageAlpha(isActive);
    }

    void SetImageAlpha(bool isActive)
    {
        if (image != null)
        {
            image.color = isActive ? new Color(image.color.r, image.color.g, image.color.b, 0.5f) : new Color(image.color.r, image.color.g, image.color.b, 0f);
        }
    }

    [Header("Sound Settings")]
    [SerializeField] AudioClip inventoryFullSound; // インベントリ満杯時のSE
    /// <summary>
    /// インベントリ満杯時のSEを再生します。
    /// </summary>
    void PlayInventoryFullSound()
    {
        if (SoundManager.Instance != null && inventoryFullSound != null)
            SoundManager.Instance.PlaySE(inventoryFullSound);
    }
}