using UnityEngine;
using System.Collections;

/// <summary>
/// 特定のステージのチュートリアルを制御するクラス
/// </summary>
public class StageTutorialController : MonoBehaviour
{
    [Header("Tutorial Settings")]
    [Tooltip("インスペクタから設定するチュートリアルポインターコントローラー")]
    [SerializeField] private TutorialPointerController pointerController;

    [Tooltip("発射のドラッグ先となる相対方向（左下）")]
    [SerializeField] private Vector2 fireDragDirection = new Vector2(-2f, -1f);

    [Tooltip("発射に関するドラッグをどこまで伸ばして見せるかの距離")]
    [SerializeField] private float fireDragDistance = 400f;

    [Tooltip("一度のドラッグアニメーションにかかる合計時間")]
    [SerializeField] private float animationLoopTime = 3.0f;

    [Header("Phase 3 Settings (Tap & Drag to First Element)")]
    [Tooltip("倒した後に指示を出すターゲットの敵")]
    [SerializeField] private EnemyController targetEnemy;
    
    [Tooltip("敵を倒してからタップ指示を出すまでの待機時間")]
    [SerializeField] private float waitTimeAfterEnemyDefeat = 3.0f;
    
    [Tooltip("タップしてからドラッグ指示を出すまでの待機時間")]
    [SerializeField] private float waitTimeAfterTap = 1.0f;

    private bool hasEquipped = false;
    private bool hasFired = false;
    private bool isTargetEnemyDead = false;
    private bool hasTappedSecondSpell = false;
    private bool hasEquippedSecondSpell = false;
    private int initialWandSpellCount = 0;

