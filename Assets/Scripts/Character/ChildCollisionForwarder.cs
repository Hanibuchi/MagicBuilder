// ChildCollisionForwarder.cs (子オブジェクトにアタッチ)
using UnityEngine;

public class ChildCollisionForwarder : MonoBehaviour
{
    // 親オブジェクトの CharacterHealth コンポーネントを格納する変数
    [SerializeField] CharacterHealth _healthComponent;

    // Start はコンポーネントが有効化された最初のフレームで呼び出されます
    private void Start()
    {
        if (_healthComponent == null)
        {
            Debug.LogError("親または自身に CharacterHealth コンポーネントが見つかりませんでした。Collisionイベントを転送できません。", this);
        }
    }

    // --- 衝突イベント（Is TriggerがOFFの場合） ---

    // 衝突が始まったときに一度だけ呼び出される
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // CharacterHealth が存在する場合にのみ、そのメソッドを呼び出す
        if (_healthComponent != null)
        {
            _healthComponent.OnCollisionEnter2D(collision);
        }
    }

    // 衝突している間、フレームごとに呼び出される
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (_healthComponent != null)
        {
            _healthComponent.OnCollisionStay2D(collision);
        }
    }

    // --- トリガーイベント（Is TriggerがONの場合） ---

    // トリガーへの侵入が始まったときに一度だけ呼び出される
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_healthComponent != null)
        {
            _healthComponent.OnTriggerEnter2D(other);
        }
    }

    // トリガー内に留まっている間、フレームごとに呼び出される
    private void OnTriggerStay2D(Collider2D other)
    {
        if (_healthComponent != null)
        {
            _healthComponent.OnTriggerStay2D(other);
        }
    }
}