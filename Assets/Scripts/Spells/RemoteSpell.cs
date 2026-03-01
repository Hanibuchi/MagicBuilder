using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[CreateAssetMenu(fileName = "RemoteSpell", menuName = "Wand System/RemoteSpell")]
public class RemoteSpell : SpellBase
{
    [SerializeField] float distance = 2f;
    [SerializeField] float magicCircleDelay = 0.3f;

    public override void FireSpell(
        List<SpellBase> wandSpells, int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        // 発射方向（rotationZ）に向かって一定距離離れた位置を新しいCasterPositionにする
        float angleRad = rotationZ * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * distance;
        context.CasterPosition += offset;

        // context.ProjectileModifier にキャラクター移動処理を追加
        context.ProjectileModifier += (projectile) =>
        {
            // キャラクターコントローラーを持っている場合は、座標をオフセット分移動させる
            if (projectile.GetComponent<MyCharacterController>() != null)
            {
                projectile.transform.position += (Vector3)offset;
            }
        };

        if (SpellScheduler.Instance != null && magicCircleDelay > 0)
        {
            SpellScheduler.Instance.StartCoroutine(FireNextSpellsDelayed(
                wandSpells, currentSpellIndex, rotationZ, strength, context));
        }
        else
        {
            FireNextSpells(wandSpells, currentSpellIndex, rotationZ, strength, context);
        }
    }

    private void FireNextSpells(List<SpellBase> wandSpells, int currentSpellIndex, float rotationZ, float strength, SpellContext context)
    {
        FireSpellForNextSpells(GetNextSpellOffsets(wandSpells, currentSpellIndex), wandSpells, currentSpellIndex, rotationZ, strength, context);
    }

    private IEnumerator FireNextSpellsDelayed(
        List<SpellBase> wandSpells, int currentSpellIndex,
        float rotationZ, float strength, SpellContext context)
    {
        MagicCircle magicCircle = null;
        GameObject prefab = SpellCommonData.Instance?.magicCirclePrefab;

        // 💡 魔法陣の表示演出を追加
        if (prefab != null)
        {
            GameObject circleGo = Instantiate(prefab, context.CasterPosition, Quaternion.Euler(0, 0, rotationZ));
            magicCircle = circleGo.GetComponent<MagicCircle>();

            if (magicCircle != null)
            {
                // 💡 modifierColorを使用して魔法陣を表示
                Color color = SpellCommonData.Instance.modifierColor;
                magicCircle.Show(magicCircleDelay, color: color);
                // 魔法陣が出てから少し待ってから発射
                yield return new WaitForSeconds(magicCircleDelay);
            }
        }

        FireNextSpells(wandSpells, currentSpellIndex, rotationZ, strength, context);

        // 消滅演出の開始
        if (magicCircle != null)
        {
            magicCircle.Hide(magicCircleDelay);
        }
    }

    public override void DisplayAimingLine(
        List<SpellBase> wandSpells, int currentSpellIndex,
        float rotationZ, float strength, SpellContext context, bool clearLine = false)
    {
        // 予測線でも位置の変更を反映させる
        float angleRad = rotationZ * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * distance;
        context.CasterPosition += offset;

        base.DisplayAimingLine(wandSpells, currentSpellIndex, rotationZ, strength, context, clearLine);
    }

    int[] nextSpellOffsets = { 1 };
    public override int[] GetNextSpellOffsets(List<SpellBase> wandSpells,
        int currentSpellIndex)
    {
        return nextSpellOffsets;
    }

    public override List<SpellDescriptionItem> GetDescriptionDetails()
    {
        base.GetDescriptionDetails();
        detailItems.Add(new SpellDescriptionItem
        {
            icon = SpellCommonData.Instance.scaleIcon, // 距離に適したアイコンとしてscaleIconを使用
            descriptionText = $"出現距離 : +{distance}",
        });
        return detailItems;
    }
}
