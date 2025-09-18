using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Pathfinding
{
    public class GridDebug : MonoBehaviour
    {
        public static GridDebug Instance { get; private set; }

        public enum DisplayMode
        {
            Costs,
            IntegrationField,
            FlowField
        }

        // choose what to display in the cells
        public DisplayMode displayMode = DisplayMode.Costs;

        public Transform arrowPrefab;
        public Transform crossPrefab;
        public Transform circlePrefab;
        [Min(0.01f)]
        public float markerScale = 1f;

        public bool displayGrid = true;
        public bool displayCosts = true;
        public Color drawColor = Color.white;
        public Color costColor = Color.black;
        [Range(0.001f, 0.1f)]
        public float lineThickness = 0.02f;
        public int costFontSize = 12;
        public Transform markersParent;

        private FlowField currentFlowField;
        private Material lineMaterial;

        // cache of labels produced during GL pass and consumed in OnGUI
        private readonly List<(Vector3 worldPos, string text)> costLabels = new List<(Vector3, string)>();
        private GUIStyle costStyle;

        // flow field visualization markers
        private readonly List<Transform> flowFieldMarkers = new List<Transform>();

        // avoid spamming logs
        private bool loggedNoPrefabs = false;
        private bool loggedMissingFlowField = false;
         // caching to avoid rebuilding every frame
         private object lastDestinationCell = null;
         private DisplayMode lastDisplayMode;
 
         private void Awake()
         {
             Instance = this;

             Shader shader = Shader.Find("Hidden/Internal-Colored");
             lineMaterial = new Material(shader);
             lineMaterial.hideFlags = HideFlags.HideAndDontSave;
             lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
             lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
             lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
             lineMaterial.SetInt("_ZWrite", 0);

             costStyle = new GUIStyle();
             costStyle.normal.textColor = costColor;
             costStyle.alignment = TextAnchor.MiddleCenter;
             costStyle.fontSize = costFontSize;

             lastDisplayMode = displayMode;
         }

        public void SetFlowField(FlowField flowField)
        {
            currentFlowField = flowField;
            // force rebuild on new flow field
            lastDestinationCell = null;
            // reset missing-flowfield log so we can warn again if it's cleared later
            loggedMissingFlowField = false;
        }
 
         private void OnRenderObject()
         {
            // currentFlowField must be provided via SetFlowField. if missing, warn once and clear markers.
            if (currentFlowField == null || currentFlowField.grid == null)
            {
                if (displayMode == DisplayMode.FlowField && !loggedMissingFlowField)
                {
                    Debug.LogWarning("[GridDebug] currentFlowField is null or has no grid. Call SetFlowField(flowField) to enable FlowField display.");
                    loggedMissingFlowField = true;
                }
                return;
            }
 
             // draw grid only if requested
            if (displayGrid)
                DrawGrid(currentFlowField.gridSize, drawColor, currentFlowField.cellRadius);
 
            // get current destination cell (try common names via reflection)
            var currentDest = GetCurrentDestinationCell();

            // If mode changed, clear caches so next blocks will rebuild
            bool modeChanged = lastDisplayMode != displayMode;
            if (modeChanged)
            {
                lastDestinationCell = null; // force rebuild on next section
            }

            // FlowField markers: rebuild only when destination cell changed or mode switched to FlowField
            if (displayMode == DisplayMode.FlowField)
            {
                if (modeChanged || !ReferenceEquals(currentDest, lastDestinationCell))
                {
                    costLabels.Clear();

                    TryDisplayFlowField();
                    lastDestinationCell = currentDest;
                    lastDisplayMode = displayMode;
                }
            }
            // Costs/Integration: rebuild labels only when destination changed or mode switched
            else if (displayCosts)
            {
                if (modeChanged || !ReferenceEquals(currentDest, lastDestinationCell))
                {
                    costLabels.Clear();
                    ClearFlowFieldMarkers();

                    TryCollectCosts();
                    lastDestinationCell = currentDest;
                    lastDisplayMode = displayMode;
                }
            }
        }

        // attempt to find a "destination" cell object on the FlowField via common names
        private object GetCurrentDestinationCell()
        {
            if (currentFlowField == null) return null;
            var t = currentFlowField.GetType();
            string[] candidateNames = new[] { "destinationCell", "destination", "targetCell", "goalCell", "endCell" };
            foreach (var name in candidateNames)
            {
                var f = t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (f != null) return f.GetValue(currentFlowField);
                var p = t.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null) return p.GetValue(currentFlowField);
            }
            return null;
        }

        private void DrawGrid(Vector2Int gridSize, Color color, float cellRadius)
        {
            if (!lineMaterial) return;

            lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);

            GL.Begin(GL.QUADS);
            GL.Color(color);

            float cellSize = cellRadius * 2f;
            Vector3 bottomLeft = currentFlowField.grid[0, 0].worldPos;

            int width = gridSize.x;
            int height = gridSize.y;

            // Vertical lines
            for (int x = 0; x <= width; x++)
            {
                float xPos = bottomLeft.x + x * cellSize;
                Vector3 start = new Vector3(xPos, bottomLeft.y, bottomLeft.z);
                Vector3 end = new Vector3(xPos, bottomLeft.y, bottomLeft.z + height * cellSize);
                DrawQuadLine(start, end, lineThickness);
            }

            // Horizontal lines
            for (int y = 0; y <= height; y++)
            {
                float zPos = bottomLeft.z + y * cellSize;
                Vector3 start = new Vector3(bottomLeft.x, bottomLeft.y, zPos);
                Vector3 end = new Vector3(bottomLeft.x + width * cellSize, bottomLeft.y, zPos);
                DrawQuadLine(start, end, lineThickness);
            }

            GL.End();
            GL.PopMatrix();
        }

        private void TryCollectCosts()
        {
            if (currentFlowField == null || currentFlowField.grid == null)
                return;

            int width = currentFlowField.gridSize.x;
            int height = currentFlowField.gridSize.y;
            float cellRadius = currentFlowField.cellRadius;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = currentFlowField.grid[x, y];
                    string text = "-";
                    if (cell != null)
                    {
                        switch (displayMode)
                        {
                            case DisplayMode.Costs:
                                text = cell.cost.ToString();
                                break;
                            case DisplayMode.IntegrationField:
                                // show bestCost (integration field); display "-" for unreachable
                                text = cell.bestCost == ushort.MaxValue ? "-" : cell.bestCost.ToString();
                                break;
                        }
                    }

                    // store center of the cell (worldPos is bottom-left corner)
                    Vector3 centerPos = cell.worldPos + new Vector3(cellRadius, 0f, cellRadius);
                    costLabels.Add((centerPos, text));
                }
            }
        }

        private void TryDisplayFlowField()
        {
            if (currentFlowField == null || currentFlowField.grid == null)
                return;

            // require at least one prefab to render anything
            if (arrowPrefab == null && circlePrefab == null && crossPrefab == null)
            {
                if (!loggedNoPrefabs)
                {
                    Debug.LogWarning("[GridDebug] FlowField display enabled but no marker prefabs assigned (arrowPrefab/circlePrefab/crossPrefab).");
                    loggedNoPrefabs = true;
                }
                return;
            }

            // clear previous markers
            ClearFlowFieldMarkers();

            int width = currentFlowField.gridSize.x;
            int height = currentFlowField.gridSize.y;
            float cellRadius = currentFlowField.cellRadius;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = currentFlowField.grid[x, y];
                    if (cell == null) continue;

                    // center of the cell (slightly above ground to avoid z-fighting)
                    Vector3 centerPos = cell.worldPos + new Vector3(cellRadius, 0.05f, cellRadius);

                    Transform prefabToUse = null;
                    if (cell.cost == 0)
                        prefabToUse = circlePrefab;
                    else if (cell.cost == byte.MaxValue)
                        prefabToUse = crossPrefab;
                    else
                        prefabToUse = arrowPrefab;

                    if (prefabToUse == null) continue;

                    var inst = Instantiate(prefabToUse, centerPos, Quaternion.identity, markersParent);
                    inst.gameObject.hideFlags = HideFlags.DontSave;
                    inst.localScale = Vector3.one * markerScale;
                    
                    // rotate arrow according to bestDirection (0 -> East).
                    if (prefabToUse == arrowPrefab)
                    {
                        float angle = cell.bestDirection.GetAngle();
                        inst.localEulerAngles = new Vector3(inst.localEulerAngles.x, -angle, inst.localEulerAngles.z);
                    }
                    else
                    {
                        inst.localEulerAngles = new Vector3(90f, 0f, 0f);
                    }
                    
                    flowFieldMarkers.Add(inst);
                }
            }
        }

        private void ClearFlowFieldMarkers()
        {
            for (int i = 0; i < flowFieldMarkers.Count; i++)
            {
                var t = flowFieldMarkers[i];
                if (t == null) continue;

                if (Application.isPlaying)
                    Destroy(t.gameObject);
                else
                    DestroyImmediate(t.gameObject);
            }
            flowFieldMarkers.Clear();
        }

        private void DrawQuadLine(Vector3 start, Vector3 end, float thickness)
        {
            // Find a perpendicular vector on the XZ plane
            Vector3 dir = (end - start).normalized;
            Vector3 offset = Vector3.Cross(dir, Vector3.up) * (thickness * 0.5f);

            Vector3 v1 = start - offset;
            Vector3 v2 = start + offset;
            Vector3 v3 = end + offset;
            Vector3 v4 = end - offset;

            GL.Vertex(v1);
            GL.Vertex(v2);
            GL.Vertex(v3);
            GL.Vertex(v4);
        }

        private void OnGUI()
        {
            if ((displayMode != DisplayMode.FlowField) && (!displayCosts || currentFlowField == null || currentFlowField.grid == null || costLabels.Count == 0))
                return;

            Camera cam = Camera.current ?? Camera.main;
            if (cam == null) return;

            // update style color/size if changed in inspector at runtime
            costStyle.normal.textColor = costColor;
            costStyle.fontSize = costFontSize;

            foreach (var item in costLabels)
            {
                Vector3 screen = cam.WorldToScreenPoint(item.worldPos + Vector3.up * 0.05f); // slight offset above ground
                if (screen.z < 0) continue; // behind camera
                // convert to GUI coords (y flipped)
                Vector2 guiPos = new Vector2(screen.x, Screen.height - screen.y);
                var rect = new Rect(guiPos.x - 25, guiPos.y - 8, 50, 16);
                GUI.Label(rect, item.text, costStyle);
            }
        }
    }
}