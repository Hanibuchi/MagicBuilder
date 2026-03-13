using UnityEngine;
using System.Collections.Generic;
using System.Linq; // List<T>.InsertRangeのために必要

/// <summary>
/// 次の呪文を指定された回数だけ複製する（倍増させる）呪文。
/// Preprocessフェーズで呪文配列を編集します。
/// </summary>
[CreateAssetMenu(fileName = "MultiplierSpell", menuName = "Wand System/Multiplier Spell")]
public class MultiplierSpell : SpellBase
{
    [Header("倍増設定")]
    [Tooltip("次の呪文を何回複製するか（例: 2を指定すると2回発動になる）")]
    [SerializeField]
    private int multiplierCount = 2;

    // ----------------------------------------------------------------------------------
    // Preprocess: 呪文配列を編集
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// 呪文の発射前に行う配列の前処理。
    /// 自身の位置の次の呪文を、指定された回数分だけ複製して配列に挿入します。
    /// </summary>
    /// <param name="wandSpells">杖に格納されている呪文のオリジナルの配列。</param>
    /// <param name="currentSpellIndex">現在処理中の呪文が杖の配列内で何番目かを示すインデックス。</param>
    /// <returns>次の呪文のインデックス。リスト編集後インデックスが変わってる場合があるため。</returns>
    public override int Preprocess(List<SpellBase> wandSpells, int currentSpellIndex)
    {
        // 複製回数が0以下の場合、何もしない
        if (multiplierCount <= 1) return currentSpellIndex + 1;

        // 複製対象となる呪文のインデックスは、自身の次の位置
        int targetIndex = currentSpellIndex + 1;

        // 次の呪文が存在するかチェック
        if (targetIndex >= 0 && targetIndex < wandSpells.Count)
        {
            SpellBase spellToCopy = wandSpells[targetIndex];
            if (spellToCopy == null) return currentSpellIndex + 1;

            // 複製する呪文のリストを作成（複製回数分）
            var copies = new List<SpellBase>();
            for (int i = 1; i < multiplierCount; i++)
            {
                copies.Add(spellToCopy);
            }

            // 複製された呪文のリストを、元の呪文（targetIndex）の直後に挿入
            // 挿入開始位置は targetIndex + 1
            wandSpells.InsertRange(targetIndex + 1, copies);
        }
        return currentSpellIndex + multiplierCount;
    }

    /// <summary>
    /// 呪文の発射前に行うリスナー配列の前処理。
    /// 自身の位置の次のリスナーを、指定された回数分だけ複製して配列に挿入します。
    /// </summary>
    /// <param name="listeners">呪文のリスナー配列。</param>
    /// <param name="currentSpellIndex">現在処理中の呪文が配列内で何番目かを示すインデックス。</param>
    /// <returns>次の呪文のインデックス。リスト編集後インデックスが変わってる場合があるため。</returns>
    public override int Preprocess(List<ISpellCastListener> listeners, int currentSpellIndex)
    {
        // 複製回数が0以下の場合、何もしない
        if (multiplierCount <= 1) return currentSpellIndex + 1;

        // 複製対象となるリスナーのインデックスは、自身の次の位置
        int targetIndex = currentSpellIndex + 1;

        // 次のリスナーが存在するかチェック
        if (targetIndex >= 0 && targetIndex < listeners.Count)
        {
            ISpellCastListener listenerToCopy = listeners[targetIndex];
            if (listenerToCopy == null) return currentSpellIndex + 1;

            // 複製するリスナーのリストを作成（複製回数分）
            var copies = new List<ISpellCastListener>();
            for (int i = 1; i < multiplierCount; i++)
            {
                copies.Add(listenerToCopy);
            }

            // 複製されたリスナーのリストを、元のリスナー（targetIndex）の直後に挿入
            // 挿入開始位置は targetIndex + 1
            listeners.InsertRange(targetIndex + 1, copies);
        }
        return currentSpellIndex + multiplierCount;
    }


    // ----------------------------------------------------------------------------------
    // 発動チェーン
    // ----------------------------------------------------------------------------------
    readonly int[] nextSpellOffsets = { 1 };

    /// <summary>
    /// 次の呪文の相対オフセットを取得します。
    /// MultiplierSpellが実行された後、発動すべきなのは自身の次の呪文（オフセット+1）です。
    /// Preprocessによって呪文が追加された場合でも、元の杖における次の呪文（と複製された呪文全て）
    /// が、新しいインデックスで +1 の位置に来るため、オフセットは { 1 } でOKです。
    /// </summary>
    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells, int currentSpellIndex)
    {
        return nextSpellOffsets;
    }
}