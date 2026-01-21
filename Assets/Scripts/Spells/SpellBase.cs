using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using NUnit.Framework.Internal;
/// <summary>
/// 全ての具体的な呪文クラスの抽象基底クラス。
/// ScriptableObjectを継承することで、Unity Inspectorで設定可能なデータアセットとして扱えます。
/// </summary>
public abstract class SpellBase : ScriptableObject
{
    [Header("基本設定")]
    [Tooltip("呪文のカテゴリ（UIの色などに影響します）")]
    public SpellCategory category = SpellCategory.Attack;

    [Tooltip("呪文が発動した際に追加されるクールタイム（秒）")]
    public float cooldown = 0.5f;

    [Tooltip("この呪文のゲーム内での表示名")]
    public string spellName = "未定義の呪文";

    [Header("購入設定")]
    [Tooltip("保有数ごとの購入コスト。インデックスが保有数に対応します。")]
    public int[] purchaseCosts = new int[] { 100, 200, 400, 800 };

    /// <summary>
    /// 補助線（軌道予測）を表示するためのロジックを定義します。
    /// 処理内容: 発射角度と強さ、重力に基づいて軌道を計算し、プレハブを生成します。
    /// </summary>
    /// <param name="wandSpells">杖にセットされている全呪文の配列</param>
    /// <param name="currentSpellIndex">この呪文が杖の配列の何番目にあるか</param>
    /// <param name="rotationZ">発射角度（Z軸回転）</param>
    /// <param name="strength">発射の強さ。0~1の範囲</param>
    /// <param name="casterPosition">発射元となる位置</param>
    /// <param name="gravityMagnitude">重力の大きさ</param>
    public virtual void DisplayAimingLine(
        List<SpellBase> wandSpells,
        int currentSpellIndex,
        float rotationZ,
        float strength,
        Vector2 casterPosition,
        Action<GameObject> aimingModifier,
        bool clearLine = false
    )
    {
        // GetNextSpellOffsetsで得られた次の呪文に対して、同じ引数でDisplayAimingLineを呼び出す
        DisplayAimingLineForNextSpells(
            GetNextSpellOffsets(wandSpells, currentSpellIndex),
            wandSpells, currentSpellIndex, rotationZ, strength, casterPosition, aimingModifier, clearLine
        );
    }

    /// <summary>
    /// 渡された相対オフセット配列に基づいて、杖リスト内の対象の呪文のDisplayAimingLineを呼び出します。
    /// </summary>
    /// <param name="nextSpelloffsets">この呪文からの相対的なインデックス（オフセット）の配列</param>
    /// <param name="wandSpells">杖にセットされている全呪文の配列</param>
    /// <param name="currentSpellIndex">この呪文（呼び出し元）が杖の配列の何番目にあるか</param>
    /// <param name="rotationZ">発射角度</param>
    /// <param name="strength">発射の強さ</param>
    /// <param name="casterPosition">発射元となる位置</param>
    /// <param name="aimingModifier">補助線の描画時に適用する修飾子</param>
    /// <param name="clearLine">ラインをクリアするかどうか</param>
    protected void DisplayAimingLineForNextSpells(
        int[] nextSpelloffsets,
        List<SpellBase> wandSpells,
        int currentSpellIndex,
        float rotationZ,
        float strength,
        Vector2 casterPosition,
        Action<GameObject> aimingModifier,
        bool clearLine = false
    )
    {
        foreach (int offset in nextSpelloffsets)
        {
            // 相対オフセットを絶対インデックスに変換
            int targetIndex = currentSpellIndex + offset;

            // インデックスが杖リストの範囲内にあるかチェック
            if (targetIndex >= 0 && targetIndex < wandSpells.Count)
            {
                SpellBase spellToDisplay = wandSpells[targetIndex];

                // 対象の呪文のDisplayAimingLineを呼び出し
                spellToDisplay?.DisplayAimingLine(
                    wandSpells,
                    targetIndex,        // 新しい開始インデックス
                    rotationZ,
                    strength,
                    casterPosition,
                    aimingModifier,
                    clearLine         // 最初の呼び出しでのみクリアを実行
                );
            }
        }
    }

