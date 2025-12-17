using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 持ち込み呪文選択画面のコントローラー。
/// Modelからの変更通知をUIに伝え、UIからの操作をModel/Managerに反映させる橋渡しを行います。
/// </summary>
public class EquippedSpellController : MonoBehaviour,
    IEquippedSpellModelObserver,
    IEquippedSpellUIProvider
{
    // --- シングルトン実装 ---
    private static EquippedSpellController _instance;
    public static EquippedSpellController Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject singletonObject = new GameObject(nameof(EquippedSpellController));
                _instance = singletonObject.AddComponent<EquippedSpellController>();
                DontDestroyOnLoad(singletonObject);
            }
            return _instance;
        }
    }

    // --- 参照 ---
    [SerializeField] private EquippedSpellSelectionUI _selectionUI;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Modelのオブザーバーとして自身を登録
        EquippedSpellModel.Instance.SetObserver(this);

        // UIの初期化（Providerとして自身を渡す）
        if (_selectionUI != null)
        {
            _selectionUI.Init(this);
        }
    }

    /// <summary>
    /// UIへの参照をセットします（シーン遷移時などに利用）。
    /// </summary>
    public void SetUI(EquippedSpellSelectionUI ui)
    {
        _selectionUI = ui;
        if (_selectionUI != null)
        {
            _selectionUI.Init(this);
        }
    }

    // --- IEquippedSpellModelObserver の実装 (Model -> UI) ---

    public void OnAllSpellStatusesChanged(IReadOnlyList<SpellHoldStatus> allSpellStatuses)
    {
        if (_selectionUI != null)
        {
            _selectionUI.SetHoldSpells(allSpellStatuses);
        }
    }

    public void OnEquippedSpellsChanged(IReadOnlyList<SpellBase> equippedSpells)
    {
        if (_selectionUI != null)
        {
            _selectionUI.SetEquippedSpells(equippedSpells);
        }
    }

    public void OnMaxCapacityChanged(int newCapacity)
    {
        // 現状のUIに最大容量専用の更新メソッドがないため、
        // 必要に応じて UI 側の拡張（スロット枠の増減など）をここで呼び出します。
        Debug.Log($"[Controller] Max Capacity Changed to: {newCapacity}");
    }


    // --- IEquippedSpellUIProvider の実装 (UI -> Model/Manager) ---

    public IReadOnlyList<SpellHoldStatus> GetAllSpellStatuses()
    {
        return EquippedSpellModel.Instance.GetAllSpellStatuses();
    }

    public IReadOnlyList<SpellBase> GetCurrentEquippedSpells()
    {
        return EquippedSpellModel.Instance.GetEquippedSpells();
    }

    public void SetSpell(int index, SpellBase spell)
    {
        // Managerを介してデータを更新。
        // 更新されると EquippedSpellManager -> EquippedSpellModel -> Controller(Observer) 
        // の順で通知が戻り、最終的に UI が再構築されます。
        EquippedSpellManager.Instance.SetSpell(index, spell);
    }

    public void RemoveSpell(int index)
    {
        EquippedSpellManager.Instance.RemoveSpell(index);
    }

    /// <summary>
    /// UIからの容量増加リクエストを処理します。
    /// </summary>
    public void RequestIncreaseCapacity()
    {
        if (CanIncreaseCapacity())
        {
            // 条件を満たしていればManagerの容量増加を呼び出す
            EquippedSpellManager.Instance.IncreaseCapacity();
            Debug.Log("<color=lime>[Controller]</color> 容量を1増やしました。");
        }
        else
        {
            Debug.LogWarning("<color=orange>[Controller]</color> 容量を増やすための条件を満たしていません。");
        }
    }
    /// <summary>
    /// 容量を増やせる状態（コスト支払いなど）にあるかチェックします。
    /// 具体的なロジックは今後実装。
    /// </summary>
    private bool CanIncreaseCapacity()
    {
        // 今後はここで所持金マネージャー等を参照する
        // 現時点ではインスペクタから設定したテスト用フラグを返す
        return test_canAffordUpgrade;
    }



    // --- デバッグ・テスト用機能 ---
    [Header("--- Capacity Upgrade Settings ---")]
    [Tooltip("テスト用：容量増加の条件（所持金など）を満たしているか")]
    [SerializeField] private bool test_canAffordUpgrade = true;

    [System.Serializable]
    public struct TestHoldStatusData
    {
        public SpellType type;
        public bool isUnlocked;
        public int totalCount;
        public int equippedCount;
    }

    [Header("--- Test Data Persistence ---")]
    [Tooltip("このリストの内容をSpellHoldInfoManagerに強制適用します")]
    [SerializeField] private List<TestHoldStatusData> test_manualHoldStatuses = new List<TestHoldStatusData>();

    /// <summary>
    /// インスペクタで設定した test_manualHoldStatuses をセーブデータ(Manager)に反映し、
    /// UIを強制リロードします。
    /// </summary>
    public void Test_ApplyAndSaveManualStatuses()
    {
        if (SpellHoldInfoManager.Instance == null) return;

        Debug.Log("<color=orange>[Test]</color> データの強制上書きを開始します...");

        foreach (var data in test_manualHoldStatuses)
        {
            // 現在の保持数を一度リセットするために、0になるまで減らす（または初期化メソッドが必要）
            // ここでは簡易的に現在の数を取得して差分を調整します
            int currentCount = SpellHoldInfoManager.Instance.GetSpellCount(data.type);

            // 保持数の調整
            if (data.totalCount > currentCount)
            {
                for (int i = 0; i < (data.totalCount - currentCount); i++)
                    SpellHoldInfoManager.Instance.IncreaseSpellCount(data.type);
            }
            else if (data.totalCount < currentCount)
            {
                for (int i = 0; i < (currentCount - data.totalCount); i++)
                    SpellHoldInfoManager.Instance.DecreaseSpellCount(data.type);
            }

            // アンロック状態の適用
            if (data.isUnlocked)
            {
                SpellHoldInfoManager.Instance.UnlockSpell(data.type);
            }
        }

        Debug.Log("<color=lime>[Test]</color> データの適用が完了しました。UIをリロードします。");
        // Model経由で最新のデータを取得し、UIに通知されるのを待ちます（Managerが変更されるとModelに通知が行くため自動で走ります）
    }

    /// <summary>
    /// 全てのセーブデータを削除して初期状態に戻します。
    /// </summary>
    public void Test_ResetAllData()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("<color=red>[Test]</color> PlayerPrefsを全削除しました。再起動が必要です。");
    }
}