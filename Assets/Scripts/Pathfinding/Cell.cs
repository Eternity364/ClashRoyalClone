using UnityEngine;

namespace Pathfinding
{
    public class Cell
    {
        public Vector3 worldPos;
        public Vector2Int gridIndex;

        public Cell(Vector3 worldPos, Vector2Int gridIndex)
        {
            this.worldPos = worldPos;
            this.gridIndex = gridIndex;
        }
    }
}
