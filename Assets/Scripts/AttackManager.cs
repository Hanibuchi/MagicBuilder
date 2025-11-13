using UnityEngine;
using System.Collections.Generic;

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

    // --- 攻撃準備フェーズのメソッド ---
/// <summary>
    /// 選択された杖の最初の呪文を参照し、補助線を表示するメソッドを実行します。
    /// </summary>
    /// <param name="wandIndex">照準情報を取得する対象の杖のインデックス（番号）</param>
    /// <param name="rotationZ">発射角度 (Z軸回転、float)。例: 0°は右、90°は上。</param>
    /// <param name="strength">発射の強さ（大きさ、float）。</param>
    public void DisplayAimingLine(int wandIndex, float rotationZ, float strength)
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

        // 選択したWandの持つ最初のSpellを参照
        SpellBase firstSpell = wandToUse.spells[0];
        
        // **Z回転と強さから発射方向のベクトルを計算する例:**
        // float angleRad = rotationZ * Mathf.Deg2Rad;
        // Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        // Vector2 velocity = direction * strength;
        
        Debug.Log($"照準補助線を表示中: 杖 Index={wandIndex}, 角度={rotationZ:F2}° (Z回転), 強さ={strength:F2}");
        
        // 補助線表示のロジックは、このrotationZとstrengthを使って軌道を予測し、UI描画を行います。
    }
// --- 攻撃実行フェーズのメソッド ---
    
    // FireWandメソッドも、同様にインデックスと角度・強さを受け取るように修正しておきます。
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
        
        Debug.Log($"杖を発射: Index={wandIndex} | 角度={rotationZ:F2}° | 強さ={strength:F2}");
        
        // **発射ロジックの実行**
        // このロジック内で、rotationZとstrengthから最終的な初速ベクトルを計算し、
        // 杖の呪文リストを処理して魔法弾を生成することになります。
        
        // (例: SpellExecutioner.Execute(wandToUse, rotationZ, strength);)
    }
}