using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private readonly GameObject prefab;
    private readonly Transform root;
    private readonly Queue<GameObject> inactive = new Queue<GameObject>();

    public ObjectPool(GameObject prefab, Transform root, int prewarm)
    {
        this.prefab = prefab;
        this.root = root;

        for (int i = 0; i < prewarm; i++)
        {
            GameObject go = CreateInstance();
            go.SetActive(false);
            inactive.Enqueue(go);
        }
    }

    private GameObject CreateInstance()
    {
        GameObject go = Object.Instantiate(prefab, root);

        PooledObject pooled = go.GetComponent<PooledObject>();
        if (pooled == null)
        {
            pooled = go.AddComponent<PooledObject>();
        }
        pooled.Pool = this;

        return go;
    }

    public GameObject Get(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject go = inactive.Count > 0 ? inactive.Dequeue() : CreateInstance();

        Transform t = go.transform;
        t.SetParent(parent != null ? parent : root, false);
        t.SetPositionAndRotation(position, rotation);
        go.SetActive(true);

        go.GetComponent<PooledObject>().InvokeSpawn();
        return go;
    }

    public void Release(GameObject go)
    {
        go.GetComponent<PooledObject>().InvokeDespawn();
        go.SetActive(false);
        go.transform.SetParent(root, false);
        inactive.Enqueue(go);
    }
}
