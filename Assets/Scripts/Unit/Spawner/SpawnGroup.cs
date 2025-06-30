using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Units
{
    public class SpawnGroup : MonoBehaviour, ISpawnable
    {
        [SerializeField]
        private Unit unitType;
        [SerializeField]
        int count = 1;

        public UnitData Data => unitType.Data;

        private List<Unit> units = new List<Unit>();

        void OnEnable()
        {
            foreach (Transform child in transform)
            {
                child.SetParent(null);
            }
            for (int i = 0; i < count; i++)
            {
                Unit unit = ObjectPool.Instance.GetObject(unitType.gameObject).GetComponent<Unit>();
                units.Add(unit);
                unit.transform.SetParent(transform);
            }
            SpreadOutUnits(7f, 7f);
        }

        void OnDisable()
        {
            foreach (Unit unit in units)
            {
                ObjectPool.Instance.ReturnObject(unit.gameObject);
            }
            units.Clear();
        }

        public void Init(Transform destination, BulletFactory bulletFactory, int team)
        {
            PerformActionForEachUnit((unit) =>
            {
                unit.Init(destination, bulletFactory, team);
            });
            units.Clear();
        }
        
        public void SetParentForUnits(Transform parent)
        {
            PerformActionForEachUnit((unit) =>
            {
                unit.transform.SetParent(parent);
            });
        }

        public void SetTeamColor(Color teamColor)
        {
            PerformActionForEachUnit((unit) =>
            {
                unit.SetTeamColor(teamColor);
            });
        }

        public void SetCopyMode(bool enabled)
        {
            PerformActionForEachUnit((unit) =>
            {
                unit.SetCopyMode(enabled);
            });
        }

        public void PerformActionForEachUnit(Action<Unit> Action)
        {
            foreach (Unit unit in units)
            {
                Action?.Invoke(unit);
            }
        }

        public float baseOffset => units[0].baseOffset;

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public void SpreadOutUnits(float areaWidth, float areaLength, float randomOffset = 0.2f)
        {
            if (units == null || units.Count == 0) return;

            int count = units.Count;
            // Estimate grid size: ensure enough white cells for all units
            int gridSize = 1;
            while ((gridSize * gridSize + 1) / 2 < count) // number of white cells on a checkerboard
                gridSize++;

            float cellWidth = areaWidth / gridSize;
            float cellLength = areaLength / gridSize;

            int placed = 0;
            for (int row = 0; row < gridSize && placed < count; row++)
            {
                for (int col = 0; col < gridSize && placed < count; col++)
                {
                    // Checkerboard: only place on "white" cells (row+col even)
                    if ((row + col) % 2 == 0)
                    {
                        float x = -areaWidth / 2f + col * cellWidth + cellWidth / 2f;
                        float z = -areaLength / 2f + row * cellLength + cellLength / 2f;

                        // Add randomness within a fraction of the cell size
                        float randX = UnityEngine.Random.Range(-cellWidth * randomOffset, cellWidth * randomOffset);
                        float randZ = UnityEngine.Random.Range(-cellLength * randomOffset, cellLength * randomOffset);

                        units[placed].transform.localPosition = new Vector3(x + randX, 0, z + randZ);
                        placed++;
                    }
                }
            }
        }
    }
}
