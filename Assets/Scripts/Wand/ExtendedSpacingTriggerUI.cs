// ファイル名: ExtendedSpacingTriggerUI.cs

using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// SpacingUIの感知範囲を拡張するためのクラス。
/// ドラッグ中のみSetActive(true)にして使用する。
/// </summary>
public class ExtendedSpacingTriggerUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    // 親のSpacingUIから、必要なメソッドを委譲してもらうための参照
    private SpacingUI spacingUI;

    public void Initialize(SpacingUI parentSpacingUI)
    {
        this.spacingUI = parentSpacingUI;
    }

    // --- IDropHandler の委譲 ---
    public void OnDrop(PointerEventData eventData)
    {
        // 処理を親のSpacingUIに委譲する
        spacingUI.OnDrop(eventData);

        // ドロップ成功後、自身は非アクティブ化されることが期待される
    }

    // --- IPointerEnterHandler の委譲 ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 処理を親のSpacingUIに委譲する
        spacingUI.OnPointerEnter(eventData);
    }

    // --- IPointerExitHandler の委譲 ---
    public void OnPointerExit(PointerEventData eventData)
    {
        // 処理を親のSpacingUIに委譲する
        spacingUI.OnPointerExit(eventData);
    }
}