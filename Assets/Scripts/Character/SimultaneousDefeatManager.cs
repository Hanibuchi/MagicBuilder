using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 複数の敵を同時に倒さないと復活する仕組みを管理するクラス。
/// インスペクタから設定した敵のグループが全滅した際、TeleportManagerに通知します。
/// </summary>
public class SimultaneousDefeatManager : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnInfo
    {
        [Tooltip("敵のプレハブ")]
        public GameObject prefab;
        [Tooltip("出現させる位置のリスト")]
        public Transform[] spawnPoints;
    }

    [System.Serializable]
    public class EnemyGroup
    {
        [Tooltip("グループの識別用ID（空欄の場合はStageIDが使用されます）")]
        public string id;
        [Tooltip("TeleportManagerに登録するStageID")]
        public string stageId;
        [Tooltip("一人が倒れてから全員倒すまでの猶予時間。これを超えると復活します。")]
        public float reviveDelay = 5.0f;

        [Tooltip("このグループに含まれる敵の種類ごとの出現設定")]
        public List<EnemySpawnInfo> spawnInfos = new List<EnemySpawnInfo>();

        [Tooltip("グループ全滅時にドロップする呪文のリスト")]
        public SpellBase[] dropSpells;

        // 全ての敵のコントローラーをフラットに保持
        [HideInInspector] public List<MyCharacterController> spawnedControllers = new List<MyCharacterController>();

        // リスポーン（再生成）用に元の情報を保持する内部クラス
        public class SpawnContext
        {
            public GameObject prefab;
            public Transform spawnPoint;
            public int controllerIndex;
        }
        [HideInInspector] public List<SpawnContext> spawnContexts = new List<SpawnContext>();

        [HideInInspector] public bool isCleared = false;
        [HideInInspector] public float firstDeathTime = -1f;

        public string DisplayId => string.IsNullOrEmpty(id) ? stageId : id;
    }

    [SerializeField] private List<EnemyGroup> groups = new List<EnemyGroup>();

    [Header("再生成演出設定")]
    [Tooltip("復活・再スポーン時に再生するSE")]
    [SerializeField] private AudioClip respawnSE;
    [Tooltip("復活・再スポーン時に生成するエフェクトなどのプレハブ")]
    [SerializeField] private GameObject respawnEffectPrefab;
    [Tooltip("演出用オブジェクトを削除するまでの時間")]
    [SerializeField] private float effectDestroyDelay = 2.0f;

    private void Start()
    {
        foreach (var group in groups)
        {
            // グループ全体で1体の敵としてTeleportManagerに登録
            if (TeleportManager.Instance != null && !string.IsNullOrEmpty(group.stageId))
            {
                TeleportManager.Instance.RegisterEnemy(group.stageId);
                Debug.Log($"SimultaneousDefeatManager: Registered group {group.DisplayId} with StageID {group.stageId}");
            }

            // 敵をスポーンさせる
            int controllerIndex = 0;
            foreach (var info in group.spawnInfos)
            {
                if (info.prefab == null) continue;

                foreach (var spawnPoint in info.spawnPoints)
                {
                    if (spawnPoint == null) continue;

                    // 再生成用に情報を保存
                    group.spawnContexts.Add(new EnemyGroup.SpawnContext 
                    { 
                        prefab = info.prefab, 
                        spawnPoint = spawnPoint, 
                        controllerIndex = controllerIndex 
                    });
                    
                    // 初期スポーン（ダミーのnullを入れておいてSpawnEnemy内でセット）
                    group.spawnedControllers.Add(null);
                    SpawnEnemy(group, group.spawnContexts.Count - 1, false); // 初回は演出なし
                    controllerIndex++;
                }
            }
        }
    }

    private void SpawnEnemy(EnemyGroup group, int contextIndex, bool showEffect)
    {
        var context = group.spawnContexts[contextIndex];
        if (context.prefab == null || context.spawnPoint == null) return;

        if (showEffect)
        {
            PlayRespawnEffect(context.spawnPoint.position);
        }

        GameObject go = Instantiate(context.prefab, context.spawnPoint.position, context.spawnPoint.rotation);

        // 個別のEnemyTeleportTriggerが二重登録しないように削除または無効化
        if (go.TryGetComponent<EnemyTeleportTrigger>(out var trigger))
        {
            Destroy(trigger);
        }

        if (go.TryGetComponent<MyCharacterController>(out var controller))
        {
            group.spawnedControllers[context.controllerIndex] = controller;
        }
        else
        {
            Debug.LogWarning($"SimultaneousDefeatManager: Prefab {context.prefab.name} does not have MyCharacterController.");
        }
    }

    private void PlayRespawnEffect(Vector3 position)
    {
        if (SoundManager.Instance != null && respawnSE != null)
        {
            SoundManager.Instance.PlaySE(respawnSE);
        }

        if (respawnEffectPrefab != null)
        {
            GameObject effect = Instantiate(respawnEffectPrefab, position, Quaternion.identity);
            Destroy(effect, effectDestroyDelay);
        }
    }

    private void Update()
    {
        foreach (var group in groups)
        {
            if (group.isCleared) continue;

            int deadCount = 0;
            foreach (var controller in group.spawnedControllers)
            {
                // オブジェクトが削除されている、またはIsDeadがtrueなら死亡とみなす
                if (controller == null || controller.GetComponent<CharacterHealth>().IsDead)
                {
                    deadCount++;
                }
            }

            // 全員生存（かつ全員存在）している場合はタイマーをリセット
            if (deadCount == 0)
            {
                group.firstDeathTime = -1f;
                continue;
            }

            // 全員撃破した場合
            if (deadCount == group.spawnedControllers.Count)
            {
                group.isCleared = true;
                if (TeleportManager.Instance != null && !string.IsNullOrEmpty(group.stageId))
                {
                    TeleportManager.Instance.UnregisterEnemy(group.stageId);
                }

                // 呪文のドロップ処理
                if (group.dropSpells != null && group.dropSpells.Length > 0 && SpellDropManager.Instance != null)
                {
                    // 代表として一つ目の出現ポイントをドロップ位置とする
                    Vector3 dropPos = transform.position;
                    if (group.spawnContexts.Count > 0 && group.spawnContexts[0].spawnPoint != null)
                    {
                        dropPos = group.spawnContexts[0].spawnPoint.position;
                    }

                    foreach (var spell in group.dropSpells)
                    {
                        if (spell != null)
                        {
                            SpellDropManager.Instance.DropSpell(dropPos, spell);
                        }
                    }
                }

                Debug.Log($"SimultaneousDefeatManager: Group {group.DisplayId} (StageID: {group.stageId}) CLEARED!");
            }
            // 一部が撃破されている場合（復活判定）
            else
            {
                if (group.firstDeathTime < 0)
                {
                    group.firstDeathTime = Time.time;
                }

                // 猶予時間を超えたら復活
                if (Time.time - group.firstDeathTime > group.reviveDelay)
                {
                    Debug.Log($"SimultaneousDefeatManager: Group {group.DisplayId} failed to clear in time. Reviving...");
                    for (int i = 0; i < group.spawnedControllers.Count; i++)
                    {
                        var controller = group.spawnedControllers[i];
                        if (controller == null)
                        {
                            // オブジェクトが削除されている場合は対応するコンテキストから再生成
                            int contextIdx = group.spawnContexts.FindIndex(c => c.controllerIndex == i);
                            if (contextIdx >= 0)
                            {
                                SpawnEnemy(group, contextIdx, true);
                            }
                        }
                        else if (controller.GetComponent<CharacterHealth>().IsDead)
                        {
                            // 死んでいるオブジェクトが残っている場合も演出を再生して復活
                            PlayRespawnEffect(controller.transform.position);
                            controller.Revive();
                        }
                    }
                    group.firstDeathTime = -1f;
                }
            }
        }
    }
}
