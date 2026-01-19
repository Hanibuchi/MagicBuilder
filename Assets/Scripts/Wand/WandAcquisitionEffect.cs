using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using Unity.Cinemachine;

/// <summary>
/// 杖を取得した際の演出を制御するクラス
/// </summary>
public class WandAcquisitionEffect : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField, Tooltip("杖のプレビュー画像")]
    private Image wandImage;
    [SerializeField, Tooltip("杖の名前を表示するテキスト")]
    private TextMeshProUGUI nameText;
    [SerializeField, Tooltip("杖の説明を表示するテキスト")]
    private TextMeshProUGUI descriptionText;
    [SerializeField, Tooltip("取得ボタン")]
    private Button getButton;

    [Header("Effect References")]
    [SerializeField, Tooltip("演出用のパーティクルシステム")]
    private ParticleSystem effectParticle;
    [SerializeField, Tooltip("演出を制御するアニメーター")]
    private Animator animator;
    [SerializeField, Tooltip("削除対象となるルートオブジェクト（未指定なら自身）")]
    private GameObject rootObject;
    [SerializeField, Tooltip("カメラ振動を制御するインパルスソース")]
    private CinemachineImpulseSource impulseSource;
    [SerializeField, Tooltip("集結演出時の最大カメラ振動強度")]
    private float gatheringImpulseForce = 0.5f;

    [Header("Audio Settings")]
    [SerializeField, Tooltip("最初に鳴らす何かあると気付かせるSE")]
    private AudioClip noticeSE;
    [SerializeField, Range(0f, 2f)] private float noticeSEVolume = 1f;
    [SerializeField, Tooltip("Particleが集まるSE")]
    private AudioClip gatheringSE;
    [SerializeField, Range(0f, 2f)] private float gatheringSEVolume = 1f;
    [SerializeField, Tooltip("集結SEを繰り返す合計時間")]
    private float gatheringDuration = 2f;
    [SerializeField, Tooltip("集結SEの再生間隔")]
    private float gatheringInterval = 0.1f;
    [SerializeField, Tooltip("杖が徐々に登場するときのSE")]
    private AudioClip appearingSE;
    [SerializeField, Range(0f, 2f)] private float appearingSEVolume = 1f;
    [SerializeField, Tooltip("杖が完全に表示されるときのSE")]
    private AudioClip completedSE;
    [SerializeField, Range(0f, 2f)] private float completedSEVolume = 1f;
    [SerializeField, Tooltip("ボタンを押したときのSE")]
    private AudioClip pressedSE;
    [SerializeField, Range(0f, 2f)] private float pressedSEVolume = 1f;

    private UnityAction onGetCallback;

    private void Awake()
    {
        if (getButton != null)
        {
            getButton.onClick.AddListener(OnGetButtonClicked);
        }

        if (rootObject == null)
        {
            rootObject = gameObject;
        }
    }

    /// <summary>
    /// 演出の初期設定を行う
    /// </summary>
    /// <param name="sprite">表示する杖の画像</param>
    /// <param name="particleSprite">演出用パーティクルに使用する画像</param>
    /// <param name="name">表示する名前</param>
    /// <param name="description">表示する説明</param>
    /// <param name="callback">ボタンが押された際のコールバック</param>
    public void Setup(Sprite sprite, Sprite particleSprite, string name, string description, UnityAction callback)
    {
        if (CameraManager.Instance != null && rootObject != null)
        {
            Vector2 camPos = CameraManager.Instance.GetWorldPosition();
            // xとy座標をCameraManagerのに一致させる（2Dプロジェクトのため、リクエストのzはyの意図と推測）
            rootObject.transform.position = new Vector3(camPos.x, camPos.y, rootObject.transform.position.z);
        }

        if (wandImage != null && sprite != null)
        {
            wandImage.sprite = sprite;
            wandImage.SetNativeSize();
        }
        if (nameText != null) nameText.text = name;
        if (descriptionText != null) descriptionText.text = description;
        onGetCallback = callback;

        if (effectParticle != null && particleSprite != null)
        {
            var tsa = effectParticle.textureSheetAnimation;
            tsa.enabled = true;
            tsa.mode = ParticleSystemAnimationMode.Sprites;
            // 最初のスロットにSpriteを設定
            if (tsa.spriteCount > 0)
            {
                tsa.SetSprite(0, particleSprite);
            }
            else
            {
                tsa.AddSprite(particleSprite);
            }
        }
    }

    /// <summary>
    /// 演出を開始します
    /// </summary>
    public void StartEffect()
    {
        if (animator != null)
        {
            animator.SetTrigger("Show");
        }

        PlayNoticeSE();
    }

    private void OnGetButtonClicked()
    {
        PlayPressedSE();

        if (animator != null)
        {
            animator.SetTrigger("Hide");
        }

        onGetCallback?.Invoke();
    }

    #region Animation Events

    /// <summary>
    /// パーティクルを再生します (Animation Eventから呼び出し可能)
    /// </summary>
    public void PlayParticle()
    {
        if (effectParticle != null)
        {
            effectParticle.gameObject.SetActive(true);
            effectParticle.Play();
        }
    }

    /// <summary>
    /// 通知SEを再生します (Animation Eventから呼び出し可能)
    /// </summary>
    public void PlayNoticeSE()
    {
        PlayClip(noticeSE, noticeSEVolume);
    }

    /// <summary>
    /// 集結SEを一定時間、一定間隔で繰り返し再生します (Animation Eventから呼び出し可能)
    /// </summary>
    public void PlayGatheringSE()
    {
        StartCoroutine(RepeatGatheringSE());
    }

    private System.Collections.IEnumerator RepeatGatheringSE()
    {
        if (gatheringInterval <= 0f)
        {
            PlayClip(gatheringSE, gatheringSEVolume);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < gatheringDuration)
        {
            // 時間の経過に合わせて音量を 0 から gatheringSEVolume まで線形補間
            float currentVolume = Mathf.Lerp(gatheringSEVolume * 0.5f, gatheringSEVolume, elapsed / gatheringDuration);
            PlayClip(gatheringSE, currentVolume);

            if (impulseSource != null)
            {
                // カメラ振動強度を 0 から gatheringImpulseForce まで線形補間
                float currentImpulseForce = Mathf.Lerp(0f, gatheringImpulseForce, elapsed / gatheringDuration);
                impulseSource.GenerateImpulseWithForce(currentImpulseForce);
            }

            yield return new WaitForSeconds(gatheringInterval);
            elapsed += gatheringInterval;
        }

        // 最後に最大音量で一回鳴らす、または端数調整
        PlayClip(gatheringSE, gatheringSEVolume);
    }

    /// <summary>
    /// 登場SEを再生します (Animation Eventから呼び出し可能)
    /// </summary>
    public void PlayAppearingSE()
    {
        PlayClip(appearingSE, appearingSEVolume);
    }

    /// <summary>
    /// 完了SEを再生します (Animation Eventから呼び出し可能)
    /// </summary>
    public void PlayCompletedSE()
    {
        PlayClip(completedSE, completedSEVolume);
        impulseSource.GenerateImpulseWithForce(1);
    }

    /// <summary>
    /// ボタン押下SEを再生します (Animation Eventから呼び出し可能)
    /// </summary>
    public void PlayPressedSE()
    {
        PlayClip(pressedSE, pressedSEVolume);
    }

    private void PlayClip(AudioClip clip, float volume)
    {
        if (clip != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySE(clip, volume);
        }
    }

    #endregion

    public void Test()
    {
        Setup(null, null, "普通の杖", "何の変哲もないどこにでもある杖。特に効果はない。", () => Debug.Log("普通の杖が取得されました"));
        StartEffect();
    }
}
