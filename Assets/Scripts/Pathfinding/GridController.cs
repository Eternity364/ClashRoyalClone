using UnityEngine;

namespace Pathfinding
{
    public class GridController : MonoBehaviour
    {
        public Vector2Int gridSize;
        public float cellRadius;
        public FlowField currentFlowField;
        public Transform origin;

        private void Start()
        {
            InitializeFlowField();
        }

        private void InitializeFlowField()
        {
            currentFlowField = new FlowField(cellRadius, gridSize);
            currentFlowField.CreateGrid(origin.position);

            if (currentFlowField != null)
            {
                GridDebug.Instance.SetFlowField(currentFlowField);
            }
        }

        private void Update()
        {
            InitializeFlowField();
        }
    }
}

