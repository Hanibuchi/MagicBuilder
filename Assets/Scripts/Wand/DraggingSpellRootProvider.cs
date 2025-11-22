// ファイル名: UICanvasRootProvider.cs

using UnityEngine;

/// <summary>
/// ドラッグ中のUIの親となるRectTransformを提供するシングルトンクラス。
/// メインのCanvasルートオブジェクトにアタッチする。
/// </summary>
public class DraggingSpellRootProvider : MonoBehaviour
{
    // シングルトンインスタンス
    private static DraggingSpellRootProvider instance;
    public static DraggingSpellRootProvider Instance => instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    /// <summary>
    /// ドラッグ中のUIの親となるRectTransformを返します。
    /// </summary>
    public RectTransform GetRootTransform()
    {
        return transform as RectTransform;
    }
}