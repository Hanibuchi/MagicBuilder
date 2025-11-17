using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq; // SpellBase.GetGaussianRandom を使うために追加

public class ClickTriggerSpell : MonoBehaviour
{
    // 標準偏差の計算に使用する定数
    // 例として 0.05f を設定。context.errorDegree * 0.05f が標準偏差（秒）になる。
    const float DELAY_MULTIPLIER = 0.01f;

    /// <summary>
    /// ClickTriggerが発動したときに実行される、次の呪文を処理するアクションを作成します。
    /// </summary>
    /// <param name="wandSpells">杖にセットされている全ての呪文リスト。</param>
    /// <param name="currentSpellIndex">現在発射された呪文のインデックス。</param>
    /// <param name="context">ClickTriggerに影響を与えるかもしれないコンテキスト情報。</param>
    /// <returns>投射物GameObjectを受け取り、次の呪文のClickTriggerProjectileModifierを追加するアクション。</returns>
    public static Action<GameObject> CreateClickTriggerAction(
        List<SpellBase> wandSpells,
        int currentSpellIndex, SpellContext context)
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
                // ----------------------------------------------------
                // ⚡ 遅延時間の計算ロジックを追加 ⚡
                // ----------------------------------------------------
                float delayTime = context.errorDegree * DELAY_MULTIPLIER;
                // SpellBase のヘルパーメソッドを利用して正規分布に従うランダムな遅延時間を生成

                // 遅延時間は負にならないように0でクランプ（Clamp）する
                delayTime = Mathf.Abs(delayTime);
                // ----------------------------------------------------

                obj.AddComponent<ClickTriggerProjectileModifier>().Init(
                    nextSpell,
                    wandSpells,
                    nextSpellIndex,
                    new(),
                    delayTime // 算出した遅延時間を渡す (Initのシグネチャ変更が必要)
                );
            }
        };
    }
}