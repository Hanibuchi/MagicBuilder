// PoolManager.cs
using UnityEngine;
using System.Collections.Generic;
using System;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [Header("プールの設定リスト")]
    public PoolItem[] poolSettings; // Unityエディタで設定

    // PoolTypeごとの実際のプール (Queue) を格納
    private Dictionary<PoolType, Queue<GameObject>> poolDictionary;
    // 現在使用中のオブジェクトを追跡 (返却時の効率化のため)
    private Dictionary<PoolType, List<GameObject>> activeObjectsDictionary;

    private Transform poolParent;

    private void Awake()
    {
        // シングルトン処理
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        poolParent = transform;
        poolDictionary = new Dictionary<PoolType, Queue<GameObject>>();
        activeObjectsDictionary = new Dictionary<PoolType, List<GameObject>>();

        // すべてのプールアイテムを初期化
        InitializePools();
    }

    private void InitializePools()
    {
        foreach (var item in poolSettings)
        {
            // 辞書に新しいキューとアクティブリストを作成
            poolDictionary.Add(item.Type, new Queue<GameObject>());
            activeObjectsDictionary.Add(item.Type, new List<GameObject>());
        }
    }

    // オブジェクトの生成
    private GameObject CreateNewObject(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab, poolParent);
        return obj;
    }

    /// <summary>
    /// プールから指定されたタイプのオブジェクトを取得します。
    /// </summary>
    public GameObject GetFromPool(PoolType type)
    {
        if (!poolDictionary.ContainsKey(type))
        {
            Debug.LogError($"PoolType [{type}] がPoolManagerに設定されていません。");
            return null;
        }

        // 設定からプレハブを取得
        PoolItem item = System.Array.Find(poolSettings, p => p.Type == type);
        if (item == null || item.Prefab == null) return null;

        GameObject obj;
        Queue<GameObject> pool = poolDictionary[type];

        // プールにオブジェクトがあればDequeue、なければ新規生成
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            obj = CreateNewObject(item.Prefab);
        }

        // スケールをプレハブのデフォルトに戻す
        obj.transform.localScale = item.Prefab.transform.localScale;

        obj.SetActive(true);
        activeObjectsDictionary[type].Add(obj); // アクティブリストに追加
        return obj;
    }

    /// <summary>
    /// 使用済みのオブジェクトを指定されたタイプでプールに戻します。
    /// </summary>
    public void ReturnToPool(PoolType type, GameObject obj)
    {
        ReturnToPoolInternal(type, obj);
    }

    private void ReturnToPoolInternal(PoolType type, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(type)) return;

        // アクティブリストから削除
        activeObjectsDictionary[type].Remove(obj);

        obj.SetActive(false);
        obj.transform.SetParent(poolParent);
        poolDictionary[type].Enqueue(obj);
    }

    /// <summary>
    /// 特定のタイプの現在アクティブな全てのオブジェクトをプールに戻します。
    /// </summary>
    public void ReturnAllActiveObjects(PoolType type)
    {
        if (!activeObjectsDictionary.ContainsKey(type)) return;

        // 逆順で処理しないと、リストの操作中にインデックスが狂うため
        List<GameObject> activeList = activeObjectsDictionary[type];
        for (int i = activeList.Count - 1; i >= 0; i--)
        {
            // ReturnToPoolInternalを呼び出すと、activeListから要素が削除される
            ReturnToPoolInternal(type, activeList[i]);
        }
    }
}

[Serializable]
public class PoolItem
{
    [Tooltip("このプールの識別子")]
    public PoolType Type;

    [Tooltip("プールするGameObjectのプレハブ")]
    public GameObject Prefab;
}
public enum PoolType
{
    // 呪文の軌跡表示用
    Trajectory,
}