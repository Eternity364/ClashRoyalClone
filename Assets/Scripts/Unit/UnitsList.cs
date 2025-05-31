using System.Collections.Generic;
using UnityEngine;

namespace Units{
    public class UnitsList : MonoBehaviour
    {
        [SerializeField]
        private List<Unit> unitPrefabs = new List<Unit>();
        [SerializeField]
        private List<GameObject> transparentUnitPrefabs = new List<GameObject>();
        
        // Singleton instance
        public static UnitsList Instance { get; private set; }

        public List<Unit> Get() {
            return new List<Unit>(unitPrefabs);
        }

        public GameObject GetTransparent(Unit unit) {
            return transparentUnitPrefabs[unitPrefabs.IndexOf(unit)];
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