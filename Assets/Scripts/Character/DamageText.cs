using TMPro;
using UnityEngine;

/// <summary>
/// ダメージ表記をアニメーションさせるためのヘルパーコンポーネント。
/// </summary>
public class DamageText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI damageText;
    public void Initialize(float damage, float duration)
    {
        damageText.text = Mathf.RoundToInt(damage).ToString();
        Destroy(gameObject, duration);
    }
}