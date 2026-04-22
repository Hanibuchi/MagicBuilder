using UnityEngine;

/// <summary>
/// TutorialPointerControllerの動作テスト用スクリプト
/// 同じオブジェクト、または別オブジェクトにアタッチして使用します。
/// </summary>
public class TutorialTestRunner : MonoBehaviour
{
    [SerializeField] private TutorialPointerController tutorialController;

    [Header("Test Settings")]
    [SerializeField] private string testDescription = "これはチュートリアルのテストメッセージです。\n1文字ずつ表示されます。";
    [SerializeField] private Vector2 tapPosition = new Vector2(500, 500);
    [SerializeField] private Vector2 dragStartPosition = new Vector2(200, 200);
    [SerializeField] private Vector2 dragEndPosition = new Vector2(800, 800);

    [ContextMenu("Test: Show Text")]
    public void TestShowText()
    {
        if (tutorialController != null)
        {
            tutorialController.ShowDescription(testDescription);
            Debug.Log("テスト: テキスト表示");
        }
    }

    [ContextMenu("Test: Hide All")]
    public void TestHideAll()
    {
        if (tutorialController != null)
        {
            tutorialController.HideDescription();
            tutorialController.HidePointer();
            Debug.Log("テスト: 非表示");
        }
    }

    [ContextMenu("Test: Tap Animation")]
    public void TestTapAnimation()
    {
        if (tutorialController != null)
        {
            tutorialController.PlayTapAnimation(tapPosition);
            Debug.Log("テスト: タップアニメーション");
        }
    }

    [ContextMenu("Test: Drag Animation")]
    public void TestDragAnimation()
    {
        if (tutorialController != null)
        {
            tutorialController.PlayDragAnimation(dragStartPosition, dragEndPosition);
            Debug.Log("テスト: ドラッグアニメーション");
        }
    }

    private void Update()
    {
        if (tutorialController == null) return;

        // Tキーでテキスト表示のテスト
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestShowText();
        }

        // Hキーでテキストと指マークを非表示にするテスト
        if (Input.GetKeyDown(KeyCode.H))
        {
            TestHideAll();
        }

        // スペースキーでタップアニメーションのテスト
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestTapAnimation();
        }

        // Dキーでドラッグアニメーションのテスト
        if (Input.GetKeyDown(KeyCode.D))
        {
            TestDragAnimation();
        }
    }
}