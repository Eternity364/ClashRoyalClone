using System.Collections;
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
        private const float delayBeforeSpawn = 1f;

        void Start()
        {
            panel.SetOnDragEvents(CreateUnitCopy, TrySpawn, UpdateUnitCopyPosition);
        }

        public void TrySpawn(Vector3 position, Unit unitType, bool spawn)
        {
            StartCoroutine(TrySpawnCor(position, unitType, spawn));
        }

        private IEnumerator TrySpawnCor(Vector3 position, Unit unitType, bool spawn)
        {
            ElixirManager.Instance.ChangeValue(-unitType.Data.Cost);
            GameObject oldUnitCopy = unitCopy;
            unitCopy = null;

            if (!spawn)
            {
                ObjectPool.Instance.ReturnObject(oldUnitCopy);
                yield break;
            }

            yield return new WaitForSeconds(delayBeforeSpawn);

            if (spawn)
            {
                Unit unit = CreateUnit(position, unitType).GetComponent<Unit>();
                unit.Init(target, bulletFactory, team, teamColor, () => targetAcquiring.AddUnit(unit));
                ObjectPool.Instance.ReturnObject(oldUnitCopy);
            }
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
