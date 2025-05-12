using System.Collections;
using System.Collections.Generic;
using Units;
using UnityEngine;

namespace Units{
    public class UnitsList : MonoBehaviour
    {
        [SerializeField]
        private List<Unit> unitPrefabs = new List<Unit>();
        
        // Singleton instance
        public static UnitsList Instance { get; private set; }

        public List<Unit> Get() {
            return unitPrefabs;
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