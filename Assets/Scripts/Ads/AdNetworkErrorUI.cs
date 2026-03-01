using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 広告が読み込めなかった際に、ネットワーク接続を促すUIを制御するクラス
/// </summary>
public class AdNetworkErrorUI : MonoBehaviour
{
    [SerializeField] private Button _retryButton;

    private Action _onRetry;

    private void Awake()
    {
        if (_retryButton != null)
        {
            _retryButton.onClick.AddListener(HandleRetryButtonClick);
        }
    }

    private void OnDestroy()
    {
        if (_retryButton != null)
        {
            _retryButton.onClick.RemoveListener(HandleRetryButtonClick);
        }
    }

    /// <summary>
    /// UIの初期設定
    /// </summary>
    /// <param name="onRetry">再読み込みボタンが押された時に実行する処理</param>
    public void Setup(Action onRetry)
    {
        _onRetry = onRetry;
    }

    private void HandleRetryButtonClick()
    {
        _onRetry?.Invoke();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
