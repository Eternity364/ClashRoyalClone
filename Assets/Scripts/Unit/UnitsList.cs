using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Unit;
using UnityEngine;

namespace Assets.Scripts.Unit {
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

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}