using UnityEngine;

public class EnemyDestroyer : MonoBehaviour
{
    [SerializeField] GameObject obj;
    public void Destroy()
    {
        if (Application.isPlaying)
        {
            if (obj != null)
                Destroy(obj);
            else
                Debug.LogError("destroy target is null");
        }
    }
}
