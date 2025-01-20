using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellPool : MonoBehaviour
{
    #region Private Fields
    private Queue<GameObject> inactiveObjects;
    private HashSet<GameObject> activeObjects;
    private GameObject cellPrefab;
    private Transform cellContainer;
    private int defaultPoolSize;
    private int maxPoolSize;
    private bool isInitialized = false;
    #endregion

    #region Properties
    public int ActiveCount => activeObjects?.Count ?? 0;
    public int InactiveCount => inactiveObjects?.Count ?? 0;
    public int TotalCount => ActiveCount + InactiveCount;
    public float PoolUsagePercent => activeObjects != null ? (float)ActiveCount / TotalCount * 100f : 0f;
    public bool IsReady => isInitialized;
    #endregion

    #region Public Methods
    public void Initialize(GameObject _prefab, Transform _container, int _gridSize)
    {
        cellPrefab = _prefab;
        cellContainer = _container;

        // Calcul adaptatif de la taille du pool
        int baseSize = 100;
        float percentage = Mathf.Lerp(0.25f, 0.05f, Mathf.InverseLerp(5, 50, _gridSize));
        int calculatedSize = baseSize + Mathf.CeilToInt(Mathf.Pow(_gridSize, 3) * percentage);
        defaultPoolSize = Mathf.Clamp(calculatedSize, 100, 3000);
        maxPoolSize = Mathf.CeilToInt(defaultPoolSize * 1.5f);

        inactiveObjects = new Queue<GameObject>(defaultPoolSize);
        activeObjects = new HashSet<GameObject>();

        StartCoroutine(PrewarmPool());
    }

    public GameObject GetObject(Vector3 _position)
    {
        if (!isInitialized) return null;

        GameObject obj;

        if (inactiveObjects.Count == 0)
        {
            if (TotalCount >= maxPoolSize)
            {
                // Extension dynamique du pool si nécessaire
                maxPoolSize = Mathf.Min(maxPoolSize + 100, defaultPoolSize * 2);
                if (TotalCount >= maxPoolSize)
                {
                    Debug.LogWarning($"Pool has reached absolute max size of {maxPoolSize}");
                    return null;
                }
            }

            obj = CreateNewObject();
        }
        else
        {
            obj = inactiveObjects.Dequeue();
        }

        obj.transform.position = _position;
        obj.SetActive(true);
        activeObjects.Add(obj);

        return obj;
    }

    public void ReturnObject(GameObject _obj)
    {
        if (_obj == null || !isInitialized) return;

        if (!activeObjects.Contains(_obj))
        {
            Debug.LogError("Trying to return an object that isn't from this pool!");
            return;
        }

        _obj.SetActive(false);
        activeObjects.Remove(_obj);
        inactiveObjects.Enqueue(_obj);
    }

    public void ReturnAllObjects()
    {
        if (!isInitialized) return;

        var objectsToReturn = new List<GameObject>(activeObjects);
        foreach (var obj in objectsToReturn)
        {
            ReturnObject(obj);
        }
    }

    public void ClearPool()
    {
        ReturnAllObjects();

        inactiveObjects?.Clear();
        activeObjects?.Clear();

        if (cellContainer != null)
        {
            while (cellContainer.childCount > 0)
            {
                DestroyImmediate(cellContainer.GetChild(0).gameObject);
            }
        }

        isInitialized = false;
    }
    #endregion

    #region Private Methods
    private IEnumerator PrewarmPool()
    {
        int batchSize = Mathf.CeilToInt(defaultPoolSize * 0.1f);
        batchSize = Mathf.Clamp(batchSize, 10, 50);

        int created = 0;
        while (created < defaultPoolSize)
        {
            for (int i = 0; i < batchSize && created < defaultPoolSize; i++)
            {
                inactiveObjects.Enqueue(CreateNewObject());
                created++;
            }
            yield return null;
        }

        isInitialized = true;
        Debug.Log($"Pool initialized with {created} objects. Batch size was {batchSize}");
    }

    private GameObject CreateNewObject()
    {
        GameObject obj = Instantiate(cellPrefab, cellContainer);
        obj.SetActive(false);
        return obj;
    }
    #endregion
}