    /// <summary>
    /// 呪文の主要な効果を発射・実行するためのロジックを定義します。
    /// 処理内容: 例として、特定のプレハブを発射します。
    /// </summary>
    /// <param name="wandSpells">杖にセットされている全呪文の配列</param>
    /// <param name="currentSpellIndex">この呪文が杖の配列の何番目にあるか</param>
    /// <param name="rotationZ">発射角度（Z軸回転）</param>
    /// <param name="strength">発射の強さ</param>
    /// <param name="context">発射時の環境情報を持つインスタンス</param>
    public virtual void FireSpell(
        List<SpellBase> wandSpells,
        int currentSpellIndex,
        float rotationZ,
        float strength,
        SpellContext context
    )
    {
        // GetNextSpellOffsetsで得られた次の呪文に対して、同じ引数でFireSpellを呼び出す
        FireSpellForNextSpells(
            GetNextSpellOffsets(wandSpells, currentSpellIndex),
            wandSpells, currentSpellIndex, rotationZ, strength, context
        );
    }


    /// <summary>
    /// 渡された相対オフセット配列に基づいて、杖リスト内の対象の呪文のFireSpellを呼び出します。
    /// </summary>
    /// <param name="nextSpelloffsets">この呪文からの相対的なインデックス（オフセット）の配列</param>
    /// <param name="wandSpells">杖にセットされている全呪文の配列</param>
    /// <param name="currentSpellIndex">この呪文が杖の配列の何番目にあるか</param>
    /// <param name="rotationZ">発射角度（Z軸回転）</param>
    /// <param name="strength">発射の強さ</param>
    /// <param name="context">発射時の環境情報を持つインスタンス</param>
    protected void FireSpellForNextSpells(
        int[] nextSpelloffsets,
        List<SpellBase> wandSpells,
        int currentSpellIndex,
        float rotationZ,
        float strength,
        SpellContext context
    )
    {
        foreach (int offset in nextSpelloffsets)
        {
            // 相対オフセットを絶対インデックスに変換
            int targetIndex = currentSpellIndex + offset;

            // インデックスが杖リストの範囲内にあるかチェック
            if (targetIndex >= 0 && targetIndex < wandSpells.Count)
            {
                SpellBase spellToDisplay = wandSpells[targetIndex];

                // 対象の呪文のDisplayAimingLineを呼び出し
                spellToDisplay?.FireSpell(
                    wandSpells,
                    targetIndex,        // 新しい開始インデックス
                    rotationZ,
                    strength,
                    context
                );
            }
        }
    }

    public void ModifyProjectile(SpellContext context, GameObject projectile)
    {
        context.ProjectileModifier?.Invoke(projectile);
    }

    public virtual int[] GetNextSpellOffsets(List<SpellBase> wandSpells,
        int currentSpellIndex)
    {
        return new int[0];
    }

    // ----------------------------------------------------------------------------------
    // 軌道計算のヘルパー（具体的な実装例として）
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// 指定されたパラメータに基づき、特定の時間における発射体の予測位置を計算します。
    /// </summary>
    /// <param name="initialPosition">初期位置</param>
    /// <param name="initialVelocity">初速ベクトル</param>
    /// <param name="gravity">重力の大きさ</param>
    /// <param name="time">発射後の時間</param>
    /// <returns>予測位置</returns>
    protected Vector2 CalculateTrajectoryPoint(
        Vector2 initialPosition,
        Vector2 initialVelocity,
        float gravity,
        float time)
    {
        // 運動の公式: P(t) = P0 + V0*t + 1/2 * G * t^2
        Vector2 position = initialPosition
                           + initialVelocity * time
                           + (Vector2.down * gravity * time * time * 0.5f);

        return position;
    }

    // Mathf.Deg2Radの代わりに標準偏差をラジアンに変換して使用する場合は、このメソッドで
    // ラジアンの標準偏差を計算します。
    // 例: error_degree=5 (度) の場合、standardDeviation = 5 * Mathf.Deg2Rad を使用します。
    public float GetGaussianRandom(float standardDeviation)
    {
        standardDeviation = Mathf.Max(0f, standardDeviation);
        // Box-Muller変換を使って正規分布に従う乱数（平均0、標準偏差1）を生成
        // R.NextDouble()は [0.0, 1.0) の乱数
        float u1 = 1f - UnityEngine.Random.value; // (0, 1]
        float u2 = 1f - UnityEngine.Random.value; // (0, 1]

        // 標準正規分布に従う乱数 (Z)
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);

