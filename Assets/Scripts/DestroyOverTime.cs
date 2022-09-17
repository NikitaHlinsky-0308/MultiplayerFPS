using UnityEngine;

public class DestroyOverTime : MonoBehaviour
{

    [SerializeField] private float timeToDestroy = 1.5f;
    void Start()
    {
        Destroy(gameObject, timeToDestroy);
    }
}
