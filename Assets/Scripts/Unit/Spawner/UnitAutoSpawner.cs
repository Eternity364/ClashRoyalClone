using UnityEngine;

namespace Units{
    public class UnitAutoSpawner : UnitSpawner
    {
        
        [SerializeField]
        private float spawnRate;
        [SerializeField]
        private GameObject unitPrefab;

        private float timePassed = 0;

        private void Awake()
        {
            timePassed = spawnRate;
        }
        
        void Update() {
            if (UnitSpawner.Instance.SpawningAllowed)
            {
                timePassed += Time.deltaTime;
                if (timePassed > spawnRate)
                {
                    TrySpawn(transform.position, unitPrefab.GetComponent<Unit>().Type, true, false);
                    timePassed = 0;
                }
            }
        }
    }
}
