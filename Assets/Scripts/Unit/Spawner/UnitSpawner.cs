using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.AI;


namespace Units{
    public enum Sides
    {
        Player,
        Enemy
    }

    public class UnitSpawner : NetworkBehaviour
    {
        [SerializeField]
        UnitButtonPanel panel;
        [SerializeField]
        SpawnParticles spawnParticlesPrefab;
        [SerializeField]
        TargetAcquiring targetAcquiring;
        [SerializeField]
        Transform unitsParent;
        [SerializeField]
        Transform unitCopiesParent;
        [SerializeField]
        private Color playerTeamColor;
        [SerializeField]
        private Color enemyTeamColor;
        [SerializeField, Range(0, 1)]
        protected int team;

        [Serializable]
        public readonly struct SpawnParams : INetworkSerializeByMemcpy
        {
            public readonly Vector3 Position;
            public readonly Units.Type UnitType;
            public readonly bool Spawn;
            public readonly bool PayElixir;
            public readonly int Team;
            public readonly int UnitIndex;

            public SpawnParams(Vector3 position, Units.Type unitType, bool spawn, bool payElixir, int team, int unitIndex = -1)
            {
                Position = position;
                UnitType = unitType;
                Spawn = spawn;
                PayElixir = payElixir;
                Team = team;
                UnitIndex = unitIndex;
            }
        }

        public static UnitSpawner Instance
        {
            get { return _instance; }
            protected set { _instance = value; }
        }
        public Transform UnitsParent => unitsParent;
        public IReadOnlyList<Base> Bases => bases.AsReadOnly();
        public bool SpawningAllowed => spawningAllowed;
        public List<Color> TeamColors => new List<Color> { playerTeamColor, enemyTeamColor };

        protected bool spawningAllowed = false;

        private List<Base> bases = new();
        private GameObject unitCopy;
        private const float delayBeforeSpawn = 1f;
        private static UnitSpawner _instance;
        private int clientCopyIndex = 1;
        private Dictionary<int, GameObject> unitCopies = new();

        protected virtual void Start()
        {
            unitCopiesParent.transform.position = unitsParent.transform.position;
            NetworkManager.Singleton.OnServerStarted += StartSpawning;
            if (this.GetType() != typeof(UnitSpawner))
                return;
            _instance = this;
            NetworkManager.Singleton.OnServerStarted += SpawnBases;
            panel.SetOnDragEvents(CreateUnitCopy, SendSpawnRequest, UpdateUnitCopyPosition);
        }

