using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// ステージ選択画面のチュートリアルを制御するクラス
/// </summary>
public class StageSelectTutorialController : MonoBehaviour
{
    [Header("Tutorial Settings")]
    [Tooltip("インスペクタから設定するチュートリアルポインターコントローラー")]
    [SerializeField] private TutorialPointerController pointerController;

    [Tooltip("チュートリアルを開始するステージID（最初のラッシュステージなど）")]
    [SerializeField] private string targetStageIdentifier = "1-4";

    [Tooltip("要求する装備可能な呪文の数")]
    [SerializeField] private int requiredCapacityCount = 4;

    [Tooltip("ドラッグ対象となる特定のSpellBase")]
    [SerializeField] private SpellBase targetDragSpell;

    private bool hasTappedSpellSelect = false;

    private void Start()
    {
        if (StageInfoDisplayUI.Instance != null)
        {
            StageInfoDisplayUI.Instance.OnUIOpened += OnStageInfoDisplayUIOpened;
            
            if (StageInfoDisplayUI.Instance.OpenSpellSelectButton != null)
            {
                StageInfoDisplayUI.Instance.OpenSpellSelectButton.onClick.AddListener(OnSpellSelectButtonClicked);
            }
        }
    }

    private void OnDestroy()
    {
        if (StageInfoDisplayUI.Instance != null)
        {
            StageInfoDisplayUI.Instance.OnUIOpened -= OnStageInfoDisplayUIOpened;
            
            if (StageInfoDisplayUI.Instance.OpenSpellSelectButton != null)
            {
                StageInfoDisplayUI.Instance.OpenSpellSelectButton.onClick.RemoveListener(OnSpellSelectButtonClicked);
            }
        }
    }

    private void OnSpellSelectButtonClicked()
    {
        if (StageInfoDisplayUI.Instance.CurrentStageIdentifier == targetStageIdentifier)
        {
            hasTappedSpellSelect = true;
        }
    }

    private void OnStageInfoDisplayUIOpened(string identifier)
    {
        if (identifier == targetStageIdentifier && !hasTappedSpellSelect)
        {
            StartCoroutine(TutorialSequenceRoutine());
        }
    }

