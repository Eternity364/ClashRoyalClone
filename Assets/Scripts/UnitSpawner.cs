using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    [SerializeField]
    Unit unitPrefab;
    [SerializeField]
    ClickableArea area;
    [SerializeField]
    TargetAcquiring targetAcquiring;
    [SerializeField]
    Transform target;
    [SerializeField]
    Transform parent;
    



    void Start() {
        area.Init(Spawn);
    }

    public void Spawn(Vector2 position)
    {
        Unit unit = Instantiate(unitPrefab, parent);
        unit.transform.position = new Vector3(position.x, position.y, 0);
        unit.Init(target);
        targetAcquiring.AddUnit(unit);
    }
}
