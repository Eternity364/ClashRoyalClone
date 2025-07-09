using UnityEngine;

namespace Units{
    public class UnitAutoSpawner : UnitSpawner
    {
        
        [SerializeField]
        private float spawnRate;
        [SerializeField]
        private GameObject unitPrefab;

        private float timePassed = 0;
        
        void Start() {
            timePassed = spawnRate;
        }
        
        void Update() {
            timePassed += Time.deltaTime;
            if (timePassed > spawnRate) {
                TrySpawn(transform.position, unitPrefab.GetComponent<Unit>().Type, true, false);
                timePassed = 0;
            }
        }
    }
}
