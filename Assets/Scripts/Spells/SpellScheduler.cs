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
    public static SpellScheduler Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // シーンをまたいでも破棄されないように設定することが多い
            // DontDestroyOnLoad(gameObject); 
        }
        else
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