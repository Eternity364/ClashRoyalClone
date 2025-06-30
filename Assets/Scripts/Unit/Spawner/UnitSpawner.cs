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

            if (!spawn && oldUnitCopy != null)
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

            ISpawnable spawnable = CreateUnit(position, unitType).GetComponent<ISpawnable>();
            spawnable.SetTeamColor(teamColor);
            StartSpawnAnimation(spawnable, () =>
            {
                spawnable.PerformActionForEachUnit(unit =>
                {
                    targetAcquiring.AddUnit(unit);
                });
                spawnable.Init(target, bulletFactory, team);
            });
            if(oldUnitCopy != null)
            {
                ObjectPool.Instance.ReturnObject(oldUnitCopy);
            }
        }

        private void UpdateUnitCopyPosition(Vector3 position)
        {
            if (unitCopy != null)
            {
                position.y = unitCopy.GetComponent<ISpawnable>().baseOffset;
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
            GameObject prefab = unitType.Spawnable.GetGameObject();
            GameObject go = ObjectPool.Instance.GetObject(prefab);
            Transform parent = unitType is Giant ? giantsParent : normiesParent;
            go.transform.SetParent(parent);
            go.gameObject.SetActive(true);
            position.y = 0;
            go.transform.localPosition = position;
            ISpawnable spawnable = go.GetComponent<ISpawnable>();
            spawnable.SetCopyMode(isCopy);

            return go;
        }

        private void StartSpawnAnimation(ISpawnable spawnable, TweenCallback OnSpawnAnimationFinish = null)
        {
            Sequence seq = DOTween.Sequence();
            float minRandom = 0.1f;
            float maxRandom = 0.6f;
            if (spawnable is Unit)
            {
                minRandom = 0;
                maxRandom = 0;
            } else if (spawnable is SpawnGroup)
            {
                SpawnGroup group = spawnable as SpawnGroup;
                group.SetParentForUnits(group.transform.parent);
            }
            spawnable.PerformActionForEachUnit(unit =>
            {
                unit.gameObject.SetActive(false || spawnable is not SpawnGroup);
                seq.InsertCallback(Random.Range(minRandom, maxRandom), () =>
                {
                    unit.gameObject.SetActive(true);
                    GameObject spawnParticles = ObjectPool.Instance.GetObject(this.spawnParticlesPrefab.gameObject);
                    spawnParticles.GetComponent<SpawnParticles>().StartSpawnAnimation(unit, normiesParent, OnSpawnAnimationFinish);
                });
            });
        }
    }
}
