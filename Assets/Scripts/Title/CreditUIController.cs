using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class CreditUIController : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("表示するクレジット情報を含むScriptableObject")]
    public CreditData creditData;

    [Header("UI要素")]
    [Tooltip("クレジットテキストを表示するTextMeshProUGUI")]
    public TextMeshProUGUI creditText;

    [Tooltip("UIを閉じるために使用するボタン")]
    public Button closeButton;

    [Tooltip("UIオブジェクトを制御するAnimator (非表示アニメーションを含む)")]
    public Animator animator;

    // ボタンが既に押されたかどうかを追跡
    private bool isClosing = false;

    // アニメーターの非表示トリガー名
    private const string CLOSE_TRIGGER = "Close";

    // 呼び出し元がクローズを感知するためのアクション
    private Action onUICloseAction;

    /// <summary>
    /// UIの初期設定を行い、閉じられたときに実行されるコールバックを設定します。
    /// </summary>
    /// <param name="onClosed">UIオブジェクトが破棄されたときに呼び出されるアクション</param>
    public void Init(Action onClosed)
    {
        // 閉じられた後のコールバックを設定
        onUICloseAction = onClosed;
    }

    void Start()
    {
        // 1. 文字列をTextMeshProUGUIに設定
        if (creditData != null && creditText != null)
        {
            creditText.text = creditData.creditTextContent;
        }
        else
        {
            Debug.LogError("CreditData または CreditText が設定されていません。", this);
        }

        // 2. 閉じるボタンにリスナーを設定
        if (closeButton != null)
        {
            // ボタンがクリックされたらCloseUIを呼び出す
            closeButton.onClick.AddListener(CloseUI);
        }
        else
        {
            Debug.LogError("CloseButton が設定されていません。", this);
        }
    }

    /// <summary>
    /// UIを閉じるプロセスを開始します。
    /// </summary>
    public void CloseUI()
    {
        // ボタンが一度も押されていないことを確認
        if (isClosing) return;

        isClosing = true;

        // ボタンの操作を無効にする（再クリック防止）
        closeButton.interactable = false;

        // 3. アニメーションを再生
        if (animator != null)
        {
            animator.SetTrigger(CLOSE_TRIGGER);
        }
        else
        {
            // Animatorがない場合は即座に破棄
            DestroyObject();
        }
    }

    /// <summary>
    /// アニメーションの最後に呼び出され、このオブジェクトを破棄します。
    /// </summary>
    // ※このメソッドはアニメーションクリップのイベントとして設定する必要があります※
    public void DestroyObject()
    {
        // Nullチェックを行い、設定されていれば実行
        onUICloseAction?.Invoke();
        Destroy(this.gameObject);
    }
}