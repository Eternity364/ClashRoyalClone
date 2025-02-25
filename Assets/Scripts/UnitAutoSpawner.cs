using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAutoSpawner : UnitSpawner
{
    
    [SerializeField]
    private float spawnRate;

    private float timePassed = 0;
    
    void Start() {
    }
    
    void Update() {
        timePassed += Time.deltaTime;
        if (timePassed > spawnRate) {
            Spawn(transform.position);
            timePassed = 0;
        }
    }
}
