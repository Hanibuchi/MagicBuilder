// ファイル名: SpellTrashCanController.cs

using UnityEngine;

/// <summary>
/// SpellTrashCanUIの表示/非表示を、SpellUIのドラッグイベントに応じて制御します。
/// ISpellDragHandlerを実装し、WandUIManagerに登録されます。
/// </summary>
public class SpellTrashCanController : MonoBehaviour, ISpellDragHandler
{
    public static SpellTrashCanController Instance { get; private set; }

    [Tooltip("表示/非表示を制御するSpellTrashCanUIのコンポーネント")]
    [SerializeField] private SpellTrashCanUI trashCanUI;
    
    [Tooltip("呪文を完全に削除するためのUI")]
    [SerializeField] private SpellDestroyDropUI spellDestroyDropUI;

    private bool isDestroyDropUIEnabled = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// SpellDestroyDropUIの有効/無効を設定します。
    /// </summary>
    /// <param name="isEnabled">有効にするかどうか</param>
    public void SetDestroyDropUIEnabled(bool isEnabled)
    {
        isDestroyDropUIEnabled = isEnabled;
    }

    void Start()
    {
        // 1. WandUIManagerに自身をドラッグハンドラとして登録
        if (WandUIManager.Instance != null)
        {
            WandUIManager.Instance.RegisterSpellDragHandler(this);
        }
        else
        {
            Debug.LogError("WandUIManagerインスタンスが見つかりません。ゴミ箱の制御ができません。");
            return;
        }

        // 2. 初期状態ではゴミ箱を非表示にしておきます
        if (trashCanUI != null)
        {
            trashCanUI.SetActive(false);
        }
        else
        {
            Debug.LogError("SpellTrashCanUIがインスペクターで設定されていません。");
        }
    }

    void OnDestroy()
    {
        // 破棄時にWandUIManagerから登録解除
        if (WandUIManager.Instance != null)
        {
            WandUIManager.Instance.UnregisterSpellDragHandler(this);
        }
    }

    // --- ISpellDragHandlerの実装 ---

    /// <summary>
    /// SpellUIのドラッグが開始されたとき、ゴミ箱を表示します。
    /// </summary>
    public void OnSpellDragBegan()
    {
        if (trashCanUI != null)
        {
            trashCanUI.SetActive(true);
            
            // フラグが立っていれば削除UIオブジェクトを表示する
            if (isDestroyDropUIEnabled && spellDestroyDropUI != null)
            {
                spellDestroyDropUI.gameObject.SetActive(true);
            }
            else if (spellDestroyDropUI != null)
            {
                spellDestroyDropUI.gameObject.SetActive(false);
            }
            
            Debug.Log("SpellTrashCanを表示しました。");
        }
    }

    /// <summary>
    /// SpellUIのドラッグが終了したとき、ゴミ箱を非表示にします。
    /// </summary>
    public void OnSpellDragEnded()
    {
        if (trashCanUI != null)
        {
            // ドロップ成功/失敗にかかわらずドラッグ終了時に非表示に戻す
            trashCanUI.SetActive(false);
            
            if (spellDestroyDropUI != null)
            {
                spellDestroyDropUI.gameObject.SetActive(false);
            }
            
            Debug.Log("SpellTrashCanを非表示にしました。");
        }
    }
}