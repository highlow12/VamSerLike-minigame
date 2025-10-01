using UnityEngine;
using UnityEngine.Pool;

public class PoolAble : MonoBehaviour
{
    public IObjectPool<GameObject> Pool { get; set; }

    public void ReleaseObject()
    {
        if (!gameObject.activeInHierarchy)
            return;
        Pool.Release(gameObject);
    }
}