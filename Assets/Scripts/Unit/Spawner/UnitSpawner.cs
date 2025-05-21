using System.Collections.Generic;
using UnityEngine;


namespace Units{
    public class UnitSpawner : MonoBehaviour
    {
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

        private GameObject unitCopy;

        void Start()
        {
            panel.SetOnDragEvents(CreateUnitCopy, TrySpawn, UpdateUnitCopyPosition);
        }

        public void TrySpawn(Vector3 position, Unit unitType, bool spawn)
        {
            if (spawn)
            {
                Unit unit = CreateUnit(position, unitType).GetComponent<Unit>();
                unit.Init(target, bulletFactory, team, teamColor);
                targetAcquiring.AddUnit(unit);
            }

            if (unitCopy == null)
            {
                return;
            }

            ObjectPool.Instance.ReturnObject(unitCopy);
            unitCopy = null;
        }

        private void UpdateUnitCopyPosition(Vector3 position)
        {
            if (unitCopy != null)
            {
                position.y = 0;
                unitCopy.transform.localPosition = position;
            }
        }

        private void CreateUnitCopy(Vector3 position, Unit unitType)
        {
            if (unitCopy != null)
            {
                return;
            }
            unitCopy = CreateUnit(position, unitType, true);
            Unit unit = unitCopy.GetComponent<Unit>();
            unit.SetAlpha(0.5f);
        }

        private GameObject CreateUnit(Vector3 position, Unit unitType, bool isCopy = false)
        {
            GameObject prefab = unitType.gameObject;
            if (isCopy)
            {
                prefab = UnitsList.Instance.GetTransparent(unitType);
            }
            GameObject unit = ObjectPool.Instance.GetObject(prefab);
            unit.transform.SetParent(parent);
            unit.gameObject.SetActive(true);
            position.y = 0;
            unit.transform.localPosition = position;
            unit.GetComponent<Unit>().SetTeamColor(teamColor);

            return unit;
        }
    }
}
