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
    [SerializeField]
    BulletFactory bulletFactory;
    



    void Start() {
        area.Init(Spawn);
    }

    public void Spawn(Vector3 position)
    {
        Unit unit = Instantiate(unitPrefab, parent);
        position.y = 0;
        unit.transform.localPosition = position;
        unit.Init(target, bulletFactory);
        //targetAcquiring.AddUnit(unit);
    }
}
