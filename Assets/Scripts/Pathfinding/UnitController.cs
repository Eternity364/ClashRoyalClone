using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class UnitController : MonoBehaviour
    {
        public GridController gridController;
        public GameObject unitPrefab;
        public int numUnitsPerSpawn;
        public float moveSpeed;

        private List<GameObject> unitsInGame;

        private void Awake()
        {
            unitsInGame = new();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SpawnUnits();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                DestroyUnits();
            }
        }

        private void FixedUpdate()
        {
            if (gridController.currentFlowField == null) return;
            foreach (GameObject unit in unitsInGame)
            {
                MoveUnit(unit);
            }
        }

        private void MoveUnit(GameObject unit)
        {
            Cell nodeBelow = gridController.currentFlowField.GetCellFromWorldPos(unit.transform.position - gridController.origin);
            Vector3 moveDirection = new Vector3(nodeBelow.bestDirection.Vector.x, 0, nodeBelow.bestDirection.Vector.y);
            Rigidbody unitRB = unit.GetComponent<Rigidbody>();
            if (nodeBelow == null || nodeBelow.bestDirection == GridDirection.None) unitRB.velocity = Vector3.zero;
            else
            {
                unitRB.velocity = moveDirection * moveSpeed;
            }
        }

        private void SpawnUnits()
        {
            Vector2Int gridSize = gridController.gridSize;
            float nodeRadius = gridController.cellRadius;
            Vector2 maxSpawnPos = new Vector2(gridSize.x * nodeRadius * 2 + nodeRadius, gridSize.y * nodeRadius * 2 + nodeRadius);
            int colMask = LayerMask.GetMask("Impassable", "Units");
            Vector3 newPos;
            for (int i = 0; i < numUnitsPerSpawn; i++)
            {
                GameObject newUnit = Instantiate(unitPrefab);
                newUnit.transform.parent = this.transform;
                do
                {
                    newPos = new Vector3(Random.Range(0, maxSpawnPos.x), 0, Random.Range(0, maxSpawnPos.y)) + gridController.origin;
                } while (Physics.OverlapSphere(newPos, 0.25f, colMask).Length > 0);
                newUnit.transform.position = newPos;
                unitsInGame.Add(newUnit);
            }
        }

        private void DestroyUnits()
        {
            foreach (GameObject unit in unitsInGame)
            {
                Destroy(unit);
            }
            unitsInGame.Clear();
        }
    }
}
