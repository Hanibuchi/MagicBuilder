using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq; // SpellBase.GetGaussianRandom を使うために追加

public class ClickTriggerSpell : MonoBehaviour
{
    /// <summary>
    /// ClickTriggerが発動したときに実行される、次の呪文を処理するアクションを作成します。
    /// </summary>
    /// <param name="wandSpells">杖にセットされている全ての呪文リスト。</param>
    /// <param name="currentSpellIndex">現在発射された呪文のインデックス。</param>
    /// <param name="context">ClickTriggerに影響を与えるかもしれないコンテキスト情報。</param>
    /// <param name="magicCircleDelay">魔法陣を表示してから発射されるまでの待ち時間。</param>
    /// <returns>投射物GameObjectを受け取り、次の呪文のClickTriggerProjectileModifierを追加するアクション。</returns>
    public static Action<GameObject> CreateClickTriggerAction(
        List<SpellBase> wandSpells,
        int currentSpellIndex, SpellContext context,
        float magicCircleDelay = 0.5f)
    {
        return (GameObject obj) =>
        {
            if (obj == null)
            {
                return; // オブジェクトがnullなら何もしない
            }

            SpellBase nextSpell = null;
            int nextSpellIndex = -1; // 次の呪文のインデックスを保持
            int index = currentSpellIndex + 1;

            // 杖のリストを順にチェックし、nullでない次の呪文を見つける
            while (index < wandSpells.Count)
            {
                if (wandSpells[index] != null)
                {
                    nextSpell = wandSpells[index];
                    nextSpellIndex = index;
                    break;
                }
                index++;
            }

            // 次の呪文が見つかった場合、その呪文を発動させるためのコンポーネントを追加
            if (nextSpell != null)
            {
                obj.AddComponent<ClickTriggerProjectileModifier>().Init(
                    nextSpell,
                    wandSpells,
                    nextSpellIndex,
                    new(),
                    magicCircleDelay
                );
            }
        };
    }
}