using UnityEngine;

namespace Pathfinding
{
    public class GridController : MonoBehaviour
    {
        public Vector2Int gridSize;
        public float cellRadius;
        public FlowField currentFlowField;
        public Vector3 origin;
        public float cellPositionAdjustment = 0.5f;

        private void Start()
        {
            InitializeFlowField();
        }

        private void InitializeFlowField()
        {
            currentFlowField = new FlowField(cellRadius, gridSize);
            currentFlowField.CreateGrid(origin);

            if (currentFlowField != null)
            {
                GridDebug.Instance.SetFlowField(currentFlowField);
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                InitializeFlowField();
                currentFlowField.CreateCostField(cellPositionAdjustment);

                Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos) - origin;
                print("worldPos: " + worldPos);
                Cell destinationCell = currentFlowField.GetCellFromWorldPos(worldPos);
                print("Destination Cell: " + destinationCell.gridIndex);
                currentFlowField.CreateIntegrationField(destinationCell);

                currentFlowField.CreateFlowField();
            }
        }
    }
}

