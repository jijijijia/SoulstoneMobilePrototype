using UnityEngine;

public class PoolableObject : MonoBehaviour
{
    [SerializeField] private GameObject sourcePrefab;
    private IPoolable[] cachedPoolables;
    private bool cacheDirty = true;

    public GameObject SourcePrefab => sourcePrefab;

    public void SetSourcePrefab(GameObject prefab)
    {
        sourcePrefab = prefab;
        MarkPoolableCacheDirty();
    }

    public void MarkPoolableCacheDirty()
    {
        cacheDirty = true;
    }

    public IPoolable[] GetPoolables()
    {
        if (!cacheDirty && cachedPoolables != null)
        {
            return cachedPoolables;
        }

        MonoBehaviour[] behaviours = GetComponentsInChildren<MonoBehaviour>(true);
        int count = 0;

        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IPoolable)
            {
                count++;
            }
        }

        cachedPoolables = new IPoolable[count];
        int writeIndex = 0;

        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IPoolable poolable)
            {
                cachedPoolables[writeIndex] = poolable;
                writeIndex++;
            }
        }

        cacheDirty = false;
        return cachedPoolables;
    }
}
