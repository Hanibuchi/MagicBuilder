using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// プレイヤーの所持する全ての杖を管理するコントローラー。
/// 新しい杖の生成、UIの関連付けを行います。
/// WandControllerの配列を保持します。
/// </summary>
public class WandsController : MonoBehaviour
{
    [Header("依存コンポーネント")]
    [Tooltip("AttackManagerインスタンスへの参照")]
    // AttackManager.Instanceを使ってアクセスするため、設定は任意とします。
    AttackManager attackManager => AttackManager.Instance;

    [Header("UIとコンポーネントのプレハブ")]
    [Tooltip("WandUIとWandControllerを保持するオブジェクトのプレハブ")]
    [SerializeField] private GameObject wandUIPrefab;

    [Tooltip("WandUIを配置する親のTransform")]
    [SerializeField] private Transform wandUIParent;

    // 個々のWandControllerのリスト（配列の要件を満たす）
    private List<WandController> wandControllers = new List<WandController>();
    public WandController[] WandControllers => wandControllers.ToArray();

    /// <summary>
    /// 新しい杖（Wand）を生成し、AttackManagerに追加します。
    /// また、対応するWandUIとWandControllerも生成・初期化します。
    /// </summary>
    public void GenerateNewWand(Wand newWand)
    {
        GameObject container = Instantiate(wandUIPrefab, wandUIParent);

        WandUI wandUI = container.GetComponentInChildren<WandUI>();
        WandController wandController = new();

        if (newWand == null || wandController == null || wandUI == null)
        {
            Debug.LogError("wandContainerPrefabにWandControllerまたはWandUIコンポーネントが見つかりません。コンテナオブジェクトを破棄します。");
            // リストからWandを削除して処理を中止
            Destroy(container);
            return;
        }
        int newWandIndex = attackManager.playerWands.Count;

        // 5. WandControllerの初期化と関連付け
        wandController.Initialize(newWand, wandUI);
        attackManager.playerWands.Add(newWand);
        wandControllers.Add(wandController);

        Debug.Log($"新しい杖が生成されました。インデックス: {newWandIndex} | タイプ: {newWand.type}");
    }

    [SerializeField] Wand test_wand;
    public void Test()
    {
        GenerateNewWand(test_wand);
    }
}