using UnityEngine;
using UnityEngine.U2D.Animation; // SpriteResolverを使用するために必要

/// <summary>
/// 魔術師が持っている杖の見た目を管理するシングルトンクラス。
/// WandTypeに基づいてSpriteResolverのスプライトを切り替えます。
/// </summary>
public class WandAppearanceManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static WandAppearanceManager Instance { get; private set; }

    [Header("コンポーネント")]
    [Tooltip("杖のスプライトを解決するためのSpriteResolver")]
    [SerializeField] private SpriteResolver wandSpriteResolver;

    // SpriteResolverのカテゴリー名
    private const string WAND_CATEGORY = "wand";

    private void Awake()
    {
        // シングルトンパターンの実装
        if (Instance == null)
        {
            Instance = this;
            // シーンを跨いで保持したい場合は、以下をコメント解除
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 既にインスタンスが存在する場合は、自身を破棄
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 指定されたWandTypeに基づいて杖の見た目を変更します。
    /// </summary>
    /// <param name="type">設定する杖のタイプ</param>
    public void ChangeAppearance(WandType type)
    {
        if (wandSpriteResolver == null)
        {
            Debug.LogError("SpriteResolverがインスペクタで設定されていません。");
            return;
        }

        // WandTypeを文字列に変換し、スプライトのラベルとして使用
        string label = type.ToString();

        try
        {
            // SpriteResolverを使用してスプライトを切り替える
            // カテゴリーは "wand"、ラベルは WandType.ToString() の値
            wandSpriteResolver.SetCategoryAndLabel(WAND_CATEGORY, label);
            Debug.Log($"杖の見た目を {type} (ラベル: {label}) に変更しました。");
        }
        catch (System.Exception e)
        {
            // SpriteResolverの設定ミスなどで例外が発生した場合のハンドリング
            Debug.LogError($"杖の見た目変更に失敗しました。カテゴリー: {WAND_CATEGORY}, ラベル: {label}. エラー: {e.Message}");
        }
    }
}