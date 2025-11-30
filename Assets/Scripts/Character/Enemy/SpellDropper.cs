using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// 敵が倒されたときに呪文をドロップさせる機能を管理するクラス。
/// </summary>
public class SpellDropper : MonoBehaviour
{
    [Header("ドロップ設定")]
    [Tooltip("落とす呪文のリストとそれぞれの確率")]
    public DroppableSpell[] droppableSpells;

    [Header("ドロップ演出設定")]
    [Tooltip("複数の呪文をドロップさせる際の時間差（秒）")]
    [SerializeField] private float dropDelay = 0.2f;

    /// <summary>
    /// 設定された確率に基づいて呪文をドロップします。
    /// このメソッドは、敵が倒されたときなどに呼び出されます。
    /// </summary>
    public void DropSpells()
    {
        // 2. ドロップする呪文の抽選
        // 全体のドロップ確率に達するまで呪文を抽選
        SpellBase[] spellsToDrop = SelectSpellsToDrop();

        if (spellsToDrop.Length == 0)
        {
            Debug.Log("SpellDropper: 抽選の結果、ドロップする呪文はありませんでした。");
            return;
        }

        // 3. 呪文ドロップのアニメーションとインベントリ追加を開始
        StartCoroutine(ExecuteDropsWithDelay(spellsToDrop));
    }

    /// <summary>
    /// ドロップリストから、確率に基づいてドロップする呪文の配列を選択します。
    /// </summary>
    /// <returns>ドロップするSpellBaseの配列</returns>
    private SpellBase[] SelectSpellsToDrop()
    {
        var selectedSpells = new List<SpellBase>();

        // 合計確率の計算 (確率の合計が1を超えることを許容し、それぞれ独立に抽選)
        foreach (var droppable in droppableSpells)
        {
            if (droppable.spellData != null && Random.value <= droppable.dropChance)
            {
                selectedSpells.Add(droppable.spellData);
            }
        }

        return selectedSpells.ToArray();
    }

    /// <summary>
    /// 選択された呪文を、時間差を付けて順番にドロップ処理するコルーチン。
    /// </summary>
    /// <param name="spells">ドロップする呪文の配列</param>
    private IEnumerator ExecuteDropsWithDelay(SpellBase[] spells)
    {
        Vector3 dropPosition = transform.position; // アタッチされたGameObjectの位置を使用

        for (int i = 0; i < spells.Length; i++)
        {
            yield return new WaitForSeconds(dropDelay);

            SpellBase spell = spells[i];

            if (SpellDropManager.Instance != null)
            {
                // SpellDropManagerにドロップ処理を委譲
                SpellDropManager.Instance.DropSpell(dropPosition, spell);
                Debug.Log($"SpellDropper: 呪文 '{spell.spellName}' をドロップしました。");
            }
            else
            {
                Debug.LogError("SpellDropManagerがシーンに見つかりません。ドロップ処理を実行できません。");
                break;
            }

        }
    }
}


/// <summary>
/// 敵が落とす呪文のデータ（呪文とそのドロップ確率）を保持する構造体。
/// Inspectorで設定できるように[System.Serializable]を付与。
/// </summary>
[System.Serializable]
public class DroppableSpell
{
    [Tooltip("落とす可能性のある呪文データ")]
    public SpellBase spellData;

    [Range(0f, 1f)]
    [Tooltip("この呪文が選ばれる確率 (0.0から1.0)")]
    public float dropChance = 1.0f;
}