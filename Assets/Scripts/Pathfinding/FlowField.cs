using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class FlowField
    {
        public Cell[,] grid { get; private set; }
        public Vector2Int gridSize { get; private set; }
        public float cellRadius { get; private set; }
        public Cell destinationCell;

        private float cellDiameter;

        public FlowField(float cellRadius, Vector2Int gridSize)
        {
            this.gridSize = gridSize;
            this.cellRadius = cellRadius;
            cellDiameter = cellRadius * 2f;
        }

        public Cell GetCellFromWorldPos(Vector3 worldPos)
        {
            float percentX = (worldPos.x) / (gridSize.x * cellDiameter);
            float percentY = (worldPos.z) / (gridSize.y * cellDiameter);

            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.Clamp(Mathf.FloorToInt((gridSize.x) * percentX), 0, gridSize.x - 1);
            int y = Mathf.Clamp(Mathf.FloorToInt((gridSize.y) * percentY), 0, gridSize.y - 1);
            return grid[x, y];
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
        }

        public void CreateCostField(float positionAdjustment = 0.5f)
        {
            Vector3 cellHalfExtents = Vector3.one * cellRadius;
            int terrainMask = LayerMask.GetMask("Impassable", "RoughTerrain");
            Vector3 positionAdjustmentVector = new Vector3(positionAdjustment, 0, positionAdjustment);
            foreach (Cell curCell in grid)
            {
                Collider[] obstacles = Physics.OverlapBox(curCell.worldPos - positionAdjustmentVector, cellHalfExtents, Quaternion.identity, terrainMask);
                bool hasIncreasedCost = false;
                foreach (Collider col in obstacles)
                {
                    if (col.gameObject.layer == LayerMask.NameToLayer("Impassable"))
                    {
                        curCell.IncreaseCost(255);
                        continue;
                    }
                    else if (!hasIncreasedCost && col.gameObject.layer == LayerMask.NameToLayer("RoughTerrain"))
                    {
                        curCell.IncreaseCost(3);
                        hasIncreasedCost = true;
                    }
                }
            }
        }  

        public void CreateIntegrationField(Cell _destinationCell)
        {
            destinationCell = _destinationCell;

            destinationCell.cost = 0;
            destinationCell.bestCost = 0;

            Queue<Cell> cellsToCheck = new Queue<Cell>();

            cellsToCheck.Enqueue(destinationCell);

            while (cellsToCheck.Count > 0)
            {
                Cell curCell = cellsToCheck.Dequeue();
                List<Cell> curNeighbors = GetNeighborCells(curCell.gridIndex, GridDirection.CardinalDirections);
                foreach (Cell neighbor in curNeighbors)
                {
                    if (neighbor.cost == byte.MaxValue) continue;

                    ushort newCost = (ushort)(curCell.bestCost + neighbor.cost);
                    if (newCost < neighbor.bestCost)
                    {
                        neighbor.bestCost = newCost;
                        cellsToCheck.Enqueue(neighbor);
                    }
                }
            }
        }
        
        public void CreateFlowField()
        {
            foreach (Cell curCell in grid)
            {
                List<Cell> curNeighbors = GetNeighborCells(curCell.gridIndex, GridDirection.AllDirections);
                ushort lowestCost = ushort.MaxValue;
                Vector2Int bestDirection = GridDirection.None;

                foreach (Cell neighbor in curNeighbors)
                {
                    if (neighbor.bestCost < lowestCost)
                    {
                        lowestCost = neighbor.bestCost;
                        bestDirection = neighbor.gridIndex - curCell.gridIndex;
                        curCell.bestDirection = GridDirection.GetDirectionFromV2I(bestDirection);
                    }
                }
            }
        }

        private List<Cell> GetNeighborCells(Vector2Int nodeIndex, List<GridDirection> directions)
        {
            List<Cell> neighbors = new List<Cell>();

            foreach (GridDirection direction in directions)
            {
                Cell neighbor = GetCellAtRelativePos(nodeIndex, direction);
                if (neighbor != null)
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }

        private Cell GetCellAtRelativePos(Vector2Int originPos, Vector2Int relativePos)
        {
            Vector2Int checkPos = originPos + relativePos;

            if (checkPos.x >= 0 && checkPos.x < gridSize.x && checkPos.y >= 0 && checkPos.y < gridSize.y)
            {
                return grid[checkPos.x, checkPos.y];
            }
            return null;
        }
    }
}
