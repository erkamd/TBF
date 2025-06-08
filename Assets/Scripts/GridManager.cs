using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    public int width = 15;
    public int height = 40;
    [Tooltip("Number of cells that make up the goal mouth across the width")] public int goalWidth = 3;
    public float cellSize = 1f;
    public GameObject cellPrefab; // Assign this in the Inspector
    public GameObject gridCanvas; // Assign this in the Inspector


    private int GoalStartY => Mathf.Max(0, (height - goalWidth) / 2);
    private int GoalEndY => Mathf.Min(height - 1, GoalStartY + goalWidth - 1);

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Create a parent GameObject for all cells under the canvas
        var gridParent = new GameObject("Grid");
        gridParent.transform.SetParent(gridCanvas.transform, false);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject cell;
                if (cellPrefab != null)
                {
                    cell = Instantiate(cellPrefab);
                }
                else
                {
                    cell = GameObject.CreatePrimitive(PrimitiveType.Quad);
                }

                cell.transform.SetParent(gridParent.transform, false);
                cell.name = $"Cell_{x}_{y}";

                // Assign gridPosition if GridCell component exists
                var gridCell = cell.GetComponent<GridCell>();
                if (gridCell != null)
                {
                    gridCell.gridPosition = new Vector2Int(x, y);
                }

                // For UI, set RectTransform position
                var rectTransform = cell.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = new Vector2(x * cellSize, y * cellSize);
                    rectTransform.sizeDelta = new Vector2(cellSize, cellSize);
                }
                else
                {
                    // Fallback for non-UI prefab
                    cell.transform.position = CellToWorld(new Vector2Int(x, y));
                    cell.transform.localScale = new Vector3(cellSize, cellSize, 1f);
                    cell.transform.rotation = Quaternion.Euler(0, 0, 0);
                }
            }
        }

        // Goal cells one cell outside the pitch
        for (int y = GoalStartY; y <= GoalEndY; y++)
        {
            CreateGoalCell(gridParent.transform, -1, y, "LeftGoal");
            CreateGoalCell(gridParent.transform, width, y, "RightGoal");
        }
    }

    private void CreateGoalCell(Transform parent, int xIndex, int yIndex, string name)
    {
        GameObject cell;
        if (cellPrefab != null)
            cell = Instantiate(cellPrefab);
        else
            cell = GameObject.CreatePrimitive(PrimitiveType.Quad);

        cell.transform.SetParent(parent, false);
        cell.name = $"{name}_{xIndex}";

        var gridCell = cell.GetComponent<GridCell>();
        if (gridCell != null)
            gridCell.gridPosition = new Vector2Int(xIndex, yIndex);

        var rect = cell.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = new Vector2(xIndex * cellSize, yIndex * cellSize);
            rect.sizeDelta = new Vector2(cellSize, cellSize);
        }
        else
        {
            cell.transform.position = CellToWorld(new Vector2Int(xIndex, yIndex));
            cell.transform.localScale = new Vector3(cellSize, cellSize, 1f);
            cell.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        var renderer = cell.GetComponent<MeshRenderer>();
        if (renderer != null)
            renderer.material.color = Color.green;
    }

    public Vector3 CellToWorld(Vector2Int cell)
    {
        return new Vector3(cell.x * cellSize, cell.y * cellSize);
    }

    public bool IsGoalCell(Vector2Int cell, out int side)
    {
        side = 0;
        if (cell.y >= GoalStartY && cell.y <= GoalEndY)
        {
            if (cell.x == -1)
            {
                side = -1;
                return true;
            }
            if (cell.x == width)
            {
                side = 1;
                return true;
            }
        }
        return false;
    }
}
