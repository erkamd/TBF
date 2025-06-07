using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    public int rows = 15;
    public int columns = 40;
    public float cellSize = 1f;
    public GameObject cellPrefab; // Assign this in the Inspector
    public GameObject gridCanvas; // Assign this in the Inspector

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
    }

    public Vector3 CellToWorld(Vector2Int cell)
    {
        return new Vector3(cell.x * cellSize, cell.y * cellSize);
    }
}
