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
            if (Instance.SpawningAllowed)
            {
                timePassed += Time.deltaTime;
                if (timePassed > spawnRate)
                {
                    SpawnParams spawnParams = new SpawnParams(
                        transform.position,
                        unitPrefab.GetComponent<Unit>().Type,
                        true,
                        false,
                        team);
                    Instance.SendSpawnRequest(spawnParams);
                    timePassed = 0;
                }
            }
        }
    }
}
