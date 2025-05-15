using System.Collections.Generic;
using UnityEngine;


namespace Units{
    public class UnitSpawner : MonoBehaviour
    {
        [SerializeField]
        ClickableArea area;
        [SerializeField]
        UnitButtonPanel panel;
        [SerializeField]
        TargetAcquiring targetAcquiring;
        [SerializeField]
        Transform target;
        [SerializeField]
        Transform parent;
        [SerializeField]
        BulletFactory bulletFactory;
        [SerializeField]
        Color teamColor;
        [SerializeField, Range(0, 1)]
        int team;
        
        void Start() {
            panel.SetOnEndDragEvent(Spawn);
        }

        public void Spawn(Vector3 position, Unit unitType)
        {
            Unit unit = ObjectPool.Instance.GetObject(unitType.gameObject).GetComponent<Unit>();
            unit.transform.SetParent(parent);
            unit.gameObject.SetActive(true);
            position.y = 0;
            unit.transform.localPosition = position;
            unit.Init(target, bulletFactory, team, teamColor);
            targetAcquiring.AddUnit(unit);
        }
    }
}
