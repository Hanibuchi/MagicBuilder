using UnityEngine;

/// <summary>
/// タイルのアニメーション（消去・再出現）を制御するクラス
/// </summary>
public class TileAnimationEffect : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    [SerializeField] private string eraseTrigger = "Erase";
    [SerializeField] private string restoreTrigger = "Restore";

    /// <summary>
    /// 消去のアニメーションを再生する
    /// </summary>
    public void PlayEraseAnimation(Sprite sprite, Vector3 position)
    {
        transform.position = position;
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }

        if (animator != null)
        {
            animator.SetTrigger(eraseTrigger);
        }
    }

    /// <summary>
    /// 再出現のアニメーションを再生する
    /// </summary>
    public void PlayRestoreAnimation(Sprite sprite, Vector3 position)
    {
        transform.position = position;
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }

        if (animator != null)
        {
            animator.SetTrigger(restoreTrigger);
        }
    }

    // アニメーション終了時に呼ばれるイベント（Animation Eventなどで使用を想定）
    // あるいはDestroyをここで行う
    public void OnAnimationComplete()
    {
        Destroy(gameObject);
    }
}
