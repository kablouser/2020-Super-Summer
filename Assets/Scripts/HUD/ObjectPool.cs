using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public GameObject prefab;
    [SerializeField] public List<GameObject> pool;

    public GameObject GenerateObject(Transform newParent, int siblingIndex)
    {
        GameObject returnObject;

        if (0 < pool.Count)
        {
            int index = pool.Count - 1;
            returnObject = pool[index];
            pool.RemoveAt(index);        
        }
        else
            returnObject = Instantiate(prefab);

        returnObject.transform.SetParent(newParent, false);
        returnObject.transform.SetSiblingIndex(siblingIndex);
        returnObject.SetActive(true);

        return returnObject;
    }

    public void RemoveObject(GameObject removeObject)
    {
        removeObject.SetActive(false);
        removeObject.transform.SetParent(transform, false);
        pool.Add(removeObject);
    }
}
