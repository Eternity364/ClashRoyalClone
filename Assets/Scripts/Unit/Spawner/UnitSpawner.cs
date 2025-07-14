using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;


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
        Color teamColor;
        [SerializeField, Range(0, 1)]
        int team;

        private GameObject unitCopy;
        private const float delayBeforeSpawn = 1f;
        
        private static UnitSpawner _instance;

        public static UnitSpawner Instance
        {
            get { return _instance; }
            protected set { _instance = value; }
        }

        void Start()
        {
            panel.SetOnDragEvents(CreateUnitCopy, TrySpawn, UpdateUnitCopyPosition);
            if (this.GetType() != typeof(UnitSpawner))
                return;
            _instance = this;
        }

        public void TrySpawn(Vector3 position, Type unitType, bool spawn, bool payElixir)
        {
            StartCoroutine(TrySpawnCor(position, unitType, spawn, payElixir));
        }

        public void StartSpawnAnimation(ISpawnable spawnable, bool onlyParticles = false, TweenCallback OnSpawnAnimationFinish = null)
        {
            Sequence seq = DOTween.Sequence();
            float minRandom = 0.1f;
            float maxRandom = 0.6f;
            SpawnGroup spawnGroup = spawnable as SpawnGroup;
            if (spawnGroup == null)
            {
                minRandom = 0;
                maxRandom = 0;
            }
            else
            {
                spawnGroup.SetParentForUnits(spawnGroup.transform.parent);
            }
            spawnable.PerformActionForEachUnit(unit =>
            {
                unit.gameObject.SetActive(false || spawnable is not SpawnGroup);
                seq.InsertCallback(Random.Range(minRandom, maxRandom), () =>
                {
                    unit.gameObject.SetActive(true);
                    GameObject spawnParticles = ObjectPool.Instance.GetObject(this.spawnParticlesPrefab.gameObject);
                    spawnParticles.GetComponent<SpawnParticles>().StartSpawnAnimation(
                        unit,
                        normiesParent,
                        () =>
                        {
                            OnSpawnAnimationFinish?.Invoke();
                            spawnable.Release(false);
                        },
                        onlyParticles);
                });
            });
        }

        private IEnumerator TrySpawnCor(Vector3 position, Type unitType, bool spawn, bool payElixir)
        {
            GameObject oldUnitCopy = unitCopy;
            unitCopy = null;

            if (!spawn && oldUnitCopy != null)
            {
                oldUnitCopy.GetComponent<ISpawnable>().Release(true);
                oldUnitCopy = null;
                yield break;
            }

            UnitData data = UnitsList.Instance.Get()[(int)unitType].Data;

            if (payElixir)
            {
                ElixirManager.Instance.ChangeValue(-data.Cost);
                panel.CreateFieldElixirAnimation(data.Cost);
            }

            yield return new WaitForSeconds(delayBeforeSpawn);

            // if (NetworkManager.Singleton.IsHost || !NetworkManager.Singleton.IsListening)
            // {

            float baseOffset = UnitsList.Instance.Get()[(int)unitType].GetComponent<NavMeshAgent>().baseOffset;
            ISpawnable spawnable = Factory.Instance.Create(position, spawnParticlesPrefab.YUpOffset + baseOffset, unitType).GetComponent<ISpawnable>();
            spawnable.SetTeamColor(teamColor);
            StartSpawnAnimation(spawnable, false, () =>
            {
                spawnable.PerformActionForEachUnit(unit =>
                    {
                        targetAcquiring.AddUnit(unit);
                    });
                spawnable.Init(target, team);
            });
            // }
            if (oldUnitCopy != null)
            {
                oldUnitCopy.GetComponent<ISpawnable>().Release(true);
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

        private void CreateUnitCopy(Vector3 position, Type unitType)
        {
            if (unitCopy != null)
            {
                return;
            }
            unitCopy = Factory.Instance.Create(position, 0, unitType, true);
        }
    }
}
