using UnityEngine;

namespace Pathfinding
{
    public class FlowField
    {
        public Cell[,] grid { get; private set; }
        public Vector2Int gridSize { get; private set; }
        public float cellRadius { get; private set; }

        private float cellDiameter;

        public FlowField(float cellRadius, Vector2Int gridSize)
        {
            this.gridSize = gridSize;
            this.cellRadius = cellRadius;
            cellDiameter = cellRadius * 2f;
        }

        public void CreateGrid(Vector3 origin)
        {
            grid = new Cell[gridSize.x, gridSize.y];

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Vector3 worldPoint = new Vector3(
                        x * cellDiameter + cellRadius + origin.x,
                        origin.y,
                        y * cellDiameter + cellRadius + origin.z
                    );
                    grid[x, y] = new Cell(worldPoint, new Vector2Int(x, y));
                }
            }

            int a = 5;
        }
    }
}