    private void Start()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnEquipSpellsSequenceRoutineFinished += OnTutorialReady;
        }

        if (AimInputReader.Instance != null)
        {
            AimInputReader.Instance.OnMagicFired += OnMagicFired;
        }

        if (SpellInventory.Instance != null)
        {
            SpellInventory.Instance.OnSpellClicked += OnSpellClicked;
        }

        if (targetEnemy != null)
        {
            targetEnemy.OnDie.AddListener(OnTargetEnemyDied);
        }
    }

    private void OnDestroy()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnEquipSpellsSequenceRoutineFinished -= OnTutorialReady;
        }

        if (AimInputReader.Instance != null)
        {
            AimInputReader.Instance.OnMagicFired -= OnMagicFired;
        }

        if (SpellInventory.Instance != null)
        {
            SpellInventory.Instance.OnSpellClicked -= OnSpellClicked;
        }

        if (targetEnemy != null)
        {
            targetEnemy.OnDie.RemoveListener(OnTargetEnemyDied);
        }
    }

    private void OnTargetEnemyDied()
    {
        isTargetEnemyDead = true;
    }

    private void OnSpellClicked(int index)
    {
        // インデックス1が2番目の呪文
        if (index == 1)
        {
            hasTappedSecondSpell = true;
        }
    }

    private void OnTutorialReady()
    {
        if (WandUIManager.Instance != null && AttackManager.Instance != null)
        {
            var wandUI = WandUIManager.Instance.GetWandUI(AttackManager.Instance.GetCurrentWandIndex());
            if (wandUI != null)
            {
                initialWandSpellCount = wandUI.GetSpellCastListeners().Count;
            }
        }
        
        StartCoroutine(TutorialSequenceRoutine());
    }

    private void OnMagicFired()
    {
        hasFired = true;
    }

    private IEnumerator TutorialSequenceRoutine()
    {
        if (pointerController == null)
        {
            Debug.LogError("TutorialPointerController が設定されていません。");
            yield break;
        }

        // 1. ドラッグで呪文を装備
        pointerController.ShowDescription("ドラッグで呪文を装備");

        // UIが構築されるまでのわずかな時間を待機
        yield return new WaitForSeconds(0.5f);

        while (!hasEquipped)
        {
            // 動的に現在の呪文数をチェック（ドラッグドロップ等によって増えた場合装備完了とする）
            if (WandUIManager.Instance != null && AttackManager.Instance != null)
            {
                var wandUI = WandUIManager.Instance.GetWandUI(AttackManager.Instance.GetCurrentWandIndex());
                if (wandUI != null && wandUI.GetSpellCastListeners().Count > initialWandSpellCount)
                {
                    hasEquipped = true;
                    break;
                }
            }

            Vector2 startScreenPos = Vector2.zero;
            Vector2 endScreenPos = Vector2.zero;
            bool canShowAnimation = false;

            if (SpellInventory.Instance != null && WandUIManager.Instance != null && AttackManager.Instance != null)
            {
                RectTransform firstSpellRect = SpellInventory.Instance.GetSpellUIRectTransform(0);
                WandUI currentWand = WandUIManager.Instance.GetWandUI(AttackManager.Instance.GetCurrentWandIndex());
                RectTransform lastWandRect = currentWand != null ? currentWand.GetLastUIElementRectTransform() : null;

                if (firstSpellRect != null && lastWandRect != null)
                {
                    Camera cam = firstSpellRect.GetComponentInParent<Canvas>()?.worldCamera;
                    startScreenPos = RectTransformUtility.WorldToScreenPoint(cam, firstSpellRect.position);
                    Camera wandCam = lastWandRect.GetComponentInParent<Canvas>()?.worldCamera;
                    endScreenPos = RectTransformUtility.WorldToScreenPoint(wandCam, lastWandRect.position);
                    canShowAnimation = true;
                }
            }

            if (canShowAnimation)
            {
                pointerController.PlayDragAnimation(startScreenPos, endScreenPos);
            }

            float elapsed = 0f;
            while (elapsed < animationLoopTime)
            {
                // アニメーション再生中にも装備が完了したかチェックする
                if (WandUIManager.Instance != null && AttackManager.Instance != null)
                {
                    var wandUI = WandUIManager.Instance.GetWandUI(AttackManager.Instance.GetCurrentWandIndex());
                    if (wandUI != null && wandUI.GetSpellCastListeners().Count > initialWandSpellCount)
                    {
                        hasEquipped = true;
                        break; // すぐに次のステップへ移行
                    }
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        pointerController.HidePointer();

        // 2. ドラッグで発射
        pointerController.ShowDescription("ドラッグで発射");

        // 次のステップに移る前に少し間を設ける
        yield return new WaitForSeconds(0.5f);

        while (!hasFired)
        {
            Vector2 startScreenPos = Vector2.zero;
            bool canShowAnimation = false;

            if (AimInputReader.Instance != null)
            {
                startScreenPos = AimInputReader.Instance.StartPointScreenPosition;
                canShowAnimation = true;
            }

            if (canShowAnimation)
            {
                Vector2 endScreenPos = startScreenPos + (fireDragDirection.normalized * fireDragDistance);
                pointerController.PlayDragAnimation(startScreenPos, endScreenPos);
            }

            float elapsed = 0f;
            while (elapsed < animationLoopTime)
            {
                if (hasFired)
                {
                    break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        pointerController.HidePointer();
        pointerController.HideDescription();
        
        // 3. 特定の敵を倒した後の指示 (Phase 3)
        // 敵が設定されていない場合はチュートリアル終了
        if (targetEnemy != null)
        {
            // 目標の敵が死ぬまで待機
            while (!isTargetEnemyDead)
            {
                yield return null;
            }

            // 倒してから一定時間待つ
            yield return new WaitForSeconds(waitTimeAfterEnemyDefeat);

            // 3-1. 2番目のSpellUIをタップするよう指示
            pointerController.ShowDescription("タップして詳細を確認");

            // UIの生成待ちなどを考慮して少し待機
            yield return new WaitForSeconds(0.5f);

            while (!hasTappedSecondSpell)
            {
                Vector2 tapScreenPos = Vector2.zero;
                bool canShowTap = false;

                if (SpellInventory.Instance != null)
                {
                    // 2番目の要素 (index 1)
                    RectTransform secondSpellRect = SpellInventory.Instance.GetSpellUIRectTransform(1);
                    if (secondSpellRect != null)
                    {
                        Camera cam = secondSpellRect.GetComponentInParent<Canvas>()?.worldCamera;
                        tapScreenPos = RectTransformUtility.WorldToScreenPoint(cam, secondSpellRect.position);
                        canShowTap = true;
                    }
                }

                if (canShowTap)
                {
                    pointerController.PlayTapAnimation(tapScreenPos);
                }

                float elapsedTap = 0f;
                while (elapsedTap < 1.5f) // タップアニメーションのループ時間
                {
                    if (hasTappedSecondSpell) break;
                    elapsedTap += Time.deltaTime;
                    yield return null;
                }
            }

            pointerController.HidePointer();
            pointerController.HideDescription();

            // タップしてから一定時間待つ
            yield return new WaitForSeconds(waitTimeAfterTap);

            // 杖にセットされている現在の呪文数を取得して2番目の呪文がセットされたかを判定する
            int currentWandSpellCount = 0;
            if (WandUIManager.Instance != null && AttackManager.Instance != null)
            {
                var wandUI = WandUIManager.Instance.GetWandUI(AttackManager.Instance.GetCurrentWandIndex());
                if (wandUI != null)
                {
                    currentWandSpellCount = wandUI.GetSpellCastListeners().Count;
                }
            }

            // 3-2. 2番目のSpellUIからWandUIの最初のuiElementsに向かってドラッグする指示
            pointerController.ShowDescription("ドラッグして呪文をセット");

            // 少し待機
            yield return new WaitForSeconds(0.5f);

            while (!hasEquippedSecondSpell)
            {
                if (WandUIManager.Instance != null && AttackManager.Instance != null)
                {
                    var wandUI = WandUIManager.Instance.GetWandUI(AttackManager.Instance.GetCurrentWandIndex());
                    if (wandUI != null && wandUI.GetSpellCastListeners().Count > currentWandSpellCount)
                    {
                        hasEquippedSecondSpell = true;
                        break;
                    }
                }

                Vector2 startScreenPos = Vector2.zero;
                Vector2 endScreenPos = Vector2.zero;
                bool canShowAnimation = false;

                if (SpellInventory.Instance != null && WandUIManager.Instance != null && AttackManager.Instance != null)
                {
                    RectTransform secondSpellRect = SpellInventory.Instance.GetSpellUIRectTransform(1);
                    WandUI currentWand = WandUIManager.Instance.GetWandUI(AttackManager.Instance.GetCurrentWandIndex());
                    RectTransform firstWandElementRect = currentWand != null ? currentWand.GetFirstUIElementRectTransform() : null;

                    if (secondSpellRect != null && firstWandElementRect != null)
                    {
                        Camera cam1 = secondSpellRect.GetComponentInParent<Canvas>()?.worldCamera;
                        startScreenPos = RectTransformUtility.WorldToScreenPoint(cam1, secondSpellRect.position);
                        Camera cam2 = firstWandElementRect.GetComponentInParent<Canvas>()?.worldCamera;
                        endScreenPos = RectTransformUtility.WorldToScreenPoint(cam2, firstWandElementRect.position);
                        canShowAnimation = true;
                    }
                }

                if (canShowAnimation)
                {
                    pointerController.PlayDragAnimation(startScreenPos, endScreenPos);
                }

                float elapsedDrag = 0f;
                while (elapsedDrag < animationLoopTime)
                {
                    if (WandUIManager.Instance != null && AttackManager.Instance != null)
                    {
                        var wandUI = WandUIManager.Instance.GetWandUI(AttackManager.Instance.GetCurrentWandIndex());
                        if (wandUI != null && wandUI.GetSpellCastListeners().Count > currentWandSpellCount)
                        {
                            hasEquippedSecondSpell = true;
                            break;
                        }
                    }

                    elapsedDrag += Time.deltaTime;
                    yield return null;
                }
            }

            // Phase 3 終了
            pointerController.HidePointer();
            pointerController.HideDescription();
        }
    }
}
