using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// プレイヤーの所持する全ての杖を管理するコントローラー。
/// 新しい杖の生成、UIの関連付けを行います。
/// WandControllerの配列を保持します。
/// </summary>
public class WandsController : MonoBehaviour, WandSwitchListener
{
    public static WandsController Instance { get; private set; }
    [Header("依存コンポーネント")]
    [Tooltip("AttackManagerインスタンスへの参照")]
    // AttackManager.Instanceを使ってアクセスするため、設定は任意とします。
    AttackManager attackManager => AttackManager.Instance;

    // 個々のWandControllerのリスト（配列の要件を満たす）
    private List<WandController> wandControllers = new List<WandController>();
    public WandController[] WandControllers => wandControllers.ToArray();

    /// <summary>
    /// 新しい杖（Wand）を生成し、AttackManagerに追加します。
    /// また、対応するWandUIとWandControllerも生成・初期化します。
    /// </summary>
    public void GenerateNewWand(Wand newWand)
    {
        if (newWand == null)
        {
            Debug.LogError("newWandが見つかりません。");
            return;
        }

        WandUI wandUI = WandUIManager.Instance.CreateWandUIInstance();

        WandController wandController = new();

        if (wandController == null || wandUI == null)
        {
            Debug.LogError("wandContainerPrefabにWandControllerまたはWandUIコンポーネントが見つかりません。コンテナオブジェクトを破棄します。");
            return;
        }
        int newWandIndex = attackManager.playerWands.Count;

        // 5. WandControllerの初期化と関連付け
        wandController.Initialize(newWand, wandUI);
        attackManager.playerWands.Add(newWand);
        wandControllers.Add(wandController);

        Debug.Log($"新しい杖が生成されました。インデックス: {newWandIndex} | タイプ: {newWand.type}");
    }

    private void Awake()
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

    void Start()
    {
        WandUIManager.Instance.SetWandSwitchListener(this);
    }
    /// <summary>
    /// WandSwitchListenerインターフェースの実装。
    /// 実際の切り替え処理はAttackManagerに委譲する。
    /// </summary>
    /// <param name="index">次の杖のindex</param>
    public void SwitchWand(int index)
    {
        AttackManager.Instance.SetCurrentWandIndex(index);
        WandUIManager.Instance.SetActiveWandUI(index);
        var wand = AttackManager.Instance.GetCurrentWand();
        if (wand == null)
        {
            Debug.LogError("現在の杖が見つかりません。");
            return;
        }
        SpellInventory.Instance.DeactivateSpellUIs(wand.GetSpells());
        WandAppearanceManager.Instance.ChangeAppearance(wand.type);
    }

    [SerializeField] List<Wand> test_wand;
    int test_nextWandIndex = 0;
    public void Test()
    {
        GenerateNewWand(test_wand[test_nextWandIndex]);
        test_nextWandIndex++;
        if (test_nextWandIndex == 1)
            SwitchWand(0);
    }
}