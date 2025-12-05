using UnityEngine;

/// <summary>
/// 呪文以外用。衝突時にダメージ情報をCharacterHealthコンポーネントに提供するコンポーネント。
/// IDamageSourceインターフェースを実装しています。
/// </summary>
public class ProjectileDamageSource : DamageSourceBase
{
    [SerializeField] bool autoDestroy = true;
    // --- 外部参照用変数 (public/SerializeField) ---
    [Tooltip("このオブジェクトが自動で消滅するまでの時間 (秒)")]
    [SerializeField] private float destroyTime = 0.1f;

    [Header("ダメージ設定")]
    [Tooltip("このダメージ源が与える詳細なダメージ情報")]
    // publicかつ[System.Serializable]な構造体であるDamageを直接SerializeFieldとして定義することで、
    // インスペクタと外部スクリプトの両方から編集可能になります。
    [SerializeField] Damage damageData;

    /// <summary>
    /// このダメージ源が持つダメージ情報を取得します。
    /// </summary>
    /// <returns>設定されたDamage構造体。</returns>
    public override Damage GetDamage()
    {
        return damageData;
    }

    private void Start()
    {
        // 指定した時間経過後に自身を破棄
        if (autoDestroy)
            Destroy(gameObject, destroyTime);
        PlayLaunchSound();
    }

    [SerializeField] AudioClip launchSound;
    /// <summary>
    /// 再生する発射音を設定し、再生します。
    /// </summary>
    public void PlayLaunchSound()
    {
        if (launchSound != null)
            AudioSource.PlayClipAtPoint(launchSound, Camera.main.transform.position);
    }
}