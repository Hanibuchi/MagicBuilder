using UnityEngine;
using System.Collections;

/// <summary>
/// 呪文発射時の魔法陣演出を制御するクラス
/// </summary>
public class MagicCircle : MonoBehaviour
{
    [Header("コンポーネント設定")]
    [SerializeField, Tooltip("制御対象のSpriteRenderer。インスペクタから設定してください。")]
    private SpriteRenderer[] spriteRenderers;

    [Header("音響設定")]
    [SerializeField, Tooltip("表示時に再生するSE")]
    private AudioClip showSE;
    [SerializeField, Range(0f, 1f), Tooltip("SEの音量")]
    private float seVolume = 1f;

    // 透明度の目標値 (不透明 100 としたとき 80)
    private const float TargetAlpha = 0.8f;

    /// <summary>
    /// 魔法陣を表示します。
    /// </summary>
    /// <param name="duration">完全に表示されるまでの時間</param>
    /// <param name="color">色（オプション。指定しない場合はそのままの色）</param>
    /// <param name="animateScale">サイズを0からアニメーションさせるかどうか（falseの場合は最初から現在の大きさ）</param>
    public void Show(float duration, Color? color = null, bool animateScale = true)
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            Debug.LogError($"[MagicCircle] No SpriteRenderers are assigned on {gameObject.name}");
            return;
        }

        // 初期サイズの取得
        float targetSize = transform.localScale.x;

        // SEの再生
        if (SoundManager.Instance != null && showSE != null)
        {
            SoundManager.Instance.PlaySE(showSE, seVolume);
        }

        // 初期状態の設定
        foreach (var sr in spriteRenderers)
        {
            if (sr == null) continue;
            Color targetColor = color ?? sr.color;
            targetColor.a = 0f;
            sr.color = targetColor;
        }
        transform.localScale = animateScale ? Vector3.zero : Vector3.one * targetSize;

        StopAllCoroutines();
        StartCoroutine(AnimateShow(targetSize, duration, animateScale));
    }

    /// <summary>
    /// 魔法陣を表示時と同じ時間をかけて非表示にし、オブジェクトを破棄します。
    /// </summary>
    /// <param name="duration">消滅にかかる時間</param>
    /// <param name="animateScale">サイズを0へアニメーションさせるかどうか</param>
    public void Hide(float duration, bool animateScale = true)
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0) return;

        StopAllCoroutines();
        StartCoroutine(AnimateHide(duration, animateScale));
    }

    private IEnumerator AnimateShow(float targetSize, float duration, bool animateScale)
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
            // 正弦波（0からPI/2）を使用してイージング
            float sineT = Mathf.Sin(t * Mathf.PI * 0.5f);
            float currentSize = animateScale ? Mathf.Lerp(0f, targetSize, sineT) : targetSize;
            ApplyState(currentSize, Mathf.Lerp(0f, TargetAlpha, sineT));
            yield return null;
        }

        ApplyState(targetSize, TargetAlpha);
    }

    private IEnumerator AnimateHide(float duration, bool animateScale)
    {
        float elapsed = 0f;
        float startSize = transform.localScale.x;
        float startAlpha = 0f;
        if (spriteRenderers != null && spriteRenderers.Length > 0 && spriteRenderers[0] != null)
        {
            startAlpha = spriteRenderers[0].color.a;
        }

        if (duration <= 0)
        {
            Destroy(gameObject);
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // 正弦波を使用してイージング
            float sineT = Mathf.Sin(t * Mathf.PI * 0.5f);
            float currentSize = animateScale ? Mathf.Lerp(startSize, 0f, sineT) : startSize;
            ApplyState(currentSize, Mathf.Lerp(startAlpha, 0f, sineT));
            yield return null;
        }

        Destroy(gameObject);
    }

    private void ApplyState(float size, float alpha)
    {
        transform.localScale = Vector3.one * size;
        foreach (var sr in spriteRenderers)
        {
            if (sr == null) continue;
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }
}
