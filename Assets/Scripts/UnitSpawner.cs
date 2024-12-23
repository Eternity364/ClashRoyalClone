using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    [SerializeField]
    Dude dudePrefab;
    [SerializeField]
    ClickableArea area;
    [SerializeField]
    Transform target;
    



    void Start() {
        area.Init(Spawn);
    }

    public void Spawn(Vector2 position)
    {
        Dude dude = Instantiate(dudePrefab, transform);
        dude.transform.position = new Vector3(position.x, position.y, 0);
        dude.Init(target);
    }
}
