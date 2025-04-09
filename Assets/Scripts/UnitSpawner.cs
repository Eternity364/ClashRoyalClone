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
        Unit unit = Instantiate(unitPrefab, parent);
        position.y = 0;
        unit.transform.localPosition = position;
        unit.Init(target, bulletFactory, GetNavMeshPriority());
        //targetAcquiring.AddUnit(unit);
        StartCoroutine(nameof(AddUnitToTargetAcquiring), unit);
    }

    private IEnumerator AddUnitToTargetAcquiring(Unit unit) {
        yield return new WaitForSeconds(0.01f);
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
