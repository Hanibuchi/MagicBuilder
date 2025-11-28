using UnityEngine;

public class DamageSourceBase : MonoBehaviour, IDamageSource
{
    /// <summary>
    /// このダメージ源が持つダメージ情報を取得します。
    /// </summary>
    /// <returns>設定されたDamage構造体。</returns>
    public virtual Damage GetDamage()
    {
        return new();
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy();
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
