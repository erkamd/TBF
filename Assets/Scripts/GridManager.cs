using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    public int rows = 15;
    public int columns = 40;
    public float cellSize = 1f;

    private void Start()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                var cell = GameObject.CreatePrimitive(PrimitiveType.Quad);
                cell.transform.position = CellToWorld(new Vector2Int(r, c));
                cell.transform.localScale = new Vector3(cellSize, cellSize, 1f);
                cell.transform.rotation = Quaternion.Euler(90, 0, 0);
                cell.name = $"Cell_{r}_{c}";
            }
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    public Vector3 CellToWorld(Vector2Int cell)
    {
        return new Vector3(cell.y * cellSize, 0f, cell.x * cellSize);
    }
}
