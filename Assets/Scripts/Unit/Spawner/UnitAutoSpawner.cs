using UnityEngine;

namespace Units{
    public class UnitAutoSpawner : UnitSpawner
    {
        
        [SerializeField]
        private float spawnRate;
        [SerializeField]
        private GameObject unitPrefab;

        private float timePassed = 0;

        protected override void StartSpawning()
        {
            base.StartSpawning();
            timePassed = spawnRate;
        }
        
        void Update() {
            if (spawningAllowed)
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
