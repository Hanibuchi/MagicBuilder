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
    [SerializeField] private StageSelectUI stageSelectUI;
    [SerializeField] private string firstStageIdentifier = "1-1";

    [Tooltip("チュートリアルを開始するステージID（最初のラッシュステージなど）")]
    [SerializeField] private string targetStageIdentifier = "1-4";

    [Tooltip("要求する装備可能な呪文の数")]
    [SerializeField] private int requiredCapacityCount = 4;

    [Tooltip("ドラッグ対象となる特定のSpellBase")]
    [SerializeField] private SpellBase targetDragSpell;

    private bool hasTappedSpellSelect = false;
    private bool hasTappedWandSelect = false;
    private Coroutine tutorialCoroutine;
    private Coroutine wandTutorialCoroutine;

    private void Awake()
    {
        if (!PlayerPrefs.GetInt("IsStageSelectTutorialDone", 0).Equals(1))
        {
            if (stageSelectUI != null)
            {
                stageSelectUI.OnStagesGeneratedAction += HandleStagesGeneratedForTutorial;
            }
        }
    }

    private void Start()
    {
        if (StageInfoDisplayUI.Instance != null)
        {
            StageInfoDisplayUI.Instance.OnStageInfoSet += OnStageInfoSet;
            StageInfoDisplayUI.Instance.OnUIClosed += StopTutorial;

            if (StageInfoDisplayUI.Instance.OpenSpellSelectButton != null)
            {
                StageInfoDisplayUI.Instance.OpenSpellSelectButton.onClick.AddListener(OnSpellSelectButtonClicked);
            }

            if (StageInfoDisplayUI.Instance.OpenWandSelectButton != null)
            {
                StageInfoDisplayUI.Instance.OpenWandSelectButton.onClick.AddListener(OnWandSelectButtonClicked);
            }
        }
    }

    private void OnDestroy()
    {
        if (stageSelectUI != null)
        {
            stageSelectUI.OnStagesGeneratedAction -= HandleStagesGeneratedForTutorial;
        }

        if (StageInfoDisplayUI.Instance != null)
        {
            StageInfoDisplayUI.Instance.OnStageInfoSet -= OnStageInfoSet;
            StageInfoDisplayUI.Instance.OnUIClosed -= StopTutorial;

            if (StageInfoDisplayUI.Instance.OpenSpellSelectButton != null)
            {
                StageInfoDisplayUI.Instance.OpenSpellSelectButton.onClick.RemoveListener(OnSpellSelectButtonClicked);
            }

            if (StageInfoDisplayUI.Instance.OpenWandSelectButton != null)
            {
                StageInfoDisplayUI.Instance.OpenWandSelectButton.onClick.RemoveListener(OnWandSelectButtonClicked);
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

    private void OnWandSelectButtonClicked()
    {
        hasTappedWandSelect = true;
    }

    private void HandleStagesGeneratedForTutorial(string islandID)
    {
        if (PlayerPrefs.GetInt("IsStageSelectTutorialDone", 0) == 1) return;
        StartCoroutine(FirstStageSelectTutorialRoutine());
    }

    private IEnumerator FirstStageSelectTutorialRoutine()
    {
        yield return new WaitForSeconds(0.01f); // ボタンの生成待ち

        if (stageSelectUI == null || stageSelectUI.StageButtonParent == null) yield break;

        StageButton targetButton = null;
        foreach (Transform child in stageSelectUI.StageButtonParent)
        {
            var btn = child.GetComponent<StageButton>();
            if (btn != null && btn.StageIdentifier == firstStageIdentifier)
            {
                targetButton = btn;
                break;
            }
        }

        if (targetButton == null) yield break;

        bool isClicked = false;
        UnityEngine.Events.UnityAction clickAction = () =>
        {
            isClicked = true;

            PlayerPrefs.SetInt("IsStageSelectTutorialDone", 1);
            PlayerPrefs.Save();
        };
        targetButton.button.onClick.AddListener(clickAction);

        if (pointerController != null)
        {
            pointerController.ShowDescription("ステージを選択");
        }

        while (!isClicked)
        {
            if (pointerController != null)
            {
                var rect = targetButton.GetComponent<RectTransform>();
                if (rect != null)
                {
                    Camera cam = rect.GetComponentInParent<Canvas>()?.worldCamera;
                    Vector3 worldCenter = rect.TransformPoint(rect.rect.center);
                    Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldCenter);

                    pointerController.PlayTapAnimation(screenPos);
                }
            }

            float elapsed = 0f;
            while (elapsed < 1.5f)
            {
                if (isClicked) break;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        if (pointerController != null)
        {
            pointerController.HidePointer();
            pointerController.HideDescription();
        }
        targetButton.button.onClick.RemoveListener(clickAction);
    }

    private void OnStageInfoSet(string identifier)
    {
        StopTutorial();

        if (PlayerPrefs.GetInt("IsWandSelectTutorialDone", 0) != 1
            && WandUnlockManager.Instance != null
            && WandUnlockManager.Instance.HasAnyNewlyUnlockedWands())
        {
            wandTutorialCoroutine = StartCoroutine(WandSelectTutorialRoutine());
        }
        else if (identifier == targetStageIdentifier && !hasTappedSpellSelect)
        {
            tutorialCoroutine = StartCoroutine(SpellSetTutorialSequenceRoutine());
        }
    }

    public void StopTutorial()
    {
        if (tutorialCoroutine != null)
        {
            StopCoroutine(tutorialCoroutine);
            tutorialCoroutine = null;
        }
        if (wandTutorialCoroutine != null)
        {
            StopCoroutine(wandTutorialCoroutine);
            wandTutorialCoroutine = null;
        }
        if (pointerController != null)
        {
            pointerController.HidePointer();
            pointerController.HideDescription();
        }
    }

    private IEnumerator WandSelectTutorialRoutine()
    {
        if (pointerController == null) yield break;

        yield return new WaitForSecondsRealtime(0.5f);

        if (StageInfoDisplayUI.Instance == null
            || StageInfoDisplayUI.Instance.OpenWandSelectButton == null
            || !StageInfoDisplayUI.Instance.OpenWandSelectButton.gameObject.activeInHierarchy)
        {
            yield break;
        }

        hasTappedWandSelect = false;
        pointerController.ShowDescription("杖を確認しよう");

        while (!hasTappedWandSelect)
        {
            if (StageInfoDisplayUI.Instance.OpenWandSelectButton.TryGetComponent<RectTransform>(out var wandButtonRect))
            {
                Canvas canvas = wandButtonRect.GetComponentInParent<Canvas>();
                Camera cam = canvas != null ? canvas.worldCamera : null;
                Vector3 worldCenter = wandButtonRect.TransformPoint(wandButtonRect.rect.center);
                Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldCenter);
                pointerController.PlayTapAnimation(screenPos);
            }

            float elapsed = 0f;
            while (elapsed < 1.5f)
            {
                if (hasTappedWandSelect) break;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        PlayerPrefs.SetInt("IsWandSelectTutorialDone", 1);
        PlayerPrefs.Save();

        pointerController.HidePointer();
        pointerController.HideDescription();
    }

    private IEnumerator SpellSetTutorialSequenceRoutine()
    {
        if (pointerController == null)
        {
            Debug.LogError("TutorialPointerController が設定されていません。");
            yield break;
        }

        int targetSlotIndex = requiredCapacityCount - 1; // 装備するべき最後の枠
        bool isAlreadyEquipped = false;
        var initialSpells = EquippedSpellManager.Instance.GetEquippedSpells();
        if (initialSpells.Count > targetSlotIndex && initialSpells[targetSlotIndex] == targetDragSpell)
        {
            isAlreadyEquipped = true;
        }

        if (!isAlreadyEquipped)
        {
            yield return HandleSpellSelectButtonTutorial(targetSlotIndex);
        } // if (!isAlreadyEquipped) の閉じカッコ

        // 装備完了後もしくはすでに装備済みの場合、UIを閉じる
        if (EquippedSpellController.Instance != null && EquippedSpellSelectionUI.Instance != null && EquippedSpellSelectionUI.Instance.gameObject.activeInHierarchy)
        {
            EquippedSpellController.Instance.CloseSpellSelectionUI();
        }

        yield return new WaitForSecondsRealtime(0.5f);

        yield return HandleStartButtonTutorial();
    }

    private IEnumerator HandleSpellSelectButtonTutorial(int targetSlotIndex)
    {
        while (true)
        {
            // まず装備完了しているかチェック（ループの終了条件）
            var currentSpells = EquippedSpellManager.Instance.GetEquippedSpells();
            if (currentSpells.Count > targetSlotIndex && currentSpells[targetSlotIndex] == targetDragSpell)
            {
                break;
            }

            hasTappedSpellSelect = false;
            pointerController.ShowDescription("タップして呪文をセット");

            // UIアニメーション完了待ち
            yield return new WaitForSecondsRealtime(0.5f);

            while (!hasTappedSpellSelect)
            {
                // UIがすでに開かれているならスキップ
                if (EquippedSpellSelectionUI.Instance != null && EquippedSpellSelectionUI.Instance.IsVisible)
                {
                    hasTappedSpellSelect = true;
                    break;
                }

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
                    if (hasTappedSpellSelect || (EquippedSpellSelectionUI.Instance != null && EquippedSpellSelectionUI.Instance.IsVisible))
                    {
                        hasTappedSpellSelect = true;
                        break;
                    }
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            pointerController.HidePointer();
            pointerController.HideDescription();

            // 呪文UIが開くまで少し待機
            yield return new WaitForSecondsRealtime(1.0f);

            // 呪文UIが開いている間のみ以下のチュートリアルを実行
            Coroutine innerTutorial = StartCoroutine(InnerSpellEquipSequence(targetSlotIndex));

            // UIが閉じられるか、完了するまで待機
            while (EquippedSpellSelectionUI.Instance != null && EquippedSpellSelectionUI.Instance.IsVisible)
            {
                currentSpells = EquippedSpellManager.Instance.GetEquippedSpells();
                if (currentSpells.Count > targetSlotIndex && currentSpells[targetSlotIndex] == targetDragSpell)
                {
                    // 装備完了
                    break;
                }
                yield return null;
            }

            // もしUIが途中で閉じられたら、チュートリアルコルーチンを停止して再スタート
            if (innerTutorial != null)
            {
                StopCoroutine(innerTutorial);
            }

            // 後始末
            pointerController.HidePointer();
            pointerController.HideDescription();

            // 装備完了していたら外側のループも抜ける
            currentSpells = EquippedSpellManager.Instance.GetEquippedSpells();
            if (currentSpells.Count > targetSlotIndex && currentSpells[targetSlotIndex] == targetDragSpell)
            {
                break;
            }
        }
    }

    private IEnumerator InnerSpellEquipSequence(int targetSlotIndex)
    {
        yield return HandleCapacityIncreaseTutorial();
        yield return HandleSpellPurchaseTutorial();
        yield return HandleSpellEquipTutorial(targetSlotIndex);
    }

    private IEnumerator HandleCapacityIncreaseTutorial()
    {
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
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            pointerController.HidePointer();
            pointerController.HideDescription();

            yield return new WaitForSecondsRealtime(0.5f);

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
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            pointerController.HidePointer();
            pointerController.HideDescription();

            // UIを閉じるアニメーション等があるため待機
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    private IEnumerator HandleSpellPurchaseTutorial()
    {
        // まず呪文を持っていなければ購入を促す
        SpellType targetSpellType = SpellDatabase.Instance.GetSpellType(targetDragSpell);

        while (SpellHoldInfoManager.Instance.GetSpellCount(targetSpellType) <= 0)
        {
            // ① 対象の呪文アイコンを指す
            pointerController.ShowDescription("呪文を購入しよう");

            bool isSpellIconTapped = false;
            while (!isSpellIconTapped)
            {
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

                    if (targetIconUI != null)
                    {
                        RectTransform iconRect = targetIconUI.GetComponent<RectTransform>();
                        if (iconRect != null)
                        {
                            Camera cam = iconRect.GetComponentInParent<Canvas>()?.worldCamera;
                            Vector3 worldCenter = iconRect.TransformPoint(iconRect.rect.center);
                            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldCenter);

                            pointerController.PlayTapAnimation(screenPos);
                        }
                    }
                }

                float elapsed = 0f;
                while (elapsed < 1.5f)
                {
                    if (SpellPurchaseUI.Instance != null && SpellPurchaseUI.Instance.gameObject.activeInHierarchy)
                    {
                        isSpellIconTapped = true;
                        break;
                    }
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            pointerController.HidePointer();
            pointerController.HideDescription();
            yield return new WaitForSecondsRealtime(0.5f);

            // ② 購入ボタンを指さす
            pointerController.ShowDescription("購入して獲得");

            while (SpellHoldInfoManager.Instance.GetSpellCount(targetSpellType) <= 0)
            {
                if (SpellPurchaseUI.Instance != null && SpellPurchaseUI.Instance.PurchaseButton != null && SpellPurchaseUI.Instance.gameObject.activeInHierarchy)
                {
                    RectTransform purchaseBtnRect = SpellPurchaseUI.Instance.PurchaseButton.GetComponent<RectTransform>();
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
                    // キャンセルされたらやり直す
                    break;
                }

                float elapsed = 0f;
                while (elapsed < 1.0f)
                {
                    if (SpellHoldInfoManager.Instance.GetSpellCount(targetSpellType) > 0 || !SpellPurchaseUI.Instance.gameObject.activeInHierarchy)
                    {
                        break;
                    }
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            pointerController.HidePointer();
            pointerController.HideDescription();
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    private IEnumerator HandleSpellEquipTutorial(int targetSlotIndex)
    {
        // 装備ドラッグチュートリアル
        if (targetDragSpell != null && requiredCapacityCount > 0)
        {
            pointerController.ShowDescription("ドラッグで装備");

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
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            pointerController.HidePointer();
            pointerController.HideDescription();
        }
    }

    private IEnumerator HandleStartButtonTutorial()
    {
        // プレイ（開始）ボタンを押すよう促す
        pointerController.ShowDescription("プレイ開始！");
        bool isStartTapped = false;

        while (!isStartTapped)
        {
            if (StageInfoDisplayUI.Instance != null && StageInfoDisplayUI.Instance.StartButton != null && StageInfoDisplayUI.Instance.gameObject.activeInHierarchy)
            {
                RectTransform startBtnRect = StageInfoDisplayUI.Instance.StartButton.GetComponent<RectTransform>();
                if (startBtnRect != null)
                {
                    Camera cam = startBtnRect.GetComponentInParent<Canvas>()?.worldCamera;
                    Vector3 worldCenter = startBtnRect.TransformPoint(startBtnRect.rect.center);
                    Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldCenter);

                    pointerController.PlayTapAnimation(screenPos);
                }
            }
            else
            {
                // UIが閉じた場合はスタートされたとみなす
                isStartTapped = true;
                break;
            }

            float elapsed = 0f;
            while (elapsed < 1.5f)
            {
                if (StageInfoDisplayUI.Instance == null || !StageInfoDisplayUI.Instance.gameObject.activeInHierarchy)
                {
                    isStartTapped = true;
                    break;
                }
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        pointerController.HidePointer();
        pointerController.HideDescription();
    }
}
