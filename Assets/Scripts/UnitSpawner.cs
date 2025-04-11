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
    

    private const int maxNavMeshPriority = 99;
    private int currentNavMeshPriority = -1;

    void Start() {
        area.Init(Spawn);
    }

    public void Spawn(Vector3 position)
    {
        Unit unit = ObjectPool.Instance.GetObject(unitPrefab.gameObject).GetComponent<Unit>();
        unit.transform.SetParent(parent);
        unit.gameObject.SetActive(true);
        position.y = 0;
        unit.transform.localPosition = position;
        unit.Init(target, bulletFactory, GetNavMeshPriority());
        targetAcquiring.AddUnit(unit);
    }

    private int GetNavMeshPriority() {
        currentNavMeshPriority++;
        if (currentNavMeshPriority > maxNavMeshPriority) {
            currentNavMeshPriority = 0;
        }
        return currentNavMeshPriority;
    }
}
