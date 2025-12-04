using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

// シーン遷移アニメーションを管理するシングルトンクラス
public class SceneTransitionManager : MonoBehaviour
{
    // 外部からアクセスするためのシングルトンインスタンス
    public static SceneTransitionManager Instance { get; private set; }

    // シーン遷移アニメーションを制御するAnimator
    [SerializeField] private Animator transitionAnimator;

    // ロード開始アニメーションのトリガー名 (Inspectorで設定)
    [SerializeField] private string startTransitionTrigger = "StartLoad";

    // ロード終了アニメーションのトリガー名 (Inspectorで設定)
    [SerializeField] private string endTransitionTrigger = "EndLoad";

    // ロード完了時に待機する最小時間 (アニメーションの長さに合わせる)
    [SerializeField] private float minTransitionDuration = 1.0f;

    // 遷移先のシーン名と、最初以外のシーンをAdditiveロードするかどうかを保持
    private string sceneToLoad;

    // シーンの非同期ロード操作
    private AsyncOperation asyncOperation;

    // ロード開始時間
    private float loadStartTime;

    private void Awake()
    {
        // シングルトンパターンの実装
        if (Instance == null)
        {
            Instance = this;
            // シーンを跨いでオブジェクトを維持
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 既にインスタンスが存在する場合は自身を破棄
            Destroy(gameObject);
            return;
        }

        // Animatorが設定されているか確認
        if (transitionAnimator == null)
        {
            Debug.LogError("SceneTransitionManagerにAnimatorが設定されていません。Inspectorで設定してください。");
        }
    }

    /// <summary>
    /// 指定されたシーン名へ遷移を開始する。
    /// 最初のシーンは通常ロード、2番目以降のシーンはAdditiveロードする。
    /// </summary>
    /// <param name="sceneNames">遷移先のシーン名のリスト</param>
    public void LoadScenesWithTransition(string sceneNames)
    {
        if (sceneNames == null)
        {
            Debug.LogError("遷移するシーン名が指定されていません。");
            return;
        }

        // 既にロード中であれば処理を中断
        if (asyncOperation != null)
        {
            Debug.LogWarning("既にシーン遷移が進行中です。");
            return;
        }

        this.sceneToLoad = sceneNames;

        // 遷移開始アニメーションを再生
        if (transitionAnimator != null)
        {
            transitionAnimator.SetTrigger(startTransitionTrigger);
        }

        // ロード処理を開始
        StartCoroutine(LoadScenesAsync());
    }

    // シーンの非同期ロードとアニメーションの完了を待機するコルーチン
    private IEnumerator LoadScenesAsync()
    {
        // アニメーションが再生されるのを少し待つ
        yield return null;

        loadStartTime = Time.time;

        // 最初のシーンの非同期ロード (通常ロード)
        asyncOperation = SceneManager.LoadSceneAsync(sceneToLoad);
        // すぐに遷移しないように設定
        asyncOperation.allowSceneActivation = false;

        // すべてのロード操作が完了するのを待つ (allowSceneActivation=falseなので、読み込み完了で止まる)
        while (asyncOperation.progress < 0.9f)
        {
            // ロードの進捗状況などを表示したい場合はここで処理
            // 例えば、進捗バーを更新するなど
            Debug.Log($"Progress: {asyncOperation.progress}");
            yield return null;
        }

        // **シーンロード完了 + 最小アニメーション時間待機**

        // 最小アニメーション時間を満たしているかチェック
        float elapsedTime = Time.time - loadStartTime;
        float remainingTime = minTransitionDuration - elapsedTime;

        if (remainingTime > 0)
        {
            // 最小アニメーション時間を満たすまで待機
            yield return new WaitForSeconds(remainingTime);
        }

        // **次のシーンへ遷移（アクティブ化）**

        // 最初のシーンをアクティブ化（これにより遷移が実行される）
        // 2番目以降のAdditiveロードシーンも同時にアクティブ化される
        asyncOperation.allowSceneActivation = true;

        // シーンのアクティブ化が完了するのを待つ
        while (!asyncOperation.isDone)
        {
            yield return null;
        }

        // ロードアニメーション終了用アニメーションを再生
        if (transitionAnimator != null)
        {
            transitionAnimator.SetTrigger(endTransitionTrigger);
        }

        // クリーンアップ
        asyncOperation = null;
        this.sceneToLoad = null;
    }
}