        // 標準偏差 standardDeviation を乗算して、目的の正規分布に従う乱数 (X) を得る
        // 平均は0
        float randGaussian = randStdNormal * standardDeviation;

        return randGaussian;
    }


    /// <summary>
    /// 呪文のまとまりの相対的なオフセット配列に基づいて、実際に呼び出すべき呪文の絶対インデックス配列を計算します。
    /// </summary>
    /// <param name="wandSpells">杖にセットされている全呪文の配列</param>
    /// <param name="currentSpellIndex">この呪文が杖の配列の何番目にあるか</param>
    /// <param name="relativeGroupOffsets">この呪文からの相対的なまとまりインデックスの配列</param>
    /// <param name="allSpell">relativeGroupOffsetsを無視して、すべての呪文のまとまりの最初の要素を返すかどうか</param>
    /// <returns>呼び出すべき呪文の絶対インデックスの配列</returns>
    public static int[] GetAbsoluteIndicesFromSpellGroupArray(
        List<SpellBase> wandSpells,
        int currentSpellIndex,
        int[] relativeGroupOffsets, bool allSpell = false)
    {
        // オフセットを昇順にソートし、重複を排除（連鎖の計算をシンプルにするため）
        relativeGroupOffsets = relativeGroupOffsets.Distinct().OrderBy(o => o).ToArray();

        bool[] fired = new bool[wandSpells.Count];
        for (int i = 0; i <= fired.Length - 1; i++)
        {
            if (i <= currentSpellIndex || wandSpells[i] == null)
                fired[i] = true;
        }
        int maxOffset = allSpell ? int.MaxValue : relativeGroupOffsets[^1];
        int targetOffsetIndex = 0; // 現在探してる相対オフセットのインデックス
        var targetIndices = new List<int>();

        for (int i = 1; i <= maxOffset; i++)
        {
            // まだ呼び出されてない最初の呪文のインデックスを求める配列に格納。
            int start = Array.IndexOf(fired, false);
            if (start < 0) break;

            if (allSpell)
            {
                targetIndices.Add(start);
                targetOffsetIndex++;
            }
            else if (relativeGroupOffsets[targetOffsetIndex] == i)
            {
                targetIndices.Add(start);
                targetOffsetIndex++;
            }

            // インデックスがstartのものから呼び出される呪文を深さ優先探索して記録。
            var stack = new Stack<int>();
            stack.Push(start);
            while (stack.Count > 0)
            {
                int v = stack.Pop();
                if (v < 0 || fired.Length <= v || fired[v]) continue;
                fired[v] = true;

                // スタックはLIFOなので、隣接リストを逆順で積むと
                // 元の隣接リストの順序通りに巡ることができる
                var neighborsRelative = wandSpells[v].GetNextSpellOffsets(wandSpells, v);

                for (int j = neighborsRelative.Length - 1; j >= 0; j--)
                {
                    int to = v + neighborsRelative[j];
                    if (!(to < 0 || fired.Length <= to || fired[to])) stack.Push(to);
                }
            }
        }
        return targetIndices.ToArray();
    }

    /// <summary>
    /// 呪文の発射前に行う配列の前処理。
    /// 特定の呪文（例：〇倍呪文）はここで配列を編集する。
    /// </summary>
    /// <param name="wandSpells">杖に格納されている呪文のオリジナルの配列。</param>
    /// <param name="currentSpellIndex">現在処理中の呪文が杖の配列内で何番目かを示すインデックス。</param>
    /// <returns>次の呪文のインデックス。リスト編集後インデックスが変わってる場合があるため。</returns>
    public virtual int Preprocess(List<SpellBase> wandSpells, int currentSpellIndex)
    {
        return currentSpellIndex + 1;
    }


    [Header("UI")]
    public Sprite icon;

    /// <summary>
    /// このSpellBaseに対応するSpellUIインスタンスを生成する。
    /// </summary>
    public virtual SpellUI CreateUI()
    {
        // プレハブから生成
        SpellUI uiInstance = Instantiate(SpellCommonData.Instance.spellUIPrefab).GetComponent<SpellUI>();
        // アイコンと色を設定
        uiInstance.SetData(this);

        return uiInstance;
    }

    /// <summary>
    /// このSpellBaseに対応するEquippedSpellIconUIインスタンスを生成します。
    /// </summary>
    public virtual EquippedSpellIconUI CreateEquippedIconUI()
    {
        if (SpellCommonData.Instance.equippedSpellIconUIPrefab == null)
        {
            Debug.LogError($"SpellCommonData.Instance.equippedSpellIconUIPrefab is null.");
            return null;
        }

        // プレハブから生成
        // EquippedSpellIconUIがアタッチされているプレハブを SpellCommonData が保持している前提
        EquippedSpellIconUI uiInstance = Instantiate(SpellCommonData.Instance.equippedSpellIconUIPrefab).GetComponent<EquippedSpellIconUI>();

        if (uiInstance == null)
        {
            Debug.LogError($"Instantiated prefab from equippedSpellIconUIPrefab does not contain EquippedSpellIconUI component.");
            return null;
        }

        // 呪文データと色を設定
        uiInstance.SetData(this);

        return uiInstance;
    }

    /// <summary>
    /// 呪文がワールドからドロップしてインベントリに回収される際のアニメーションに使用するUIオブジェクトを生成します。
    /// </summary>
    /// <returns>生成されたUIオブジェクトのGameObject。</returns>
    public virtual GameObject CreateDropUI()
    {
        if (SpellCommonData.Instance.dropUIPrefab == null)
        {
            Debug.LogError($"SpellCommonData.Instance.dropUIPrefab is null");
            return null;
        }
        // UIオブジェクトを生成
        GameObject dropUIInstance = Instantiate(SpellCommonData.Instance.dropUIPrefab);
        if (dropUIInstance.TryGetComponent<SpellDropUI>(out var spellDropUI))
        {
            // データと色を設定
            spellDropUI.SetData(this);
        }
        return dropUIInstance;
    }

    [SerializeField]
    private AudioClip spellDropSound; // ドラッグ開始時に再生するAudioClip
    [SerializeField] float spellDropSoundVolume = 1.0f;
    public void GetDropSound(out AudioClip clip, out float volume)
    {
        clip = spellDropSound;
        volume = spellDropSoundVolume;
    }

    [Tooltip("呪文の説明文")]
    [TextArea(1, 3)]
    [SerializeField] string spellDescription = "";

    public string GetDescription()
    {
        return spellDescription;
    }
    protected List<SpellDescriptionItem> detailItems = new();
    /// <summary>
    /// 呪文の詳細説明パネルに表示するための項目リストを取得します。
    /// 具体的な呪文クラスはこれをオーバーライドして動的な情報を追加できます。
    /// </summary>
    /// <returns>SpellDescriptionItemのリスト</returns>
    public virtual List<SpellDescriptionItem> GetDescriptionDetails()
    {
        detailItems.Clear();

        // クールタイム項目を動的に生成
        detailItems.Add(new SpellDescriptionItem
        {
            icon = SpellCommonData.Instance.coolDownIcon,
            descriptionText = $"クールタイム : {cooldown:F1} 秒"
        });

        return detailItems;
    }
}

/// <summary>
/// 呪文の発射・実行時に、環境や発射元の情報などを伝達するためのクラス。
/// </summary>
public class SpellContext
{
    public Vector2 CasterPosition;
    public Action<GameObject> ProjectileModifier;
    public float errorDegree = 0;
    public Damage damage;

    public SpellContext()
    {

    }

    /// <summary>
    /// このコンテキストの値をコピーした新しいインスタンスを返す。
    /// </summary>
    /// <returns>値が同じ新しい SpellContext インスタンス。</returns>
    public SpellContext Clone()
    {
        return new SpellContext
        {
            // 値型 (Vector2, float) は値そのものがコピーされる
            CasterPosition = this.CasterPosition,
            errorDegree = this.errorDegree,

            // 参照型 (Action) は参照がコピーされるが、Actionは不変(イミュータブル)なので問題なし
            ProjectileModifier = this.ProjectileModifier,
            damage = this.damage
        };
    }
}