    private IEnumerator TutorialSequenceRoutine()
    {
        if (pointerController == null)
        {
            Debug.LogError("TutorialPointerController が設定されていません。");
            yield break;
        }

        pointerController.ShowDescription("タップして呪文をセット");

        // UIアニメーション完了待ち
        yield return new WaitForSeconds(0.5f);

        while (!hasTappedSpellSelect)
        {
            if (StageInfoDisplayUI.Instance != null && StageInfoDisplayUI.Instance.OpenSpellSelectButton != null)
            {
                RectTransform spellButtonRect = StageInfoDisplayUI.Instance.OpenSpellSelectButton.GetComponent<RectTransform>();
                if (spellButtonRect != null)
                {
                    Camera cam = spellButtonRect.GetComponentInParent<Canvas>()?.worldCamera;
                    // Rectの中心座標（ワールド座標）を取得してからスクリーン座標に変換
                    Vector3 worldCenter = spellButtonRect.TransformPoint(spellButtonRect.rect.center);
                    Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldCenter);
                    
                    pointerController.PlayTapAnimation(screenPos);
                }
            }
            
            // アニメーションのループ時間待機
            float elapsed = 0f;
            while (elapsed < 1.5f)
            {
                if (hasTappedSpellSelect)
                {
                    break;
                }
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        pointerController.HidePointer();
        pointerController.HideDescription();

        // 呪文UIが開くまで少し待機
        yield return new WaitForSeconds(1.0f);

        // 枠拡張チュートリアル
        while (EquippedSpellManager.Instance.GetMaxCapacity() < requiredCapacityCount)
        {
            bool isIncreaseCapacityTapped = false;

            // ① 容量拡張ボタンを指さす
            pointerController.ShowDescription("枠を拡張しよう");

            while (!isIncreaseCapacityTapped)
            {
                if (EquippedSpellSelectionUI.Instance != null && EquippedSpellSelectionUI.Instance.IncreaseCapacityButton != null)
                {
                    RectTransform increaseBtnRect = EquippedSpellSelectionUI.Instance.IncreaseCapacityButton.GetComponent<RectTransform>();
                    if (increaseBtnRect != null)
                    {
                        Camera cam = increaseBtnRect.GetComponentInParent<Canvas>()?.worldCamera;
                        Vector3 worldCenter = increaseBtnRect.TransformPoint(increaseBtnRect.rect.center);
                        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldCenter);
                        
                        pointerController.PlayTapAnimation(screenPos);
                    }
                }

                // 購入UIが開いたかどうかでタップ判定しつつ、アニメーションのループ時間を待機
                float elapsed = 0f;
                while (elapsed < 1.5f)
                {
                    if (CapacityPurchaseUI.Instance != null && CapacityPurchaseUI.Instance.gameObject.activeInHierarchy)
                    {
                        isIncreaseCapacityTapped = true;
                        break;
                    }
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }

            pointerController.HidePointer();
            pointerController.HideDescription();

            yield return new WaitForSeconds(0.5f);

            // ② 購入ボタンを指さす
            int currentCapacity = EquippedSpellManager.Instance.GetMaxCapacity();
            pointerController.ShowDescription("購入して枠を増やす");

            while (EquippedSpellManager.Instance.GetMaxCapacity() == currentCapacity)
            {
                if (CapacityPurchaseUI.Instance != null && CapacityPurchaseUI.Instance.PurchaseButton != null && CapacityPurchaseUI.Instance.gameObject.activeInHierarchy)
                {
                    RectTransform purchaseBtnRect = CapacityPurchaseUI.Instance.PurchaseButton.GetComponent<RectTransform>();
                    if (purchaseBtnRect != null)
                    {
                        Camera cam = purchaseBtnRect.GetComponentInParent<Canvas>()?.worldCamera;
                        Vector3 worldCenter = purchaseBtnRect.TransformPoint(purchaseBtnRect.rect.center);
                        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldCenter);
                        
                        pointerController.PlayTapAnimation(screenPos);
                    }
                }
                else
                {
                    // もし購入UIが閉じられたら（キャンセルされたら）やり直す
                    break;
                }

                float elapsed = 0f;
                while (elapsed < 1.0f)
                {
                    if (EquippedSpellManager.Instance.GetMaxCapacity() > currentCapacity || !CapacityPurchaseUI.Instance.gameObject.activeInHierarchy)
                    {
                        break;
                    }
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }

            pointerController.HidePointer();
            pointerController.HideDescription();
            
            // UIを閉じるアニメーション等があるため待機
            yield return new WaitForSeconds(0.5f);
        }

        // 装備ドラッグチュートリアル
        if (targetDragSpell != null && requiredCapacityCount > 0)
        {
            pointerController.ShowDescription("ドラッグで装備");

            int targetSlotIndex = requiredCapacityCount - 1; // 装備するべき最後の枠
            bool isEquipped = false;

            while (!isEquipped)
            {
                // すでに装備されているかチェック
                var currentSpells = EquippedSpellManager.Instance.GetEquippedSpells();
                if (currentSpells.Count > targetSlotIndex && currentSpells[targetSlotIndex] == targetDragSpell)
                {
                    isEquipped = true;
                    break;
                }

                Vector2 startScreenPos = Vector2.zero;
                Vector2 endScreenPos = Vector2.zero;
                bool canShowAnimation = false;

                if (EquippedSpellSelectionUI.Instance != null)
                {
                    EquippedSpellIconUI targetIconUI = null;
                    foreach (var ui in EquippedSpellSelectionUI.Instance.HoldListSpellUIs)
                    {
                        if (ui != null && ui.GetSpellData() == targetDragSpell)
                        {
                            targetIconUI = ui;
                            break;
                        }
                    }

                    Component targetSlotUI = null;
                    if (EquippedSpellSelectionUI.Instance.EquippedSlotUIs.Count > targetSlotIndex)
                    {
                        targetSlotUI = EquippedSpellSelectionUI.Instance.EquippedSlotUIs[targetSlotIndex];
                    }

                    if (targetIconUI != null && targetSlotUI != null)
                    {
                        RectTransform startRect = targetIconUI.GetComponent<RectTransform>();
                        RectTransform endRect = targetSlotUI.GetComponent<RectTransform>();

                        if (startRect != null && endRect != null)
                        {
                            Camera startCam = startRect.GetComponentInParent<Canvas>()?.worldCamera;
                            Camera endCam = endRect.GetComponentInParent<Canvas>()?.worldCamera;

                            Vector3 startCenter = startRect.TransformPoint(startRect.rect.center);
                            Vector3 endCenter = endRect.TransformPoint(endRect.rect.center);

                            startScreenPos = RectTransformUtility.WorldToScreenPoint(startCam, startCenter);
                            endScreenPos = RectTransformUtility.WorldToScreenPoint(endCam, endCenter);

                            canShowAnimation = true;
                        }
                    }
                }

                if (canShowAnimation)
                {
                    pointerController.PlayDragAnimation(startScreenPos, endScreenPos);
                }

                float elapsed = 0f;
                // アニメーションループ中の装備判定
                while (elapsed < 3.0f) // PlayDragAnimationにかかる時間に合わせる
                {
                    var spells = EquippedSpellManager.Instance.GetEquippedSpells();
                    if (spells.Count > targetSlotIndex && spells[targetSlotIndex] == targetDragSpell)
                    {
                        isEquipped = true;
                        break;
                    }
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }

            pointerController.HidePointer();
            pointerController.HideDescription();
        }
    }
}
