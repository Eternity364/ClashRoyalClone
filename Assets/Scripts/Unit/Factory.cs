using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
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
            Transform parent = normiesParent;//unit is Giant ? giantsParent : normiesParent;
            GameObject go = CreateInstance(unit);
            position.y = yUpOffset;
            ISpawnable spawnable = go.GetComponent<ISpawnable>();
            

            go.transform.localPosition = position;
            // In order for units, that are a part of a SpawnGroup, to spawn in correct positions on client side,
            // we need to set their positions before calling Spawn method on them.
            SpawnGroup spawnGroup = spawnable as SpawnGroup;
            // if (spawnGroup != null)
            //     spawnGroup.SetPositionsForUnits();
            if (!isCopy)
                CheckNetwork(spawnable);
            go.transform.SetParent(parent, false);
            if (spawnGroup != null)
                spawnGroup.SetParentForUnits(spawnGroup.transform);
            if (spawnGroup != null)
                spawnGroup.SetPositionsForUnits();

            //StartCoroutine(SetNetworkTransformEnabledCoroutine(spawnable, true));

            spawnable.SetCopyMode(isCopy);
            go.gameObject.SetActive(true);

            return go;
        }

        private GameObject CreateInstance(Unit unitType)
        {
            GameObject prefab = unitType.Spawnable.GetGameObject();
            GameObject go = ObjectPool.Instance.GetObject(prefab);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            return go;
        }

        IEnumerator SetNetworkTransformEnabledCoroutine(ISpawnable spawnable, bool enabled)
        {
            List<NetworkUnit> networkUnits = new List<NetworkUnit>();
            spawnable.PerformActionForEachUnit(unit => {
                NetworkUnit networkUnit = unit.GetComponent<NetworkUnit>();
                if (networkUnit != null)
                    networkUnits.Add(unit.GetComponent<NetworkUnit>());
            });

            yield return new WaitForNextFrameUnit();

            foreach (NetworkUnit netUnit in networkUnits)
                netUnit.SetNetworkTransformEnabledNetworkVar(enabled);
        }

        private void SetNetworkTransformEnabled(ISpawnable spawnable, bool enabled)
        {
            spawnable.PerformActionForEachUnit(unit =>
            {
                NetworkUnit networkUnit = unit.GetComponent<NetworkUnit>();
                if (networkUnit != null)
                    unit.GetComponent<NetworkUnit>().SetNetworkTransformEnabledNetworkVar(enabled);
            });
        }

        private void CheckNetwork(ISpawnable spawnable)
        {
            if (NetworkManager.Singleton.IsListening && NetworkManager.Singleton.IsHost)
            {
                if (spawnable is SpawnGroup spawnGroup)
                    spawnGroup.GetComponent<NetworkObject>().Spawn();
                spawnable.PerformActionForEachUnit(unit => unit.gameObject.GetComponent<NetworkObject>().Spawn());
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
