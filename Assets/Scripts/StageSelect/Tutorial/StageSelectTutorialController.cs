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
    }
}
