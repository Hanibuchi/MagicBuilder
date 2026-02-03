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
        Initialize(Mathf.RoundToInt(damage).ToString(), duration);
    }

    public void Initialize(string text, float duration)
    {
        damageText.text = text;
        Destroy(gameObject, duration);
    }
}