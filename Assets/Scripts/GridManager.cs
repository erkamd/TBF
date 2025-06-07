using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    public int rows = 15;
    public int columns = 40;
    [Tooltip("Number of rows that make up the goal mouth")] public int goalWidth = 3;
    public float cellSize = 1f;
    public GameObject cellPrefab; // Assign this in the Inspector
    public GameObject gridCanvas; // Assign this in the Inspector

    private int GoalStartRow => Mathf.Max(0, (rows - goalWidth) / 2);
    private int GoalEndRow => Mathf.Min(rows - 1, GoalStartRow + goalWidth - 1);

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Create a parent GameObject for all cells under the canvas
        var gridParent = new GameObject("Grid");
        gridParent.transform.SetParent(gridCanvas.transform, false);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
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
                cell.name = $"Cell_{r}_{c}";

                // Assign gridPosition if GridCell component exists
                var gridCell = cell.GetComponent<GridCell>();
                if (gridCell != null)
                {
                    gridCell.gridPosition = new Vector2Int(r, c);
                }

                // For UI, set RectTransform position
                var rectTransform = cell.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = new Vector2(r * cellSize, c * cellSize);
                    rectTransform.sizeDelta = new Vector2(cellSize, cellSize);
                }
                else
                {
                    // Fallback for non-UI prefab
                    cell.transform.position = CellToWorld(new Vector2Int(r, c));
                    cell.transform.localScale = new Vector3(cellSize, cellSize, 1f);
                    cell.transform.rotation = Quaternion.Euler(0, 0, 0);
                }
            }
        }

        // Goal cells one column outside the pitch
        for (int r = GoalStartRow; r <= GoalEndRow; r++)
        {
            CreateGoalCell(gridParent.transform, r, -1, "LeftGoal");
            CreateGoalCell(gridParent.transform, r, columns, "RightGoal");
        }
    }

    private void CreateGoalCell(Transform parent, int row, int column, string name)
    {
        GameObject cell;
        if (cellPrefab != null)
            cell = Instantiate(cellPrefab);
        else
            cell = GameObject.CreatePrimitive(PrimitiveType.Quad);

        cell.transform.SetParent(parent, false);
        cell.name = $"{name}_{row}";

        var gridCell = cell.GetComponent<GridCell>();
        if (gridCell != null)
            gridCell.gridPosition = new Vector2Int(row, column);

        var rect = cell.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = new Vector2(row * cellSize, column * cellSize);
            rect.sizeDelta = new Vector2(cellSize, cellSize);
        }
        else
        {
            cell.transform.position = CellToWorld(new Vector2Int(row, column));
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
        if (cell.x >= GoalStartRow && cell.x <= GoalEndRow)
        {
            if (cell.y == -1)
            {
                side = -1;
                return true;
            }
            if (cell.y == columns)
            {
                side = 1;
                return true;
            }
        }
        return false;
    }
}
