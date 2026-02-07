using UnityEngine;

/// <summary>
/// MegaBeamのチャージ演出を制御するためのクラス。
/// </summary>
public class MegaBeamChargeEffect : MonoBehaviour
{
    /// <summary>
    /// 指定されたオーディオクリップをSoundManager経由で再生します。
    /// アニメーションイベントから呼び出すことを想定しています。
    /// </summary>
    /// <param name="clip">再生するSEのクリップ</param>
    public void PlaySE(AudioClip clip)
    {
        if (clip != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySE(clip);
        }
    }

    /// <summary>
    /// このコンポーネントがアタッチされているGameObjectを破棄します。
    /// アニメーションの終了イベントなどで呼び出すことを想定しています。
    /// </summary>
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
