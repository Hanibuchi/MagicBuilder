using UnityEngine;
using UnityEngine.UI;

public class BackgroundScroller : MonoBehaviour
{
    // スクロールさせる2つのUI要素をインスペクタから設定
    [SerializeField]
    private RectTransform background1;
    
    [SerializeField]
    private RectTransform background2;

    // スクロール速度 (例: 50.0f)
    [SerializeField]
    private float scrollSpeed = 50.0f; 

    // 背景UIの幅
    private float backgroundWidth;

    void Start()
    {
        // 画面幅（=背景UIの幅）を取得
        backgroundWidth = background1.rect.width;

        // 【重要】
        // background1とbackground2のピボットが(0.5, 0.5) (中央)
        // アンカーが Stretch, Stretch で、Left/Right/Top/Bottom が全て 0 であることを前提とします。

        // background2をbackground1の真横（右）に配置する初期設定を保証
        // background2のX座標を背景1の幅（画面幅）に設定。
        // ピボットが中央なので、X座標 backgroundWidth は画面外の右端になります。
        background2.anchoredPosition = new Vector2(backgroundWidth, 0);
    }

    void Update()
    {
        // 1. 移動処理
        Vector2 moveVector = Vector2.left * scrollSpeed * Time.deltaTime;
        
        background1.anchoredPosition += moveVector;
        background2.anchoredPosition += moveVector;

        // 2. ループ処理 (判定タイミングの修正)
        
        // パネルが完全に画面外（左側）に出たかの判定を行います。
        //
        // ピボットが中央(0.5, 0.5)の場合:
        // パネルの左端のローカルX座標は background.anchoredPosition.x - (backgroundWidth / 2)
        // 画面の左端のX座標は、親の RectTransform の左端、つまりグローバルで X=0 です。
        // RectTransform におけるローカル座標系では、画面の中心が X=0 です。
        // パネルが完全に画面外に出るのは、ピボットの位置が -backgroundWidth/2 よりも左、
        // つまり background.anchoredPosition.x <= -backgroundWidth の時です。
        // ※画面の左端は X = -backgroundWidth / 2 です。

        // background1が完全に画面外（左側）に出たら、background2の右隣に移動させる
        if (background1.anchoredPosition.x <= -backgroundWidth)
        {
            // background1 を background2 の真横（右）へ再配置
            // background2の現在のX座標 + backgroundWidth の位置へ移動させる
            // このテレポートにより、background1は画面の右外側で待機します。
            float newX = background2.anchoredPosition.x + backgroundWidth;
            background1.anchoredPosition = new Vector2(newX, 0);
        }

        // background2についても同様の処理
        if (background2.anchoredPosition.x <= -backgroundWidth)
        {
            // background2 を background1 の真横（右）へ再配置
            float newX = background1.anchoredPosition.x + backgroundWidth;
            background2.anchoredPosition = new Vector2(newX, 0);
        }
    }
}