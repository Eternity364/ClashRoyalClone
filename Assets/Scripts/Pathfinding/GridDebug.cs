using UnityEngine;

namespace Pathfinding
{
    public class GridDebug : MonoBehaviour
    {
        public static GridDebug Instance { get; private set; }

        public bool displayGrid = true;
        public Color drawColor = Color.white;

        private FlowField currentFlowField;

        private void Awake()
        {
            Instance = this;
        }

        public void SetFlowField(FlowField flowField)
        {
            currentFlowField = flowField;
        }   

        private void OnDrawGizmos()
        {
            if (displayGrid && currentFlowField != null)
            {
                DrawGrid(currentFlowField.gridSize, drawColor, currentFlowField.cellRadius);
            }
        }

        private void DrawGrid(Vector2Int drawGridSize, Color drawColor, float drawCellRadius)
        {
            if (currentFlowField == null || currentFlowField.grid == null)
                return;

            Gizmos.color = drawColor;
            
            Vector3 bottomLeft = currentFlowField.grid[0, 0].worldPos;
            bottomLeft.y = 0;
            for (int x = 0; x < currentFlowField.gridSize.x; x++)
            {
                for (int y = 0; y < currentFlowField.gridSize.y; y++)
                {
                    Cell cell = currentFlowField.grid[x, y];
                    Vector3 center = cell.worldPos + bottomLeft;
                    Vector3 size = Vector3.one * drawCellRadius * 2;
                    Gizmos.DrawWireCube(center, size);
                }
            }
        }
    }
}
