using System.Collections;
using DG.Tweening;
using UnityEngine;


namespace Units{
    public class UnitSpawner : MonoBehaviour
    {
        [SerializeField]
        UnitButtonPanel panel;
        [SerializeField]
        SpawnParticles spawnParticlesPrefab;
        [SerializeField]
        TargetAcquiring targetAcquiring;
        [SerializeField]
        Transform target;
        [SerializeField]
        Transform normiesParent;
        [SerializeField]
        Transform giantsParent;
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

        public void TrySpawn(Vector3 position, Unit unitType, bool spawn, bool payElixir)
        {
            StartCoroutine(TrySpawnCor(position, unitType, spawn, payElixir));
        }

        private IEnumerator TrySpawnCor(Vector3 position, Unit unitType, bool spawn, bool payElixir)
        {
            GameObject oldUnitCopy = unitCopy;
            unitCopy = null;

            if (!spawn)
            {
                ObjectPool.Instance.ReturnObject(oldUnitCopy);
                yield break;
            }

            if (payElixir)
            {
                ElixirManager.Instance.ChangeValue(-unitType.Data.Cost);
                panel.CreateFieldElixirAnimation(unitType.Data.Cost);
            }

            yield return new WaitForSeconds(delayBeforeSpawn);

            Unit unit = CreateUnit(position, unitType).GetComponent<Unit>();
            unit.SetTeamColor(teamColor);
            StartSpawnAnimation(unit, () =>
            {
                unit.Init(target, bulletFactory, team, teamColor);
                targetAcquiring.AddUnit(unit);
            });
            ObjectPool.Instance.ReturnObject(oldUnitCopy);
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
        }

        private GameObject CreateUnit(Vector3 position, Unit unitType, bool isCopy = false)
        {
            GameObject prefab = unitType.gameObject;
            GameObject unit = ObjectPool.Instance.GetObject(prefab);
            Transform parent = unitType is Giant ? giantsParent : normiesParent;
            unit.GetComponent<Collider>().enabled = false;
            unit.transform.SetParent(parent);
            unit.gameObject.SetActive(true);
            position.y = 0;
            unit.transform.localPosition = position;
            Unit unitComp = unit.GetComponent<Unit>();
            unitComp.SetTransparent(isCopy);
            if (isCopy)
                unitComp.SetAlpha(0.25f);

            return unit;
        }

        private void StartSpawnAnimation(Unit unit, TweenCallback OnSpawnAnimationFinish = null)
        {
            GameObject spawnParticles = ObjectPool.Instance.GetObject(this.spawnParticlesPrefab.gameObject);
            spawnParticles.GetComponent<SpawnParticles>().StartSpawnAnimation(unit, normiesParent, OnSpawnAnimationFinish);
        }
    }
}
