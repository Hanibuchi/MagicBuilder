using UnityEngine;

public class ProjectileEffectDestroyer : MonoBehaviour
{
    [SerializeField] GameObject obj;
    public void Destroy()
    {
        if (Application.isPlaying)
        {
            if (obj != null)
                Destroy(obj);
            else
                Destroy(gameObject);
        }
    }
}
