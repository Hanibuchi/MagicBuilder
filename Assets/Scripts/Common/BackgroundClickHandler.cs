using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// 背景などがクリックされた際に、UnityEventを介して外部メソッドを呼び出す共通クラス。
/// </summary>
public class BackgroundClickHandler : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private UnityEvent onClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        // レイキャストが当たったオブジェクトが、このスクリプトがアタッチされているオブジェクト自身であるか確認
        // これにより、子要素（手前のUI）をクリックした際のイベント伝播を無視できる
        if (eventData.pointerCurrentRaycast.gameObject == gameObject)
        {
            onClick?.Invoke();
        }
    }
}
