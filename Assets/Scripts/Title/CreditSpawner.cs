using UnityEngine;
using UnityEngine.UI;

public class CreditSpawner : MonoBehaviour
{
    [Tooltip("生成するCreditUIControllerコンポーネントを持つプレハブ (CreditUICanvas)")]
    public GameObject creditUIPrefab;

    [Tooltip("この機能を開始するためのボタン")]
    public Button spawnButton;

    // UIが現在アクティブかどうかを追跡するフラグ
    private bool isUIActive = false;

    void Awake()
    {
        if (spawnButton != null)
        {
            // ボタンにリスナーを設定
            spawnButton.onClick.AddListener(SpawnCreditUI);
            // 初期状態ではボタンを有効化
            spawnButton.interactable = true;
        }
        else
        {
            Debug.LogError("SpawnButtonが設定されていません。", this);
        }
    }

    /// <summary>
    /// クレジットUIを生成し、アクティブ状態を管理します。
    /// </summary>
    private void SpawnCreditUI()
    {
        // 既にUIがアクティブな場合は、何もしない（一意性を保証）
        if (isUIActive)
        {
            Debug.Log("クレジットUIは既に表示されています。");
            return;
        }

        // 1. UI生成フラグをONにし、ボタンを無効化
        isUIActive = true;
        spawnButton.interactable = false;

        // 2. プレハブをインスタンス化
        GameObject uiInstance = Instantiate(creditUIPrefab);

        // 3. CreditUIControllerを取得
        CreditUIController controller = uiInstance.GetComponent<CreditUIController>();

        if (controller != null)
        {
            // 4. Initメソッドを呼び出し、UIが閉じられた後に実行するアクションを渡す
            controller.Init(() =>
            {
                // UIが破棄されたときに実行されるコールバック
                OnCreditUIClosed();
            });
        }
        else
        {
            Debug.LogError("生成されたプレハブにCreditUIControllerが見つかりません。", this);
            // エラーが発生したら状態をリセット
            OnCreditUIClosed();
        }
    }

    /// <summary>
    /// クレジットUIが閉じられ、破棄された後に呼び出されます。
    /// </summary>
    private void OnCreditUIClosed()
    {
        // UI非アクティブフラグを戻す
        isUIActive = false;

        // ボタンを再び有効化
        if (spawnButton != null)
        {
            spawnButton.interactable = true;
        }
    }
}