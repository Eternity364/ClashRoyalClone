using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Units
{
    public class Factory : MonoBehaviour
    {

        [SerializeField]
        Transform normiesParent;
        [SerializeField]
        Transform giantsParent;

        // Singleton instance
        public static Factory Instance { get; private set; }

        public GameObject Create(Vector3 position, float yUpOffset, Type unitType, bool isCopy = false)
        {
            Unit unit = UnitsList.Instance.Get(!isCopy)[(int)unitType];
            Transform parent = unit is Giant ? giantsParent : normiesParent;
            GameObject go = CreateInstance(unit, isCopy);
            position.y = parent.position.y + yUpOffset;
            go.transform.position = position;
            ISpawnable spawnable = go.GetComponent<ISpawnable>();
            CheckNetwork(spawnable, isCopy);
            spawnable.SetCopyMode(isCopy);
            go.transform.SetParent(parent);
            go.gameObject.SetActive(true);


            return go;
        }

        private GameObject CreateInstance(Unit unitType, bool isCopy)
        {
            GameObject prefab = unitType.Spawnable.GetGameObject();
            return ObjectPool.Instance.GetObject(prefab);
        }

        private void CheckNetwork(ISpawnable spawnable, bool isCopy)
        {
            if (NetworkManager.Singleton.IsListening && !isCopy)
            {
                if (NetworkManager.Singleton.IsHost)
                {
                    spawnable.PerformActionForEachUnit(unit => unit.gameObject.GetComponent<NetworkObject>().Spawn());
                }
                else if (NetworkManager.Singleton.IsClient)
                {
                    spawnable.PerformActionForEachUnit(unit => unit.gameObject.GetComponent<Unit>().enabled = false);
                    spawnable.PerformActionForEachUnit(unit => unit.gameObject.GetComponent<NavMeshAgent>().enabled = false);
                    spawnable.PerformActionForEachUnit(unit => unit.gameObject.GetComponent<NavMeshObstacle>().enabled = false);
                }
            }
        }
        
        private void Awake()
        {
            // Ensure only one instance of UnitsList exists
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning("Multiple UnitsList instances detected. Destroying duplicate.");
                Destroy(gameObject);
            }
        }
    }
}
