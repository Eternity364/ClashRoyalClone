using UnityEngine;

namespace Assets.Scripts.Unit {
    public class UnitAutoSpawner : UnitSpawner
    {
        
        [SerializeField]
        private float spawnRate;
        [SerializeField]
        private int index;

        private float timePassed = 0;
        
        void Start() {
            timePassed = spawnRate;
        }
        
        void Update() {
            timePassed += Time.deltaTime;
            if (timePassed > spawnRate) {
                Spawn(transform.position, index);
                timePassed = 0;
            }
        }
    }
}
