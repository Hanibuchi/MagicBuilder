using UnityEngine;

public class ChainBuilder : MonoBehaviour
{
    [Tooltip("HingeJoint2Dが付いている鎖のプレハブ")]
    [SerializeField] private GameObject chainPrefab;

    [Tooltip("生成する鎖の数")]
    [SerializeField] private int chainLength = 5;

    [Tooltip("根元となるRigidbody2D")]
    [SerializeField] private Rigidbody2D startBody;

    [Tooltip("先につなげる対象のHingeJoint2D（任意）")]
    [SerializeField] private HingeJoint2D endHinge;

    private void Start()
    {
        BuildChain();
    }

    private void BuildChain()
    {
        if (chainPrefab == null || startBody == null)
        {
            Debug.LogWarning("ChainBuilder: プレハブか根元のRigidbody2Dが設定されていません。");
            return;
        }

        Rigidbody2D previousBody = startBody;

        for (int i = 0; i < chainLength; i++)
        {
            // 鎖のプレハブを生成
            GameObject newChain = Instantiate(chainPrefab, transform);
            HingeJoint2D hinge = newChain.GetComponent<HingeJoint2D>();
            Rigidbody2D currentBody = newChain.GetComponent<Rigidbody2D>();

            if (hinge != null)
            {
                hinge.connectedBody = previousBody;

                // 最初の鎖はConnectedAnchorを(0,0)にする
                if (i == 0)
                {
                    hinge.autoConfigureConnectedAnchor = false;
                    hinge.connectedAnchor = Vector2.zero;
                }
            }
            else
            {
                Debug.LogWarning("ChainBuilder: 生成されたプレハブにHingeJoint2Dがアタッチされていません。");
            }

            // 次の鎖のつなぎ先を、今回生成した鎖のRigidbody2Dにする
            previousBody = currentBody;
        }

        // 先のHingeJoint2Dが指定されている場合、最後の鎖とつなぐ
        if (endHinge != null)
        {
            endHinge.connectedBody = previousBody;
        }
    }
}
