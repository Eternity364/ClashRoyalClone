using System.Collections.Generic;
using UnityEngine;
using Units;
using Unity.Netcode;

public class ObjectPool : MonoBehaviour
{
    // Singleton instance
    public static ObjectPool Instance { get; private set; }

    // Dictionary to hold pools for different object types
    private Dictionary<string, List<GameObject>> pools = new Dictionary<string, List<GameObject>>();

    private void Awake()
    {
        // Ensure only one instance of ObjectPool exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple ObjectPool instances detected. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    // Retrieve an object from the pool
    public GameObject GetObject(GameObject prefab, bool setActive = true)
    {
        string key = prefab.name;

        // Ensure the pool for this prefab exists
        if (!pools.ContainsKey(key))
        {
            pools[key] = new List<GameObject>();
        }

        // Look for an inactive object in the pool
        foreach (GameObject obj in pools[key])
        {
            if (!obj.activeInHierarchy)
            {
                obj.SetActive(setActive);
                return obj;
            }
        }

        // If no inactive object is found, create a new one
        GameObject newObj = Instantiate(prefab);
        newObj.name = prefab.name; // Ensure the name matches the prefab
        pools[key].Add(newObj);
        newObj.SetActive(setActive);
        
        return newObj;
    }

    // Return an object to the pool
    public void ReturnObject(GameObject obj)
    {
        NetworkObject networkObject = obj.GetComponent<NetworkObject>();
        if (obj.GetComponent<NetworkObject>() != null && NetworkManager.Singleton.IsHost)
        {
            networkObject.Despawn();
        }
        obj.SetActive(false);
    }
}