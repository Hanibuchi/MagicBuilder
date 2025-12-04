using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI; // Buttonを使用する場合

public class IslandSelector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // --- インスペクタから設定するフィールド ---

    [Header("島の識別子")]
    [Tooltip("この島を一意に識別するための文字列。外部メソッドの引数として使用されます。")]
    public string islandID;

    [Header("アニメーション設定")]
    [Tooltip("この島のAnimatorコンポーネント。")]
    public Animator islandAnimator;

    [Header("選択時アクション")]
    [Tooltip("島が選択されたときに呼び出される外部メソッドを設定します。引数としてこの島のIDが渡されます。")]
    public StringEvent onIslandSelected; // カスタムUnityEventを使用

    [Header("非選択時アクション")]
    [Tooltip("島が非選択状態に戻るときに呼び出される外部メソッドを設定します。")]
    public UnityEvent onIslandDeselected;

    // --- 内部状態 ---

    private bool isSelected = false;

    private string selectTriggerName = "Selected";
    private string normalizedTriggerName = "Normalized";
    // --- 初期化とクリック処理 ---

    private void Start()
    {
        // 必須コンポーネントのチェック（Animatorは必須と仮定）
        if (islandAnimator == null)
        {
            Debug.LogError("IslandSelector: Animatorが設定されていません。このコンポーネントを無効にします。", this);
            enabled = false;
            return;
        }

        // 初期状態としてアニメーターをリセット
        islandAnimator.SetTrigger("Normalized");
    }


    /// <summary>
    /// 島がクリックされたときに呼び出されるメソッド。
    /// ButtonコンポーネントのOnClickイベントから呼び出すことを想定。
    /// </summary>
    public void OnIslandClicked()
    {
        if (!isSelected)
        {
            Select();
        }
        else
        {
            Normalize();
        }
    }

    public void Select()
    {
        // 💡 未選択 -> 選択状態への遷移
        if (isSelected == true) return;

        isSelected = true;

        // 1. アニメーションを「選択状態」へ
        islandAnimator.SetTrigger(selectTriggerName);

        // 2. 外部メソッドの実行（島の識別子を引数に渡す）
        onIslandSelected?.Invoke(islandID);
        CameraInputHandler.Instance.MoveCameraTo(transform.position);

        Debug.Log($"島を選択: {islandID}");
    }

    public void Normalize()
    {
        // 💡 選択 -> 非選択状態への遷移 (もう一度クリックされた場合)
        if (isSelected == false) return;

        isSelected = false;

        // 1. アニメーションを「非選択状態」へ
        islandAnimator.SetTrigger(normalizedTriggerName);

        // 2. 外部メソッドの実行（引数なし）
        onIslandDeselected?.Invoke();

        Debug.Log($"島の選択を解除: {islandID}");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnIslandClicked();
    }
}

// --- カスタムUnityEventの定義 ---

/// <summary>
/// 文字列を引数にとるUnityEventをインスペクタで設定可能にするためのクラス。
/// </summary>
[System.Serializable]
public class StringEvent : UnityEvent<string> { }