using UnityEngine;
using System.Collections;

/// <summary>
/// 呪文発射時の魔法陣演出を制御するクラス
/// </summary>
public class MagicCircle : MonoBehaviour
{
    [Header("コンポーネント設定")]
    [SerializeField, Tooltip("制御対象のSpriteRenderer。インスペクタから設定してください。")]
    private SpriteRenderer spriteRenderer;

    // 透明度の目標値 (不透明 100 としたとき 80)
    private const float TargetAlpha = 0.8f;

    /// <summary>
    /// 魔法陣を表示します。
    /// </summary>
    /// <param name="duration">完全に表示されるまでの時間</param>
    /// <param name="size">最終的なサイズ（スケール。既定は1）</param>
    /// <param name="color">色（オプション。指定しない場合はそのままの色）</param>
    public void Show(float duration, float size = 1f, Color? color = null)
    {
        if (spriteRenderer == null)
        {
            Debug.LogError($"[MagicCircle] SpriteRenderer is not assigned on {gameObject.name}");
            return;
        }

        // 初期状態の設定
        Color targetColor = color ?? spriteRenderer.color;
        targetColor.a = 0f;
        spriteRenderer.color = targetColor;
        transform.localScale = Vector3.zero;

        StopAllCoroutines();
        StartCoroutine(AnimateShow(size, duration));
    }

    /// <summary>
    /// 魔法陣を表示時と同じ時間をかけて非表示にし、オブジェクトを破棄します。
    /// </summary>
    /// <param name="duration">消滅にかかる時間</param>
    public void Hide(float duration)
    {
        if (spriteRenderer == null) return;

        StopAllCoroutines();
        StartCoroutine(AnimateHide(duration));
    }

    private IEnumerator AnimateShow(float targetSize, float duration)
    {
        float elapsed = 0f;

        // durationが0の場合は即座に目標値へ
        if (duration <= 0)
        {
            ApplyState(targetSize, TargetAlpha);
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            ApplyState(Mathf.Lerp(0f, targetSize, t), Mathf.Lerp(0f, TargetAlpha, t));
            yield return null;
        }

        ApplyState(targetSize, TargetAlpha);
    }

    private IEnumerator AnimateHide(float duration)
    {
        float elapsed = 0f;
        float startSize = transform.localScale.x;
        float startAlpha = spriteRenderer.color.a;

        if (duration <= 0)
        {
            Destroy(gameObject);
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            ApplyState(Mathf.Lerp(startSize, 0f, t), Mathf.Lerp(startAlpha, 0f, t));
            yield return null;
        }

        Destroy(gameObject);
    }

    private void ApplyState(float size, float alpha)
    {
        transform.localScale = Vector3.one * size;
        Color c = spriteRenderer.color;
        c.a = alpha;
        spriteRenderer.color = c;
    }
}
