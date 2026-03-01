using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// SpellBaseから非同期処理（コルーチン）を開始するためのヘルパークラス。
/// シーン内のどこかのGameObjectにアタッチする必要があります。
/// </summary>
public class SpellScheduler : MonoBehaviour
{
    // シングルトンパターンでどこからでもアクセス可能にする
    private static SpellScheduler _instance;
    public static SpellScheduler Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("SpellScheduler");
                _instance = go.AddComponent<SpellScheduler>();
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            // シーンをまたいでも破棄されないように設定することが多い
            // DontDestroyOnLoad(gameObject); 
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 実行したいコルーチンを外部から開始するためのパブリックメソッド
    /// </summary>
    /// <param name="coroutine">実行するIEnumerator</param>
    public Coroutine StartSpellCoroutine(IEnumerator coroutine)
    {
        return StartCoroutine(coroutine);
    }
}