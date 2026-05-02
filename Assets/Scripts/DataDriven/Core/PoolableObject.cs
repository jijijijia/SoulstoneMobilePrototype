using UnityEngine;

public class PoolableObject : MonoBehaviour
{
    [SerializeField] private GameObject sourcePrefab;

    public GameObject SourcePrefab => sourcePrefab;

    public void SetSourcePrefab(GameObject prefab)
    {
        sourcePrefab = prefab;
    }
}
