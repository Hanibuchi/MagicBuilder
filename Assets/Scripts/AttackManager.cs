using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 攻撃の発射と杖のリスト管理を行うシングルトンクラス。
/// ターン制の戦闘において、プレイヤーの発射操作を処理します。
/// </summary>
public class AttackManager : MonoBehaviour
{
    // --- シングルトンインスタンス ---
    private static AttackManager instance;
    public static AttackManager Instance => instance;

    // --- プロパティ ---

    [Header("杖の管理")]
    [Tooltip("プレイヤーが現在所持している杖の配列")]
    public List<Wand> playerWands = new List<Wand>();
    public Transform casterPositionTransform;

    private void Awake()
    {
        // シングルトンの初期設定
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    [SerializeField] int displayAimingLineSpellCount = 5;
    /// <summary>
    /// 選択された杖の最初の呪文を参照し、補助線を表示するメソッドを実行します。
    /// </summary>
    /// <param name="wandIndex">照準情報を取得する対象の杖のインデックス（番号）</param>
    /// <param name="rotationZ">発射角度 (Z軸回転、float)。例: 0°は右、90°は上。</param>
    /// <param name="strength">発射の強さ（大きさ、float）。</param>
    public void DisplayAimingLine(int wandIndex, float rotationZ, float strength, bool clearLine = false)
    {
        if (wandIndex < 0 || wandIndex >= playerWands.Count)
        {
            Debug.LogError($"不正な杖インデックスが指定されました: {wandIndex}");
            return;
        }

        Wand wandToUse = playerWands[wandIndex];

        if (wandToUse == null || wandToUse.spells.Count == 0)
        {
            Debug.LogWarning($"杖 (Index: {wandIndex}) がnullか、呪文がセットされていません。");
            return;
        }

        int[] arr = new int[displayAimingLineSpellCount];
        for (int i = 0; i < displayAimingLineSpellCount; i++)
            arr[i] = i + 1;
        // GetAbsoluteIndicesFromSpellGroupArray を使用して、呪文の連鎖全体で次に発動すべき呪文を特定
        // 最初の呪文は「1つ目の呪文」として処理するため、relativeGroupOffsets は [1] を渡す
        int[] targetIndices = SpellBase.GetAbsoluteIndicesFromSpellGroupArray(
            wandToUse.spells,
            -1,
            arr // 最初の呪文グループ（すなわち最初の呪文）の次のインデックスを取得
        );

        Debug.Log($"照準補助線を表示中: 杖 Index={wandIndex}, 角度={rotationZ:F2}° (Z回転), 強さ={strength:F2}");

        // 取得した全ての開始インデックスの呪文に対して DisplayAimingLine を呼び出す
        foreach (int targetIndex in targetIndices)
        {
            if (targetIndex >= 0 && targetIndex < wandToUse.spells.Count)
            {
                SpellBase spell = wandToUse.spells[targetIndex];
                spell?.DisplayAimingLine(
                    wandToUse.spells,
                    targetIndex,
                    rotationZ,
                    strength,
                    casterPositionTransform.position,
                    clearLine // clearLineは最初の呼び出しでのみ意味を持つが、ここでは全ての開始点で実行させる
                );
            }
        }
        // --- 修正終了 ---

        // 補助線表示のロジックは、このrotationZとstrengthを使って軌道を予測し、UI描画を行います。
    }

    /// <summary>
    /// 指定された杖の発射をトリガーします。
    /// </summary>
    /// <param name="wandIndex">発射する対象の杖のインデックス（番号）</param>
    /// <param name="rotationZ">発射角度 (Z軸回転、float)。</param>
    /// <param name="strength">発射の強さ（大きさ、float）。</param>
    public void FireWand(int wandIndex, float rotationZ, float strength)
    {
        if (wandIndex < 0 || wandIndex >= playerWands.Count)
        {
            Debug.LogError($"不正な杖インデックスが指定されました: {wandIndex}");
            return;
        }

        Wand wandToUse = playerWands[wandIndex];

        int[] targetIndices = SpellBase.GetAbsoluteIndicesFromSpellGroupArray(
            wandToUse.spells,
            -1,
            new int[] { }, // 最初の呪文グループ（すなわち最初の呪文）の次のインデックスを取得
            true
        );

        Debug.Log($"杖を発射: Index={wandIndex} | 角度={rotationZ:F2}° | 強さ={strength:F2}");

        // 取得した全ての開始インデックスの呪文に対して FireSpell を呼び出す
        foreach (int targetIndex in targetIndices)
        {
            if (targetIndex >= 0 && targetIndex < wandToUse.spells.Count)
            {
                SpellContext context = new()
                {
                    CasterPosition = casterPositionTransform.position
                };
                SpellBase spell = wandToUse.spells[targetIndex];
                spell?.FireSpell(
                    wandToUse.spells,
                    targetIndex,
                    rotationZ,
                    strength,
                    context
                );
            }
        }
        // --- 修正終了 ---
    }
}