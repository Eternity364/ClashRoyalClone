using System.Collections.Generic;
using UnityEngine;


namespace Units{
    public class UnitSpawner : MonoBehaviour
    {
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
        [SerializeField]
        Color teamColor;
        [SerializeField, Range(0, 1)]
        int team;
        
        void Start() {
            area.SetOnClickEvent(Spawn);
        }

        public void Spawn(Vector3 position, int index)
        {
            if (index < 0 || index >= UnitsList.Instance.Get().Count) {
                Debug.LogError("Invalid index for unit prefab.");
                return;
            }
            Unit unit = ObjectPool.Instance.GetObject(UnitsList.Instance.Get()[index].gameObject).GetComponent<Unit>();
            unit.transform.SetParent(parent);
            unit.gameObject.SetActive(true);
            position.y = 0;
            unit.transform.localPosition = position;
            unit.Init(target, bulletFactory, team, teamColor);
            targetAcquiring.AddUnit(unit);
        }
    }
}
