using UnityEngine;

public class MiniSnowman : MonoBehaviour
{
    public float lifetime = 6f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}

