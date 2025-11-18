using UnityEngine;

public class ProjectileEffectDestroyer : MonoBehaviour
{
    public void Destroy()
    {
        if (Application.isPlaying)
            Destroy(gameObject);
    }
}