        public void SendSpawnRequest(SpawnParams spawnParams)
        {
            if (!IsHost)
            {
                if (!spawnParams.Spawn)
                {
                    RemoveCopy();
                    return;
                }
                unitCopy.transform.SetParent(unitsParent);
                spawnParams = new SpawnParams(
                    unitCopy.transform.localPosition,
                    spawnParams.UnitType,
                    spawnParams.Spawn,
                    spawnParams.PayElixir,
                    GetEnemyTeam(),
                    clientCopyIndex);
                TrySpawnUnitServerRpc(spawnParams);
                unitCopies.Add(clientCopyIndex, unitCopy);
                unitCopy = null;
                clientCopyIndex++;
            }
            else
            {
                UnitSpawner.Instance.TrySpawn(spawnParams);
            }
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
                seq.InsertCallback(UnityEngine.Random.Range(minRandom, maxRandom), () =>
                {

                    unit.GetComponent<NetworkTransform>().Interpolate = false;
                    unit.gameObject.SetActive(true);
                    GameObject spawnParticles = ObjectPool.Instance.GetObject(this.spawnParticlesPrefab.gameObject);
                    spawnParticles.GetComponent<SpawnParticles>().StartSpawnAnimation(
                        unit,
                        UnitSpawner.Instance.UnitsParent,
                        () =>
                        {
                            OnSpawnAnimationFinish?.Invoke();
                            spawnable.Release(false);
                            unit.GetComponent<NetworkTransform>().Interpolate = true;
                        },
                        onlyParticles);
                    unit.GetComponent<NetworkUnit>().StartSpawnAnimation();
                });
            });
        }

        public int GetEnemyTeam()
        {
            return team == 0 ? 1 : 0;
        }

        public void RemoveClientCopy(int index)
        {
            if (unitCopies.ContainsKey(index))
            {
                unitCopies[index].GetComponent<ISpawnable>().Release(true);
                unitCopies.Remove(index);
            }
        }

        private void TrySpawn(SpawnParams spawnParams)
        {
            StartCoroutine(TrySpawnCor(spawnParams));
        }

        private void StartSpawning()
        {
            spawningAllowed = true;
            NetworkManager.Singleton.OnServerStarted -= StartSpawning;
        }

        [ServerRpc(RequireOwnership = false)]
        private void TrySpawnUnitServerRpc(SpawnParams spawnParams)
        {
            UnitSpawner.Instance.TrySpawn(spawnParams);
        }

        private void RemoveCopy()
        {
            if (unitCopy != null)
            {
                unitCopy.GetComponent<ISpawnable>().Release(true);
                unitCopy = null;
            }
        }

        private IEnumerator TrySpawnCor(SpawnParams spawnParams)
        {
            GameObject oldUnitCopy = unitCopy;
            unitCopy = null;

            if (!spawnParams.Spawn)
            {
                RemoveCopy();
                yield break;
            }

            UnitData data = UnitsList.Instance.GetByType(spawnParams.UnitType).Data;

            if (spawnParams.PayElixir)
            {
                ElixirManager.Instance.ChangeValue(-data.Cost);
                panel.CreateFieldElixirAnimation(data.Cost);
            }

            yield return new WaitForSeconds(delayBeforeSpawn);

            float baseOffset = UnitsList.Instance.GetByType(spawnParams.UnitType).GetComponent<NavMeshAgent>().baseOffset;
            ISpawnable spawnable = Factory.Instance.Create(spawnParams.Position, spawnParticlesPrefab.YUpOffset + baseOffset, spawnParams.UnitType).GetComponent<ISpawnable>();
            spawnable.PerformActionForEachUnit((unit) =>
            {
                unit.GetComponent<NetworkUnit>().index.Value = spawnParams.UnitIndex;
                if (spawnParams.Team == (int)Sides.Enemy) 
                    unit.transform.localEulerAngles = new Vector3(0, 180, 0);
            });
            spawnable.SetTeamColor(UnitSpawner.Instance.TeamColors[spawnParams.Team]);
            StartSpawnAnimation(spawnable, false, () =>
            {
                spawnable.PerformActionForEachUnit(unit =>
                    {
                        targetAcquiring.AddUnit(unit);
                    });
                spawnable.Init(UnitSpawner.Instance.Bases[spawnParams.Team].transform, spawnParams.Team);
            });

            if (oldUnitCopy != null)
            {
                oldUnitCopy.GetComponent<ISpawnable>().Release(true);
                oldUnitCopy = null;
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
            unitCopy.transform.SetParent(unitCopiesParent);
        }

        private void SpawnBases()
        {
            NetworkManager.Singleton.OnServerStarted -= SpawnBases;

            bases.Insert(team, Factory.Instance.CreateBase(true).GetComponent<Base>());
            bases[team].Init(null, GetEnemyTeam());
            bases[team].SetTeamColor(enemyTeamColor);
            bases[team].gameObject.SetActive(true);

            bases.Insert(GetEnemyTeam(), Factory.Instance.CreateBase(false).GetComponent<Base>());
            bases[GetEnemyTeam()].Init(null, team);
            bases[GetEnemyTeam()].SetTeamColor(playerTeamColor);
            bases[GetEnemyTeam()].gameObject.SetActive(true);

            targetAcquiring.gameObject.SetActive(true);
        }
    }
}
