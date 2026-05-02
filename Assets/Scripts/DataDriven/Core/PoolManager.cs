using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PoolManager : MonoBehaviour
{
    private static PoolManager instance;
    private static bool isApplicationQuitting;
    private static bool isSceneShuttingDown;

    private readonly Dictionary<int, Queue<GameObject>> pools = new();
    private readonly Dictionary<int, Transform> poolRoots = new();
    private readonly HashSet<GameObject> activeInstances = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        isApplicationQuitting = false;
        isSceneShuttingDown = false;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        DefaultRuntimePrefabFactory.ResetCachedPrefabs();
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isSceneShuttingDown = false;
    }

    public static PoolManager Instance
    {
        get
        {
            if (isApplicationQuitting || isSceneShuttingDown)
            {
                return null;
            }

            if (instance == null)
            {
                GameObject poolManagerObject = new("PoolManager");
                instance = poolManagerObject.AddComponent<PoolManager>();
            }

            return instance;
        }
    }

    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        PoolManager manager = Instance;
        return manager != null ? manager.InternalSpawn(prefab, position, rotation, parent) : null;
    }

    public static void Release(GameObject instanceObject)
    {
        if (instanceObject == null)
        {
            return;
        }

        PoolManager manager = Instance;

        if (manager == null)
        {
            DestroyWithoutPooling(instanceObject);
            return;
        }

        manager.InternalRelease(instanceObject);
    }

    private GameObject InternalSpawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (prefab == null)
        {
            return null;
        }

        int prefabId = prefab.GetInstanceID();

        if (!pools.TryGetValue(prefabId, out Queue<GameObject> pool))
        {
            pool = new Queue<GameObject>();
            pools[prefabId] = pool;
        }

        GameObject instanceObject = null;

        while (pool.Count > 0 && instanceObject == null)
        {
            instanceObject = pool.Dequeue();
        }

        if (instanceObject == null)
        {
            instanceObject = Instantiate(prefab, position, rotation, parent);
            instanceObject.SetActive(false);
            PoolableObject poolableObject = instanceObject.GetComponent<PoolableObject>();

            if (poolableObject == null)
            {
                poolableObject = instanceObject.AddComponent<PoolableObject>();
            }

            poolableObject.SetSourcePrefab(prefab);
        }

        NotifyPoolables(instanceObject, true);
        instanceObject.transform.SetParent(parent, false);
        instanceObject.transform.SetPositionAndRotation(position, rotation);
        instanceObject.SetActive(true);
        activeInstances.Add(instanceObject);

        return instanceObject;
    }

    private void InternalRelease(GameObject instanceObject)
    {
        if (instanceObject == null)
        {
            return;
        }

        PoolableObject poolableObject = instanceObject.GetComponent<PoolableObject>();

        if (poolableObject == null || poolableObject.SourcePrefab == null)
        {
            Destroy(instanceObject);
            return;
        }

        int prefabId = poolableObject.SourcePrefab.GetInstanceID();

        if (!pools.TryGetValue(prefabId, out Queue<GameObject> pool))
        {
            pool = new Queue<GameObject>();
            pools[prefabId] = pool;
        }

        if (!poolRoots.TryGetValue(prefabId, out Transform root))
        {
            GameObject rootObject = new($"{poolableObject.SourcePrefab.name}_Pool");
            rootObject.transform.SetParent(transform);
            root = rootObject.transform;
            poolRoots[prefabId] = root;
        }

        instanceObject.SetActive(false);
        NotifyPoolables(instanceObject, false);
        activeInstances.Remove(instanceObject);
        instanceObject.transform.SetParent(root);
        pool.Enqueue(instanceObject);
    }

    private void OnDestroy()
    {
        isSceneShuttingDown = true;
        instance = null;
        ClearAllPooledObjects();
    }

    private void OnApplicationQuit()
    {
        isApplicationQuitting = true;
        ClearAllPooledObjects();
    }

    private void ClearAllPooledObjects()
    {
        foreach (GameObject activeInstance in activeInstances)
        {
            if (activeInstance == null)
            {
                continue;
            }

            DestroySafely(activeInstance);
        }

        activeInstances.Clear();

        foreach (Queue<GameObject> pool in pools.Values)
        {
            while (pool.Count > 0)
            {
                GameObject pooledObject = pool.Dequeue();

                if (pooledObject != null)
                {
                    DestroySafely(pooledObject);
                }
            }
        }

        pools.Clear();

        foreach (Transform root in poolRoots.Values)
        {
            if (root != null)
            {
                DestroySafely(root.gameObject);
            }
        }

        poolRoots.Clear();
    }

    private static void DestroySafely(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(target);
        }
        else
        {
            DestroyImmediate(target);
        }
    }

    private static void NotifyPoolables(GameObject instanceObject, bool takenFromPool)
    {
        MonoBehaviour[] behaviours = instanceObject.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is not IPoolable poolable)
            {
                continue;
            }

            if (takenFromPool)
            {
                poolable.OnTakenFromPool();
            }
            else
            {
                poolable.OnReturnedToPool();
            }
        }
    }

    private static void DestroyWithoutPooling(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(target);
        }
        else
        {
            DestroyImmediate(target);
        }
    }
}
